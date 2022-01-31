using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StatusAndPlayersCountUpdaterLoop : MonoBehaviour
{
    public GameServer server;
    public float interval = 5f;

    public UnityEvent notifyServerStatus;
    public IntUnityEvent notifyServerPlayerCounter;


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

            notifyServerPlayerCounter?.Invoke(server.PlayersCount);

            yield return null;
            
            notifyServerStatus?.Invoke();
        }
    }
}