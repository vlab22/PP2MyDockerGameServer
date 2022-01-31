using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvVariablesSet : MonoSingleton<EnvVariablesSet>
{
    private Dictionary<string, string> envNames;

    public string PP2_SERVER_ID => envNames["PP2_SERVER_ID"];
    public string PP2_USER_AUTH => envNames["PP2_USER_AUTH"];
    public string PP2_USER_AUTH_PASS => envNames["PP2_USER_AUTH_PASS"];
    public string PP2_AUTH_CODE => envNames["PP2_AUTH_CODE"];
    public string PP2_AZ_FC_URL => envNames["PP2_AZ_FC_URL"];
    public string PP2_UPDATE_GAME_SERVER_PLAYER_COUNT_QUERY => envNames["PP2_UPDATE_GAME_SERVER_PLAYER_COUNT_QUERY"];
    public string PP2_UPDATE_GAME_SERVER_STATUS_QUERY => envNames["PP2_UPDATE_GAME_SERVER_STATUS_QUERY"];
    public string PP2_MX_PLAYERS => envNames["PP2_MAX_PLAYERS"];
    public string PP2_PLAYERS_COUNT => envNames["PP2_PLAYERS_COUNT"];
    public string PP2_SERVER_PORT => envNames["PP2_SERVER_PORT"];


#if UNITY_EDITOR
    public List<string> debugEnvs;
#endif

    private void Awake()
    {
        envNames = new Dictionary<string, string>()
        {
            { "PP2_SERVER_ID", "-1" },
            { "PP2_USER_AUTH", "pp2game" },
            { "PP2_USER_AUTH_PASS", "KDcx6kB2v77B5bm5AwY7XakYSsSB7Q4R" },
            { "PP2_AUTH_CODE", "s1GIzLcjWf3uSHN7WS8HIbhh4LFWdVIsi25scAASvpBuy9u7JkRcWw==" },
            { "PP2_UPDATE_GAME_SERVER_PLAYER_COUNT_QUERY", "/api/UpdateGameServerPlayerCountDataAzFc" },
            { "PP2_UPDATE_GAME_SERVER_STATUS_QUERY", "/api/UpdateGameServerStatusDataAzFc" },
            { "PP2_MAX_PLAYERS", "-1" },
            { "PP2_PLAYERS_COUNT", "-1" },
            { "PP2_SERVER_PORT", "-1" },
#if UNITY_EDITOR
            { "PP2_AZ_FC_URL", "http://localhost:7071" },
#else
            { "PP2_AZ_FC_URL", "https://pp2gameazurefcs20220127010428.azurewebsites.net" },
#endif
        };

        var keys = envNames.Keys.ToArray();
        foreach (var k in keys)
        {
            var envVar = Environment.GetEnvironmentVariable(k);
            if (string.IsNullOrWhiteSpace(envVar))
            {
                Environment.SetEnvironmentVariable(k, envNames[k]);
            }
            else
            {
                envNames[k] = envVar;
            }
        }
        
        DebugC.Warn(string.Join(" | ", envNames.Select(kv => kv.Key + ": '" + kv.Value + "'")));
        
#if UNITY_EDITOR
        debugEnvs = envNames.Select(kv => $"{kv.Key}: {Environment.GetEnvironmentVariable(kv.Key)}").ToList();
#endif
    }
}