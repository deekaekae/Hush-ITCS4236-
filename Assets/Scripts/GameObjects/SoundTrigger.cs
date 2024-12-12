using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour, IProduceSound
{
    // Sound level emitted by the object
    [SerializeField] private float noiseLevel = 10f;
    [SerializeField] private float cooldownTime = 3f; // Cooldown before it can trigger sound again
    private float cooldownTimer = 0f;

    public float NoiseLevel { get; private set; } = 0f; // Start with no noise
    public GameObject GameObject => gameObject;

    private void Update(){
        // Handle cooldown timer
        if (cooldownTimer > 0){
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0){
                NoiseLevel = 0f; // Reset noise level after cooldown
                SoundManager.Instance?.UnregisterSoundSource(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other){
        // Only trigger if cooldown has expired and collided with relevant characters
        if (cooldownTimer <= 0 && (other.CompareTag("Player") || other.CompareTag("Survivor") || other.CompareTag("Priest"))){
            EmitSound(); // Emit sound on collision
            Debug.Log($"{other.gameObject.name} triggered sound on {gameObject.name}");
        }
    }

    public void EmitSound(){
        NoiseLevel = noiseLevel; // Set noise level for detection
        SoundManager.Instance?.RegisterSoundSource(this); // Register with SoundManager
        cooldownTimer = cooldownTime; // Start cooldown timer
    }
}
