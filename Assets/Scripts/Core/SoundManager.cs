
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }   // do not create multiple sound mangers
    private List<IProduceSound> soundSources = new List<IProduceSound>(); // List of active sound sources

    //ensure single sound manager
    private void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else{
            Destroy(gameObject);
        }
         
    }

    //add to list of sound sources 
    public void RegisterSoundSource(IProduceSound soundEmitter){
        if (!soundSources.Contains(soundEmitter))
            soundSources.Add(soundEmitter);
            DebugLogger.Instance.Log("SoundSourceRegistered", $"Registered sound source: {soundEmitter.GameObject.name}", 15f); //15 seconds cooldown
    }

    //removed from list of sound sources ( mostly just for it player hides)
    public void UnregisterSoundSource(IProduceSound soundEmitter){
        soundSources.Remove(soundEmitter);
        DebugLogger.Instance.Log("SoundSourceUnregistered", $"Unregistered sound source: {soundEmitter.GameObject.name}", 3f);// 15 seconds cooldown

    }

    // Finds the loudest sound source near the given position for priest to persue
    public Transform GetLoudestSoundSource(Vector3 priestPosition){
        IProduceSound loudest = null;
        float maxNoise = 0f;

        foreach (var emitter in soundSources){
            if (emitter.NoiseLevel > maxNoise){
                maxNoise = emitter.NoiseLevel;
                loudest = emitter;
            }
        }
        if(loudest != null){
            DebugLogger.Instance.Log("LoudestSoundSource", $"Loudest sound source: {loudest.GameObject.name} with noise level: {maxNoise}", 5f); // Log every 5 seconds
        }
        else{
            DebugLogger.Instance.Log("NoLoudestSoundSource", "No sound source detected within range.", 30f); // Log every 30 seconds if no source is found
        }
        return loudest?.GameObject.transform;  //return transform of loudest sound soure
    }

    // Check noise level 
    public float GetNoiseLevel(Transform source){
        var emitter = soundSources.FirstOrDefault(e => e.GameObject.transform == source);
        return emitter?.NoiseLevel ?? 0f; // Return 0 if the source is not found
    }

}
