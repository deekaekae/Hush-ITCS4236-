using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    private static DebugLogger _instance;
    public static DebugLogger Instance{
        get{
        if(_instance == null){
            GameObject loggerObject = new GameObject("Debuglogger");
            _instance = loggerObject.AddComponent<DebugLogger>();
            DontDestroyOnLoad(loggerObject);
        }
        return _instance;
        }
    }

    private Dictionary<string, float> logCooldowns = new Dictionary<string, float>();

    public void Log(string key, string message, float cooldown = 30f){
        float lastLogTime = logCooldowns.ContainsKey(key) ? logCooldowns[key] : 0f;
        if(Time.time - lastLogTime >= cooldown){
            Debug.Log(message);
            logCooldowns[key] = Time.time;
        }
    }

    public void ResetLogCooldown(string key){
        if(logCooldowns.ContainsKey(key))
        logCooldowns[key] = 0f;
    }
}
