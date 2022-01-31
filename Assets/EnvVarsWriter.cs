using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvVarsWriter : MonoBehaviour
{
    public string playerCountVarName = "PLAYER_COUNT";
    
    public void WriteVar(string pName, string val)
    {
        Environment.SetEnvironmentVariable(pName, val, EnvironmentVariableTarget.Machine);
    }

    public void AddIntToVar(string pName, int intVal)
    {
        var strVal = Environment.GetEnvironmentVariable(pName);
        if (!int.TryParse(strVal, out var oldVal))
        {
            Environment.SetEnvironmentVariable(pName, intVal.ToString());
        }
        else
        {
            Environment.SetEnvironmentVariable(pName, (oldVal + intVal).ToString());
        }
    }
    
    public void SetPlayerCount(int intVal)
    {
        WriteVar(playerCountVarName, intVal.ToString());
    }
}
