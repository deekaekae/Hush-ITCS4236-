using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestController : MonoBehaviour, IMoveable, IListener
{
    // movement 
    public float defaultSpeed = 3f;
    public float alertSpeed = 5f;
    private float currentSpeed;
    public float obstacleAvoidanceRange = 1.5f;

    // sound detection 
    public float hearingRange = 15f;
    private float increasedHearingRange = 25f; // Hearing range at higher threat levels

    // Target tracking
    private Transform target;
    private Vector3 targetPosition;

    // Threat level management
    private int threatLevel = 0;

    private void Awake(){
        currentSpeed = defaultSpeed;
    }

    private void Update()
    {
        ListenForSounds();
        MoveTowardsTarget();
    }

    //From IMoveable Interface
    public void Move(Vector3 direction){
        // Perform obstacle avoidance check before moving
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, obstacleAvoidanceRange)){
            // Adjust direction to avoid obstacles
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, direction).normalized;
            direction = (direction + avoidDirection).normalized;
        }

        // Move in the (possibly adjusted) direction
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Smooth rotation to face movement direction
        if (direction != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void Stop(){
        currentSpeed = 0f;
    }

    // Sets a new target for the Priest to follow
    public void SetTarget(Transform newTarget){
        target = newTarget;
        targetPosition = target.position;
    }

    private void MoveTowardsTarget(){
        if (target == null) return;

        // Calculate direction toward the target and move
        Vector3 direction = (targetPosition - transform.position).normalized;
        Move(direction);
    }

    // From IListener Interface
    public void HearSound(Vector3 soundPosition, float intensity){
        if (Vector3.Distance(transform.position, soundPosition) <= hearingRange){
            SetTargetPosition(soundPosition);  // Move toward the sound position
            SetAlert(true);                    // Increase speed when moving toward sound
        }
    }

    private void ListenForSounds(){
        Transform soundTransform = SoundManager.Instance.GetLoudestSoundSource(transform.position);

        if (soundTransform != null && Vector3.Distance(transform.position, soundTransform.position) <= hearingRange){
            HearSound(soundTransform.position, 1.0f); // Pass intensity if applicable
        }
    }


    // Adjust speed based on alert status
    public void SetAlert(bool isAlert){
        currentSpeed = isAlert ? alertSpeed : defaultSpeed;
    }

    // Method to increase threat level and adjust behavior
    public void IncreaseThreatLevel(){
        threatLevel++;
        AdjustBehavior();
    }

    // Adjust behavior based on the current threat level
    private void AdjustBehavior(){
        // Increase hearing range and set alert mode based on threat level
        hearingRange = threatLevel >= 3 ? increasedHearingRange : 15f;
        SetAlert(threatLevel >= 2); // Set alert mode if threat level is high
    }

    private void SetTargetPosition(Vector3 position){
        target = null;       // Clear current target Transform
        targetPosition = position;  // Set the target position to the sound's position
    }
}
