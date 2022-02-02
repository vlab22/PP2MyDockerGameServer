using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using shared;
using UnityEngine;
using UnityEngine.Events;

public class GameServer : MonoBehaviour
{
    private List<TcpMessageChannel> _members;

    //stores additional info for a player
    private Dictionary<TcpMessageChannel, PlayerInfo> _playerInfo;

    private Dictionary<TcpMessageChannel, float> _heartBeatClientTime;

    private Dictionary<TcpMessageChannel, string> _checkClientList;

    private float _heartBeatElapsedTime;
    public int port = 55555;
    private const float _TIMEOUT = 3;

    public IntUnityEvent playerCountUpdatedEvent;

    public UnityEvent serverRunningEvent;

    private void Awake()
    {
        _members = new List<TcpMessageChannel>();
        _playerInfo = new Dictionary<TcpMessageChannel, PlayerInfo>();
        _heartBeatClientTime = new Dictionary<TcpMessageChannel, float>();
        _checkClientList = new Dictionary<TcpMessageChannel, string>();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (!int.TryParse(EnvVariablesSet.Instance.PP2_SERVER_PORT, out port) || port < 1)
        {
            port = 55555;
        }
        
        DebugC.Log($"Starting server on port {port}");
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(50);

        yield return null;
        
        serverRunningEvent.Invoke();

        playerCountUpdatedEvent?.Invoke(0);
        
        while (true)
        {
            //check for new members	
            if (listener.Pending())
            {
                //get the waiting client
                DebugC.Log("Accepting new client...");
                TcpClient client = listener.AcceptTcpClient();
                //and wrap the client in an easier to use communication channel
                var channel = new TcpMessageChannel(client);
                //and add it to the login room for further 'processing'

                StartCoroutine(CheckClient(channel));
            }

            ProcessClientValidationMessages();

            RemoveFaultyMembers();
            
            ReceiveAndProcessNetworkMessages();
            
            HeartBeatClients();

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ProcessClientValidationMessages()
    {
        foreach (var kv in _checkClientList)
        {
            var channel = kv.Key;
            while (channel.HasMessage())
            {
                var msg = channel.ReceiveMessage();
                HandleNetworkMessage(msg, channel);
            }
        }
    }

    private IEnumerator CheckClient(TcpMessageChannel channel)
    {
        var code = EnvVariablesSet.Instance.PP2_SERVER_ID;

        _checkClientList.Add(channel, code);
        
        channel.SendMessage(new ValidClientRequest()
        {
            serverCode = code
        });

        if (channel.HasErrors())
        {
            channel.Close();
            _checkClientList.Remove(channel);
            
            DebugC.Warn($"Client {channel.GetRemoteEndPoint().Address}:{channel.GetRemoteEndPoint().Port} kicked, has error");
        }
        
        yield return new WaitForSeconds(10);

        if (!_members.Contains(channel))
        {
            DebugC.Warn($"Client {channel.GetRemoteEndPoint().Address}:{channel.GetRemoteEndPoint().Port} kicked, not respond");
            channel.Close();
            _checkClientList.Remove(channel);
        }
    }
    
    private void CheckClientResponse(TcpMessageChannel pSender, string pSenderCode)
    {
        if (_checkClientList.TryGetValue(pSender, out var code))
        {
            if (code != pSenderCode)
            {
                pSender.Close();
                _checkClientList.Remove(pSender);
            }
            else
            {
                AddClient(pSender);
            }
        }
    }

    private void AddClient(TcpMessageChannel channel)
    {
        int playerId = _playerInfo.Count + 1;
        _playerInfo.Add(channel, new PlayerInfo() { id = playerId, userName = $"player_{playerId}" });

        _members.Add(channel);
        
        DebugC.Warn($"Player connected: {channel.GetRemoteEndPoint().Address}");
        
        playerCountUpdatedEvent?.Invoke(_members.Count);
    }

    private void ReceiveAndProcessNetworkMessages()
    {
        SafeForEach(ReceiveAndProcessNetworkMessagesFromMember);
    }

    /**
     * Get all the messages from a specific member and process them
	 */
    private void ReceiveAndProcessNetworkMessagesFromMember(TcpMessageChannel pMember)
    {
        while (pMember.HasMessage())
        {
            var msg = pMember.ReceiveMessage();
            HandleNetworkMessage(msg, pMember);
        }
    }

    private void HandleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
    {
        switch (pMessage)
        {
            //     case MakeMoveRequest makeMoveRequest:
            //         handleMakeMoveRequest(makeMoveRequest, pSender);
            //         break;
            //     case PlayersInfoRequest playersInfoRequest:
            //         handlePlayersInfoRequest(pSender);
            //         break;
            case WhoAmIRequest whoAmIRequest:
                var whoAmIResponse = new WhoAmIResponse()
                {
                    idInRoom = _members.IndexOf(pSender) + 1,
                    userName = GetPlayerInfo(pSender).userName
                };
                pSender.SendMessage(whoAmIResponse);
                break;
            //     case LeaveGameRequest leaveGameRequest:
            //         //The winner is the player in the room that didn't request to leave
            //         var winnerMember = Members.FirstOrDefault(m => m != pSender);
            //         int winnerId = _server.GetPlayerInfo((winnerMember)).id;
            //         SendWinnerMsgAndPlayerToLobby(winnerId, " by \"Conceding\"");
            //
            //         break;
            case ValidClientResponse validResponse:
                CheckClientResponse(pSender, validResponse.code);
                break;
            
        }
    }

    /**
		 * Iterate over all members and remove the ones that have issues.
		 * Return true if any members were removed.
		 */
    private void RemoveFaultyMembers()
    {
        SafeForEach(CheckFaultyMember);
    }

    /**
		* Iterates backwards through all members and calls the given method on each of them.
		* This basically allows you to process all clients, and optionally remove them 
		* without weird crashes due to collections being modified.
		* 
		* This can happen while looking for faulty clients, or when deciding to move a bunch 
		* of members to a different room, while you are still processing them.
		*/
    private void SafeForEach(Action<TcpMessageChannel> pMethod)
    {
        for (int i = _members.Count - 1; i >= 0; i--)
        {
            //skip any members that have been 'killed' in the mean time
            if (i >= _members.Count) continue;
            //call the method on any still existing member
            pMethod(_members[i]);
        }
    }

    /**
		 * Check if a member is no longer connected or has issues, if so remove it from the room, and close it's connection.
		 */
    private void CheckFaultyMember(TcpMessageChannel pMember)
    {
        if (!pMember.Connected) RemoveAndCloseMember(pMember);
    }

    /**
		 * Removes a member from this room and closes it's connection (basically it is being removed from the server).
		 */
    private void RemoveAndCloseMember(TcpMessageChannel pMember)
    {
        var userName = GetPlayerInfo(pMember)?.userName;

        var chatMsg = new ChatMessage()
        {
            message = $"{userName} disconnected."
        };

        SendToAll(chatMsg);

        ClientDisconnected(pMember);

        RemoveMember(pMember);

        RemovePlayerInfo(pMember);
        pMember.Close();

        DebugC.Warn("Removed client at " + pMember.GetRemoteEndPoint());
    }

    private void HeartBeatClients()
    {
        var now = Time.timeSinceLevelLoad;
        if (_playerInfo.Count > 0 && now - _heartBeatElapsedTime > _TIMEOUT)
        {
            foreach (var kv in _playerInfo)
            {
                var client = kv.Key;
                client.SendMessage(new HeartBeatRequest());
            }

            _heartBeatElapsedTime = Time.timeSinceLevelLoad;

            DebugC.Log($"HeartBeat clients at {now * 0.001f} secs");
        }
        else
        {
            _heartBeatElapsedTime = now - _heartBeatElapsedTime;
        }
    }

    /**
		 * Sends a message to all members in the room.
		 */
    public void SendToAll(ASerializable pMessage)
    {
        foreach (TcpMessageChannel member in _members)
        {
            member.SendMessage(pMessage);
        }
    }

    protected virtual void ClientDisconnected(TcpMessageChannel pChannel)
    {
    }

    protected virtual void RemoveMember(TcpMessageChannel pMember)
    {
        DebugC.Log($"Client left: {pMember.GetRemoteEndPoint().Address}");

        _members.Remove(pMember);
    }

    public void RemovePlayerInfo(TcpMessageChannel pClient)
    {
        _playerInfo.Remove(pClient);
    }

    /**
		 * Returns a handle to the player info for the given client 
		 * (will create new player info if there was no info for the given client yet)
		 */
    public PlayerInfo GetPlayerInfo(TcpMessageChannel pClient)
    {
        if (!_playerInfo.ContainsKey(pClient))
        {
            _playerInfo[pClient] = new PlayerInfo();
        }

        return _playerInfo[pClient];
    }

    [ContextMenu("Debug Clients")]
    public string GetClientsInfo()
    {
        var result = "Clients connected" + Environment.NewLine;
        for (int i = _members.Count - 1; i >= 0; i--)
        {
            var m = _members[i];
            result += $"\t{m.GetRemoteEndPoint()}" + Environment.NewLine;
        }

        return result;
    }

    public void DebugClients()
    {
        DebugC.Log(GetClientsInfo());
    }

    private int GetPortFromCmdArg(int defaultVal)
    {
        var cmdPortStr = GetArg("--port");

        if (string.IsNullOrWhiteSpace(cmdPortStr))
        {
            return defaultVal;
        }

        int cmdPort = -1;

        int.TryParse(cmdPortStr, out cmdPort);

        return cmdPort > -1 ? cmdPort : defaultVal;
    }

    // Helper function for getting the command line arguments
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    public int PlayersCount => _members.Count;
}