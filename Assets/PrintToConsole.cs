using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintToConsole : MonoBehaviour
{
    protected enum MsgType
    {
        Info,
        Warn,
        Error
    }

    [SerializeField] private MsgType _msgType;

    public string prefix;
    
    public string msg;

    // Start is called before the first frame update
    void Start()
    {
        Print();
    }

    [ContextMenu("Print")]
    public void Print()
    {
        var resultMsg = prefix + msg;
        
        switch (_msgType)
        {
            case MsgType.Info:
                DebugC.Log(resultMsg);
                break;
            case MsgType.Warn:
                DebugC.Warn(resultMsg);
                break;
            case MsgType.Error:
                DebugC.Error(resultMsg);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}