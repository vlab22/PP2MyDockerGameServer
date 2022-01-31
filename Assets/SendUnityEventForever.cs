using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SendUnityEventForever : MonoBehaviour
{
    public UnityEvent evt;

    public float interval = 1.0f;
    public float delay = 0;

    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating(nameof(Send), delay, interval);
    }

    private void Send()
    {
        evt?.Invoke();
    }
}