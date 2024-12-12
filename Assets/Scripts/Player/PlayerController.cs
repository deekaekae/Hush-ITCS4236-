using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMoveable, IHideable, IProduceSound
{
    //movement 
    private CharacterController characterController;
    public float walkSpeed = 6f;
    public float crouchSpeed =2f;
    //Sprinting
    public float sprintMultiplier = 2f;
    private bool isSprinting = false;
    private float speedHold = 6f; //hold value for default walk speed
    private float speedRampUpRate = 5f;
    private float maxSprintSpeed = 12f;
    

   
    //crouching
    private float originalHeight = 1f;
    private float crouchHeight = .5f;
    private bool isCrouching = false;
    public float crouchTransitionSpeed = 5f;

    //hiding 
    public bool isHiding{get; private set; } = false;
    public GameObject GameObject => gameObject;
    public float NoiseLevel {get; private set;} = 0f; //from IProducdSound

    //camera
    public Transform cameraTransform;
    private Vector3 originalCameraPosition;
    private float crouchCameraOffset = -.5f;

    //jumping
   public float jumpHeight = 2f;
   public float gravity = -9.81f;
   private Vector3  verticalVelocity = Vector3.zero;
   private bool isGrounded;

    private void Awake(){
        characterController = GetComponent<CharacterController>();
        originalHeight = characterController.height;
        originalCameraPosition = cameraTransform.localPosition;
    }


    void start(){
        walkSpeed = speedHold;
    }
    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleHiding();
        HandleCrouching();
        HandleJumping();
        HandleSprinting();
        UpdateNoiseLevel();
        
    }

    //From IMoveable Interface
    public void Move(Vector3 direction){
        Vector3 move = direction + verticalVelocity;
        characterController.Move(move * Time.deltaTime);
    }

    public void Stop(){
        characterController.SimpleMove(Vector3.zero);
    }


    private void HandleJumping(){
        if (isGrounded && Input.GetKeyDown(KeyCode.Space)){
            //set vertical velocity based on height and gravity
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity );
        }
    }

    private void HandleCrouching(){
        if(Input.GetKeyDown(KeyCode.LeftControl)){
            isCrouching = !isCrouching;
            //If player crouching, then crouchHeight, else OriginalHeight
            characterController.height = isCrouching ? crouchHeight: originalHeight; 
        }
        
        //moved targetCameraPosition if crouching, else OG camera position
        Vector3 targetCameraPosition = isCrouching 
            ? originalCameraPosition + new Vector3(0, crouchCameraOffset, 0) 
            : originalCameraPosition;

        //smoothen camera transition to target
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition, 
            targetCameraPosition,
            crouchTransitionSpeed * Time.deltaTime
        );
    }

    private void HandleSprinting(){
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        if(isGrounded){
            if(isSprinting){
                //gradually ramp up sprint speed until maxSprintSpeed reached
                walkSpeed = Mathf.MoveTowards(walkSpeed, maxSprintSpeed, speedRampUpRate * Time.deltaTime);
            }
            else{
                walkSpeed = speedHold;
            }
            DebugLogger.Instance.Log("PlayerNoise", $"Player NoiseLevel: {NoiseLevel} ", 10f);
        }
    }

    //From IHidable Interface
    public void Hide(){
        isHiding = true;
        NoiseLevel = 0f;
        SoundManager.Instance.UnregisterSoundSource(this); // stop sound
    }

    public void ExitHide(){
        isHiding = false;
        SoundManager.Instance.RegisterSoundSource(this);// start sound
    }

    //From IProduceSound Interface
    public void EmitSound(){
        NoiseLevel = (isHiding, isSprinting, isCrouching) switch{
        (true, _, _) => 0f,           // Hiding: No sound emitted
        (false, true, _) => 4f,       // Sprinting: High noise level
        (false, false, true) => 0f, // Crouching: Lower noise level
        _ => 2f                       // Default walking noise level
        };

        SoundManager.Instance.RegisterSoundSource(this);
        DebugLogger.Instance.Log("PlayerNoise", $"Player NoiseLevel: {NoiseLevel}", 10f);
    }

    private void UpdateNoiseLevel(){
        if(characterController.velocity.magnitude  > 0 && !isHiding){
            EmitSound(); // call EmitSound to register w/ sound manager
        }
        else{
            NoiseLevel = 0f;
            SoundManager.Instance.UnregisterSoundSource(this);
        }
    }

    private void HandleMovement(){
        isGrounded = characterController.isGrounded;
        if(isGrounded && verticalVelocity.y < 0){
            verticalVelocity.y = -2f;
        }
        
        //vertical horizontal movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        //movement relative to camera orientation 
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;

        //if crouching, then crouchspeed, else walkspeed
        float speed = isCrouching ? crouchSpeed : walkSpeed;

        if(!isGrounded){
            verticalVelocity.y += gravity * Time.deltaTime; 
        }

        Move(moveDirection.normalized * speed);
    }
    
    //press H to hide, toggle if already hiding (hold)
    private void HandleHiding(){
        if(Input.GetKeyDown(KeyCode.Mouse1)){
            if(isHiding){
                ExitHide();
            }
            else{
                Hide();
                DebugLogger.Instance.Log("isHiding", $"Player Holds breath, NoiseLevel: {NoiseLevel}", 3f );
            }
        }
    }

}
