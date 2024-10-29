
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

    public void RegisterSoundSource(IProduceSound soundEmitter){
        if (!soundSources.Contains(soundEmitter))
            soundSources.Add(soundEmitter);
    }

    public void UnregisterSoundSource(IProduceSound soundEmitter){
        soundSources.Remove(soundEmitter);
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
        return loudest?.GameObject.transform;  //return transform of loudest sound soure
    }
}
