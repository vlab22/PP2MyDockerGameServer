using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UpdateServerData : MonoBehaviour
{
    [SerializeField] private int _serverId;
    [SerializeField] private string _userAuth;
    [SerializeField] private string _userAuthPass;
    [SerializeField] private string _authCode;

    [SerializeField] private string _updatePlayerCounterUrl;
    [SerializeField] private string _updateStatusUrl;


    // Start is called before the first fraSme update
    void Start()
    {
        var idStr = Environment.GetEnvironmentVariable("PP2_SERVER_ID");
        if (!int.TryParse(idStr, out _serverId))
        {
            DebugC.Warn($"ServerId cannot be parsed, incorrect PP2_SERVER_ID var");
        }

        var envSetS = EnvVariablesSet.Instance;

        _userAuth = envSetS.PP2_USER_AUTH;
        _userAuthPass = envSetS.PP2_USER_AUTH_PASS;
        _authCode = envSetS.PP2_AUTH_CODE;
        _updatePlayerCounterUrl = envSetS.PP2_AZ_FC_URL + envSetS.PP2_UPDATE_GAME_SERVER_PLAYER_COUNT_QUERY + $"?code={_authCode}";
        _updateStatusUrl = envSetS.PP2_AZ_FC_URL + envSetS.PP2_UPDATE_GAME_SERVER_STATUS_QUERY + $"?code={_authCode}";
    }

    public void SaveServerStatusData(string newStatus)
    {
        StartCoroutine(SaveServerStatusDataRoutine(newStatus));
    }

    private IEnumerator SaveServerStatusDataRoutine(string newStatus)
    {
        var postData = new StatusPostData()
        {
            user_auth = _userAuth,
            user_pass_auth = _userAuthPass,
            server_id = _serverId,
            status = newStatus
        };

        var json = JsonUtility.ToJson(postData);

        var www = new UnityWebRequest(_updateStatusUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.certificateHandler = new ForceAcceptAll();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugC.Error(
                $"Fail to request to save game server data: {_updateStatusUrl} | data: '{newStatus}' | error: {www.error}");
        }
        else
        {
            DebugC.Log($"Status Uploaded! Data Result: '{www.downloadHandler.text}'");
        }
    }

    public void SaveGameServerPlayerCountData(int playerCount)
    {
        StartCoroutine(SaveGameServerPlayerCounterCoroutine(playerCount));
    }

    public IEnumerator SaveGameServerPlayerCounterCoroutine(int playerCount)
    {
        var postData = new PlayerCounterPostData()
        {
            user_auth = _userAuth,
            user_pass_auth = _userAuthPass,
            server_id = _serverId,
            players_count = playerCount
        };

        var json = JsonUtility.ToJson(postData);

        var www = new UnityWebRequest(_updatePlayerCounterUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.certificateHandler = new ForceAcceptAll();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugC.Error(
                $"Fail to request to save game server data: {_updatePlayerCounterUrl} | data: '{playerCount}' | error: {www.error}");
        }
        else
        {
            DebugC.Log($"Player Counter Uploaded! Data Result: '{www.downloadHandler.text}'");
        }
    }

    [ContextMenu("SaveGameServerPlayerCountData")]
    public void SaveGameServerPlayerCountDataDebug()
    {
        SaveGameServerPlayerCountData(13);
    }
    
    [ContextMenu("SaveGameServerStatusDataDebug")]
    public void SaveGameServerStatusDataDebug()
    {
        SaveServerStatusData("Running");
    }
}