using System;
using System.Collections;
using UnityEngine;

public class PlayersCountUpdaterLoop : MonoBehaviour
{
    public GameServer server;
    public float interval = 5f;

    public IntUnityEvent notifyPlayerCount;

    private void Start()
    {
        if (server == null)
            server = FindObjectOfType<GameServer>();

        StartCoroutine(LoopSendUpdatePlayersCount());
    }

    private IEnumerator LoopSendUpdatePlayersCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            notifyPlayerCount?.Invoke(server.PlayersCount);
        }
    }
}