
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestController : MonoBehaviour, IMoveable, IListener, IRoam
{
    public Animator animator; // Reference to Animator

    // movement 
    public float speedHold = 3f;
    public float alertSpeed = 5f;
    private float currentSpeed;
    public float obstacleAvoidanceRange = 1.5f;

    // sound detection 
    public float hearingRange = 40f;
    private float increasedHearingRange = 50f;
    private float highNoiseThreshold = 3f; // Threshold for high noise levels

    // Target tracking
    private Transform target;
    private Vector3 targetPosition;
    [SerializeField] private Transform playerTransform;

    // Threat level management
    private int threatLevel = 0;

    // Roam Behavior
    private Vector3 roamCenter;
    [SerializeField] private float roamRadius = 10f;
    [SerializeField] private float roamSpeed = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float biasFactor = 0.7f;
    [SerializeField] private LayerMask Terrain; // LayerMask for terrain
    private bool isRoaming = false;
    private Vector3 roamTarget;
    private bool isPursuingPlayer = true;    

    private void Awake(){
        animator = GetComponent<Animator>(); // Ensure Animator is assigned

        currentSpeed = speedHold; 
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        roamCenter = transform.position;
        SetRoamArea(roamCenter, roamRadius);
        StartRoaming();
    }

    private void Update()
    {
        RoamIfIdle();
        ListenForSounds();
        MoveTowardsTarget();
        UpdateAnimation();
        StayGrounded(); // Move StayGrounded logic into Update for consistent position adjustments
    }

    private void StayGrounded()
    {
        RaycastHit hit;
        // Adjust raycast to ensure it hits the terrain
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, Vector3.down, out hit, 10f, Terrain))
        {
            // Snap the priest's Y position to the terrain height
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y;
            transform.position = newPosition;
        }
        else
        {
            Debug.LogWarning($"StayGrounded: Priest is not detecting terrain! Position: {transform.position}");
        }
    }

    public void Move(Vector3 direction){
        if(direction == Vector3.zero) return;

        direction = AvoidObstacles(direction);
        transform.position += direction * currentSpeed * Time.deltaTime;

        if (direction != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private Vector3 AvoidObstacles(Vector3 direction){
        RaycastHit hit;
        Vector3 adjustedDirection = direction;

        if (Physics.SphereCast(transform.position, 0.5f, direction, out hit, obstacleAvoidanceRange)){
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, direction).normalized;
            adjustedDirection = (direction + avoidDirection).normalized;

            int attempts = 0;
            while (Physics.SphereCast(transform.position, 0.5f, adjustedDirection, out hit, obstacleAvoidanceRange) && attempts < 3){
                adjustedDirection = Vector3.Cross(Vector3.up, adjustedDirection).normalized;
                attempts++;
            }
        }

        return adjustedDirection;
    }

    public void Stop(){
        currentSpeed = 0f;
    }

    public void SetTarget(Transform newTarget){
        target = newTarget;
        targetPosition = target.position;
    }

    private void MoveTowardsTarget(){
        if (target == null && targetPosition == Vector3.zero) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Move(direction);
    }

    public void HearSound(Vector3 soundPosition, float intensity){
        if (Vector3.Distance(transform.position, soundPosition) <= hearingRange){
            SetTargetPosition(soundPosition);
            SetAlert(true);
            IncreaseThreatLevel();
        }
    }

    private void ListenForSounds(){
        float playerNoiseLevel = SoundManager.Instance.GetNoiseLevel(playerTransform);

        // Prioritize pursuing the player if they are making noise above the threshold within range
        if (playerNoiseLevel >= highNoiseThreshold && Vector3.Distance(transform.position, playerTransform.position) <= hearingRange){
            SetTarget(playerTransform);
            SetAlert(true);

            if (!isPursuingPlayer){
                isPursuingPlayer = true;
                Debug.Log("Priest is now pursuing the player due to high noise level.");
            }
        }
        else{
            // Pursue the loudest non-player noise if the player is not within range or not noisy enough
            Transform loudestSoundSource = SoundManager.Instance.GetLoudestSoundSource(transform.position);

            if (loudestSoundSource != null){
                HearSound(loudestSoundSource.position, 1.0f);
            }
            else{
                // No sounds in range, revert to roaming
                target = null;
                StartRoaming();
            }

            if (isPursuingPlayer){
                isPursuingPlayer = false;
                DebugLogger.Instance.Log("ListenForSound", "Priest stops pursuing player and roams.", 5f);
            }
        }
    }

    public void SetAlert(bool isAlert){
        currentSpeed = isAlert ? alertSpeed : speedHold;
    }

    public void IncreaseThreatLevel(){
        threatLevel++;
        AdjustBehavior();
    }

    private void AdjustBehavior(){
        hearingRange = threatLevel >= 3 ? increasedHearingRange : 15f;
        SetAlert(threatLevel >= 2);
    }

    private void SetTargetPosition(Vector3 position){
        target = null;
        targetPosition = position;
    }

    private void RoamIfIdle(){
        if (!isRoaming || target != null) return;

        if(Vector3.Distance(transform.position, roamTarget) < .5f){
            roamTarget = getRoamTarget();
        }

        Vector3 direction = (roamTarget - transform.position).normalized;
        transform.position += direction * roamSpeed * Time.deltaTime;

        if(direction != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 getRoamTarget(){
        Vector3 roamTarget;
        Vector3 lastKnownPosition = playerTransform.position; // Last known location of the player
        float safeDistance = 1.5f; // Minimum distance to keep from player's last known position

        // Generate a target around the player's last known location
        for (int attempts = 0; attempts < 5; attempts++) {
            // Generate a random point within the roam radius around the player's last known position
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized * Random.Range(safeDistance, roamRadius);

            roamTarget = lastKnownPosition + randomOffset;

            // Check if the generated roamTarget is at least 'safeDistance' away from the player
            if (Vector3.Distance(roamTarget, lastKnownPosition) >= safeDistance) {
                return roamTarget;
            }
        }

        // Fallback: If no valid target was found, use the roamCenter as a default
        roamTarget = roamCenter + Random.insideUnitSphere * roamRadius;
        roamTarget.y = transform.position.y;
        return roamTarget;
    }

    public void StartRoaming(){
        isRoaming = true;
        roamTarget = getRoamTarget();
    }

    public void StopRoaming(){
        isRoaming = false;
    }

    public void SetRoamArea(Vector3 center, float radius){
        roamCenter = center;
        roamRadius = radius;
    }

    private void UpdateAnimation(){
        // Update animation states
        bool isMoving = currentSpeed > 0;
        animator.SetBool("isWalking", isMoving && currentSpeed == speedHold);
        animator.SetBool("isRunning", isMoving && currentSpeed == alertSpeed);
    }
}
