using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThreatLevelManager : MonoBehaviour
{
    public static ThreatLevelManager Instance { get; private set; }
    public int ThreatLevel { get; private set; } = 0; 

    private void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);  // Ensure only 1 threat level manager
    }

    public void IncreaseThreatLevel(int amount = 1){
        ThreatLevel += amount;
        ThreatLevelChanged();
    }

    public void DecreaseThreatLevel(int amount = 1){
        ThreatLevel = Mathf.Max(0, ThreatLevel - amount);
        ThreatLevelChanged();
    }

    //Alert to change in Threat level
    private void ThreatLevelChanged(){
        Debug.Log($"Threat level is now: {ThreatLevel}");
    }
}

