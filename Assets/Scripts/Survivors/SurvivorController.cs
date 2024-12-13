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

    //animation
    public Animator animator;

    // Grounding
    [SerializeField] private LayerMask terrainLayer;

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
        StayGrounded();
        UpdateAnimation();
    }

    private void StayGrounded(){
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, Vector3.down, out hit, 10f, terrainLayer)){
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y;
            transform.position = newPosition;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Keep survivor upright
        }
        else {
            Debug.LogWarning($"Survivor is not detecting terrain. Position: {transform.position}");
        }
    }

    private void UpdateAnimation(){
        if (isEscaping){
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        } else if (isRoaming) {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
        } else {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
    }

    public void Move(Vector3 direction){
        if (direction == Vector3.zero) return;

        direction = AvoidObstacles(direction);
        transform.position += direction * (isEscaping ? escapeSpeed : roamSpeed) * Time.deltaTime;

        if (direction != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private Vector3 AvoidObstacles(Vector3 direction){
        RaycastHit hit;
        Vector3 adjustedDirection = direction;

        if (Physics.SphereCast(transform.position, 0.5f, direction, out hit, 1.5f)){
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, direction).normalized;
            adjustedDirection = (direction + avoidDirection).normalized;
        }

        return adjustedDirection;
    }

    private void Escape(){
        Vector3 escapeDirection;

        // Escape opposite direction of priest
        if (priestTransform != null){
            escapeDirection = (transform.position - priestTransform.position).normalized;
        }
        else{
        // If Priest is not found, escape in a random direction
            escapeDirection = Random.onUnitSphere;
            escapeDirection.y = 0; 
        }   

        // Update rotation to face the escape direction
        if (escapeDirection != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(escapeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Move the survivor out of bounds
        transform.position += escapeDirection * escapeSpeed * Time.deltaTime;
        if (IsOutOfBounds(transform.position)){
            Destroy(gameObject);
    }
    }


    private bool IsOutOfBounds(Vector3 position){
        return position.x < -400f || position.x > 400f || position.z < -400f || position.z > 400f;
    }

    private void HandleAlertSound(){
        // Only emit sound if the survivor is not in danger
        if (isInDanger) return;

        // Update alert timer and manage sound emission state with a switch expression
        alertTimer = alertTimer <= 0 ? alertInterval : alertTimer - Time.deltaTime;
        
        // Emit sound if alert timer completes and `hasEmittedSound` is false
        hasEmittedSound = alertTimer switch {
            <= 0 when !hasEmittedSound => EmitSoundAndSetFlag(),
            _ => hasEmittedSound
        };
    }

    private bool EmitSoundAndSetFlag(){
        EmitSound();
        return true;
    }

    private void DetectPriest(){
        bool priestInRange = priestTransform != null && Vector3.Distance(transform.position, priestTransform.position) <= detectionRange;

        // Use a switch expression to handle the danger state transitions
        isInDanger = (isInDanger, priestInRange) switch {
            (false, true) => EnterDangerState(),   // Enter danger state if Priest is in range
            (true, false) => ExitDangerState(),    // Exit danger state if Priest leaves range
            _ => isInDanger                        // Maintain current state if no change
        };
    }

    private bool EnterDangerState(){
        NoiseLevel = 0f;
        SoundManager.Instance?.UnregisterSoundSource(this);
        return true;
    }

    private bool ExitDangerState(){
        NoiseLevel = 1f;
        SoundManager.Instance?.RegisterSoundSource(this);
        return false;
    }

    public void EmitSound(){
        if (!isInDanger){
            DebugLogger.Instance.Log("SurvivorAlert", "Survivor is emitting sound to alert player", 10f);
            SoundManager.Instance?.RegisterSoundSource(this);
        }
    }

    public void SetPriestTransform(Transform priest){
        priestTransform = priest;
    }

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

    private void HandleHealing(){
        if (isHealed || playerTransform == null) return; // Skip if already healed or no player

        if (Vector3.Distance(transform.position, playerTransform.position) <= 3f && Input.GetKey(KeyCode.Alpha5)){
            healTimer += Time.deltaTime;
            if (healTimer >= healTime){
                HealSurvivor();
            }
        } else {
            healTimer = 0f; // Reset the timer if player moves away or releases the key
        }
    }

    private void HealSurvivor(){
        isHealed = true;
        healTimer = 0f;
        StopRoaming();  // Stop any roaming behavior
        isEscaping = true;

        Debug.Log($"{gameObject.name} is healed and escaping.");
    }

    /*
    public bool IsHealed(){
        return isHealed; // Use the existing `isHealed` variable
    }
    */

}
