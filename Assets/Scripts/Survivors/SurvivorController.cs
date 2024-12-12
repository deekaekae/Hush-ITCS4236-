using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorController : MonoBehaviour, IProduceSound, IRoam
{
    public float NoiseLevel { get; private set; } = 3f;
    public GameObject GameObject => gameObject;
    private bool isInDanger = false;

    // Sound emission settings
    public float alertInterval = 5f;
    private float alertTimer;
    private bool hasEmittedSound = false;

    // Detection settings
    public float detectionRange = 10f;
    private Transform priestTransform;
    private Transform playerTransform;   // Reference to the Player's position


    //Roaming Behavior
    private Vector3 roamCenter;
    [SerializeField] private float roamRadius = 5f; // Radius of the roam area
    public float roamSpeed = 2f;
    private bool isRoaming = false;
    private Vector3 roamTarget;
    public float rotationSpeed = 2f;

    //healed/escaping behavior
    [SerializeField] private float healTime = 5f;      // Time required to heal the survivor
    private float healTimer = 0f;
    private bool isHealed = false;
    [SerializeField] private float escapeSpeed = 10f;  // Speed at which the survivor escapes after healing
    private bool isEscaping = false;


    

    private void Start(){
        alertTimer = alertInterval;
        roamCenter = transform.position;
        SoundManager.Instance?.RegisterSoundSource(this);  // Register survivor as a sound source
        SetRoamArea(roamCenter, roamRadius);
        StartRoaming();

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        priestTransform = GameObject.FindGameObjectWithTag("Priest")?.transform;

    }

    private void Update(){
        if(isEscaping){
            Escape();
            return;
        }

        HandleAlertSound();
        DetectPriest();
        RoamIfIdle();
        HandleHealing();
    }

    // Manage sound emission based on current state
    private void HandleAlertSound(){
        // Only emit sound if the survivor is not in danger
        if (isInDanger) return;

        // Update alert timer and manage sound emission state with a switch expression
        alertTimer = alertTimer <= 0 ? alertInterval : alertTimer - Time.deltaTime;
        
        // Emit sound if alert timer completes and `hasEmittedSound` is false
        hasEmittedSound = alertTimer switch
        {
            <= 0 when !hasEmittedSound => EmitSoundAndSetFlag(),
            _ => hasEmittedSound
        };
    }

    // Emits sound and sets the flag to prevent multiple registrations within interval
    private bool EmitSoundAndSetFlag(){
        EmitSound();
        return true;
    }

    // Detect the Priest and manage the danger state based on proximity
    private void DetectPriest(){
        bool priestInRange = priestTransform != null && Vector3.Distance(transform.position, priestTransform.position) <= detectionRange;

        // Use a switch expression to handle the danger state transitions
        isInDanger = (isInDanger, priestInRange) switch
        {
            (false, true) => EnterDangerState(),   // Enter danger state if Priest is in range
            (true, false) => ExitDangerState(),    // Exit danger state if Priest leaves range
            _ => isInDanger                        // Maintain current state if no change
        };
    }

    // Called when entering danger state
    private bool EnterDangerState(){
        NoiseLevel = 0f;
        SoundManager.Instance?.UnregisterSoundSource(this);
        return true;
    }

    // Called when exiting danger state
    private bool ExitDangerState(){
        NoiseLevel = 1f;
        SoundManager.Instance?.RegisterSoundSource(this);
        return false;
    }

    // Implementing IProduceSound
    public void EmitSound(){
        if (!isInDanger){
            DebugLogger.Instance.Log("SurvivorAlert", "Survivor is emitting sound to alert player", 10f);
            SoundManager.Instance?.RegisterSoundSource(this);
        }
    }

    // Assigns the Priest's transform for proximity detection
    public void SetPriestTransform(Transform priest){
        priestTransform = priest;
    }

    //IRoam Interface
    public void StartRoaming(){
        isRoaming = true;
        roamTarget = GetRandomRoamPoint();
    }

    public void StopRoaming(){
        isRoaming = false;
    }

    public void SetRoamArea(Vector3 center, float radius){
        roamCenter = center;
        roamRadius = radius;
    }

    private Vector3 GetRandomRoamPoint(){
        Vector2 randomPoint = Random.insideUnitCircle * roamRadius;
        return new Vector3(roamCenter.x + randomPoint.x, transform.position.y, roamCenter.z + randomPoint.y);
    }

    private void RoamIfIdle(){
        
        if(!isRoaming) return;

        if(isRoaming && Vector3.Distance(transform.position, roamTarget) < .5f){
            roamTarget = GetRandomRoamPoint();
        }
        
        Vector3 direction = (roamTarget - transform.position).normalized;
        transform.position += direction * roamSpeed * Time.deltaTime;
        
        if(direction != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleHealing()
    {
        if (isHealed || playerTransform == null) return; // Skip if already healed or no player

        if (Vector3.Distance(transform.position, playerTransform.position) <= 3f && Input.GetKey(KeyCode.Alpha5))
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healTime)
            {
                HealSurvivor();
            }
        }
        else
        {
            healTimer = 0f; // Reset the timer if player moves away or releases the key
        }
    }

    // Heal survivor and start escape behavior
    private void HealSurvivor()
    {
        isHealed = true;
        healTimer = 0f;
        StopRoaming();  // Stop any roaming behavior
        isEscaping = true;

        Debug.Log($"{gameObject.name} is healed and escaping.");
    }

    // Move survivor away from the priest and out of map bounds
    private void Escape()
    {
        Vector3 escapeDirection;

        // Determine escape direction as opposite of the Priest
        if (priestTransform != null)
        {
            escapeDirection = (transform.position - priestTransform.position).normalized;
        }
        else
        {
            // If Priest is not found, escape in a random direction
            escapeDirection = Random.onUnitSphere;
            escapeDirection.y = 0; // Keep the escape direction on the horizontal plane
        }

        // Move the survivor out of bounds
        transform.position += escapeDirection * escapeSpeed * Time.deltaTime;

        if(IsOutOfBounds(transform.position)){
            //Destroy(GameObject);
        }
        // Optional: Add fade-out effect here if needed to make the survivor disappear visually
    }

    private bool IsOutOfBounds(Vector3 position)
    {
        return position.x < -400f || position.x > 400f || position.z < -400f || position.z > 400f;
    }


    

    // IMPLEMENET YOU HEAL THE SURVIVOR, WHEN THEY HEAL THEY RUN OFF THE MAP
    // RODEO THE PRIEST TO SAVE AS MANY AS POSSIBLE
}
