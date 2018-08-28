using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
  
    [Header("General Movement Parameters")]
    [Tooltip("Maximum speed the character can achieve walking.")]
    public float baseMovementSpeed = 4f;
    [Tooltip("The boost in movement speed the character will get while running.")]
    public float runSpeedMultiplier = 1.5f;
    [Tooltip("The speed measured in Degrees/seconds.")]
    public float turnSpeed = 720f;
    [Tooltip("Multiplier value used to increase or decrease the gravity effect.")]
    public float gravityScale = 2f;
    [Tooltip("Vertical speed asigned to the player when jumping.")]
    public float jumpSpeed = 7.5f;
    [Tooltip("Maximum speed the character reach get to while falling.")]
    public float maximumFallingSpeed = 10f;
    [Tooltip("Boolean that indicates if the character should automatically change its movement direction when colliding a wall.")]
    public bool changeMovementDirectionOnCollision = false;


    [Header("Step Climbing")]
    [Tooltip("Minimum free of collider depth that a step must have on top to be climbable.")]
    public float stepMinDepth = 0.4f;
    private float stepMaxHeight;


    [Header("Steep Slope Sliding")]
    [Tooltip("Minimum angle between the slope and the ground to consider it a steep.")]
    public float steepAngle = 50f;
    [Tooltip("Minimum speed the character will get while sliding.")]
    public float steepSlidingSpeed = 8f;


    
    
    // Input variables.
    // Transform to calculate the direction of the movement.
    // Movement Axes for the player. M = V + H
    // Maximum Movement Speed = base Movement Speed (* run Speed Multiplier while running)
    Transform cameraTransform;
    Vector2 movementInput;
    bool jumpInput;
    bool runInput;
    Vector3 movementDirection;
    Vector3 horizontalDirection;
    Vector3 verticalDirection;
    float maximumMovementSpeed;

    // Physics variables (Raycasts, Capsulecasts, etc.)
    int environmentLayerMask;
    int enemiesLayerMask;
    RaycastHit[] capsulecastHitArray;
    Vector3 point1;
    Vector3 point2;
    float radius;
    float radiusScale = 0.95f;

    [HideInInspector]
    public bool playerCloseToGround;
    private bool playerJumping;
    [HideInInspector]
    public bool playerSliding;
    private bool playerGrounded;

    Vector3 groundNormal;
    float groundAngle;

    
    // Player variables
    private Animator playerAnimator;
    private float animatorWalkSpeedParameter;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;

    // Variables used to rotate the player
    private Quaternion targetRotation;
    private Vector3 velocityPlaneDirection;






    private bool inputEnabled = true;
  



    
    void Awake ()
    {
        playerRigidbody = transform.GetComponent<Rigidbody>();
        playerCapsuleCollider = transform.GetComponent<CapsuleCollider>();
        playerAnimator = GetComponentInChildren<Animator>();
        stepMaxHeight = radius = playerCapsuleCollider.radius;

        GlobalData.PlayerTransform = transform;
        GlobalData.PlayerTargetTransform = transform.Find("Target");
        GlobalData.PlayerMovementScript = this;
		GlobalData.PlayerAnimator = playerAnimator;
    }

    void Start()
    {
        cameraTransform = GlobalData.PlayerCamera.transform;
        environmentLayerMask = 1 << (int) Mathf.Log(GlobalData.EnvironmentLayerMask.value,2);
        enemiesLayerMask = 1 << (int) Mathf.Log(GlobalData.EnemiesLayerMask.value,2);

        inputEnabled = true;
    }
	

	void Update ()
    {
        // Read the inputs.
        if (inputEnabled)
        {
            // Limit movementInput to a maximum magnitude of 1f.
            movementInput = new Vector2(Input.GetAxis(InputAxis.HorizontalMovement),Input.GetAxis(InputAxis.VerticalMovement)) ;
            if (movementInput.magnitude > 1 )
            {
                movementInput.Normalize();
            }

            // jumpInput acts as a flag because the jump is performed in FixedUpdate()
            if (Input.GetButton(InputAxis.Jump)) 
            {
                jumpInput = true;
            }

            // Gamepads provide a float value for this button.
            runInput = Input.GetButton(InputAxis.Run);
            
        }
        else
        {
            movementInput = Vector2.zero;
            jumpInput = false;
            runInput = false;
        }

               
        // Update movement axes 
        if (!GlobalData.IsEnemyLocked)
        {
            horizontalDirection = Vector3.Scale(cameraTransform.right, new Vector3(1f, 0f, 1f)).normalized;
            verticalDirection = Vector3.Scale(cameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;
        }
        else
        {
            verticalDirection = Vector3.Scale(GlobalData.LockedEnemyTransform.position - transform.position, new Vector3(1f, 0f, 1f)).normalized;
            horizontalDirection = Vector3.Cross(verticalDirection, -transform.up).normalized;
        }
        movementDirection = horizontalDirection * movementInput.x + verticalDirection * movementInput.y;


        //Rotate the player (Why is this here? Because it is not possible to interpolate rotations in no-kinematic objects).
        if (playerRigidbody.velocity.magnitude > 0f)
        {            
            if (playerSliding)
            {
                playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,targetRotation,turnSpeed*2*Time.deltaTime) );
            }
            else
            {
                if (!GlobalData.IsEnemyLocked)
                {                    
                    if ( movementInput.magnitude > 0.01f && playerRigidbody.velocity.magnitude > 0.01f)
                    {
                        if( Vector3.Angle(transform.forward,velocityPlaneDirection) > 135) 
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,targetRotation,turnSpeed*2*Time.deltaTime ));
                        }
                        else
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,targetRotation,turnSpeed*Time.deltaTime) );
                        }
                    }
                }
                else
                {
                    playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,targetRotation,turnSpeed*Time.deltaTime) );
                }
            }      
        }
    }


    void FixedUpdate()
    {
        // Modify parameters based on character's state
        // ( run/walk -> maximumMovementSpeed)
        // ( update target rotation based on last fixed step )
        UpdateParameters();

        // This is used to update variables for the capsule casts.
        UpdatePlayerCapsulePosition();

        // Check If the character is close to the ground, grounded, sliding or falling.
        // Set groundNormal and groundAngle values.
        CheckIfGroundedOrSliding();

        Jump();

        // Project movementDirection on the floor's normal to properly constraint movement direction later.
        ProjectMovementDirection();

        // If the player is in front of a wall/steep, constraint the movement direction
        // If the player is in front of a step, jump it
        // If the player is in front of a different terrain, project the movement direction.
        ConstraintMovementDirection();

        // Set the velocity of the character.
        // If the character is not grounded, prevent velocity.y from increase to avoid the character "flying" when walking up slopes. (This happens because playerGrounded works with a small offset)
        // Clamp velocity.y to avoid constant falling acceleration
        SetVelocity();
        
        // If the character is grounded and not jumping, project the velocity to groundNormal.
        // If the character is sliding, constrain the velocity to make the it slide properly
        // If the character is not on a steep, constraint the velocity magnitude
        ProjectVelocityDirection();

        // Updates the values of the animator parameters based on the changes of this fixed step.
        // It also updates the sounds to be played
        UpdateAnimatorAndSound();

        //Add gravity the player.
        playerRigidbody.AddForce(Physics.gravity*gravityScale,ForceMode.Acceleration);
    }

    void UpdateParameters()
    {
        maximumMovementSpeed = (runInput) ? baseMovementSpeed*runSpeedMultiplier : baseMovementSpeed;
        velocityPlaneDirection = Vector3.Scale(playerRigidbody.velocity,new Vector3(1,0,1));
        targetRotation = Quaternion.LookRotation(velocityPlaneDirection,Vector3.up);
        
    }
    void UpdatePlayerCapsulePosition()
    {
        point1 = playerRigidbody.position + playerCapsuleCollider.center + transform.up *( playerCapsuleCollider.height / 2 - radius);
        point2 = playerRigidbody.position + playerCapsuleCollider.center - transform.up *( playerCapsuleCollider.height / 2 - radius);
    }

    RaycastHit[] CapsuleCastFromPlayer(float radiusScale,Vector3 direction, float distance, int layerMask)
    {
        return Physics.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }
    RaycastHit[] OptimizedCapsuleCastFromPlayer(float radiusScale,Vector3 direction, float distance, int layerMask)
    {
        return OptimizedCast.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }

    bool Vector3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }

    void ProjectMovementDirection()
    {
        if (!Vector3Equal(groundNormal, Vector3.zero) && !playerJumping)
        {
            float oldMovementMagnitude = movementDirection.magnitude;
            movementDirection = Vector3.ProjectOnPlane(movementDirection, groundNormal);
            
            // If the new movementDirection isn't 0, scale the movementDirection vector.
            if (!Vector3Equal(movementDirection,Vector3.zero)) 
            {
                movementDirection *= oldMovementMagnitude/movementDirection.magnitude;
            }
        }        
    }


    // CapsuleCast below the player with a distance at least as big as the stepMaxHeight.
    // 2 different grounded booleans:
    //      playerGrounded is true when the player is actually on the ground
    //      playerCloseToGround is true when the character is not grounded but between the distance (Used when climbing down steps, to avoid problems with the animator.) 
    void CheckIfGroundedOrSliding()
    {
        capsulecastHitArray = OptimizedCapsuleCastFromPlayer(radiusScale,Vector3.down, Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-stepMaxHeight)),environmentLayerMask| enemiesLayerMask);

        if (capsulecastHitArray.Length > 0 ) 
        {   
            playerCloseToGround = true;
            playerSliding = true;
            playerGrounded = false;     
            
            for (int i = capsulecastHitArray.Length-1; i >= 0 ; i--)
            {
                if (capsulecastHitArray[i].collider.isTrigger)
                {
                    continue;
                }

                RaycastHit hitInfo;
                groundNormal = capsulecastHitArray[i].normal;
                groundAngle = Vector3.Angle(groundNormal, Vector3.up);

                // Check if the ground's surface isn't a slope and the normal isn't "pointing downwards"
                // The raycast avoid sliding when the character is close to the edge of a slope.
                if ( groundAngle <= steepAngle && groundNormal.y >= 0 )
                {
                    playerSliding = false;

                }
                else if (Physics.Raycast(capsulecastHitArray[i].point+capsulecastHitArray[i].normal*capsulecastHitArray[i].distance,-capsulecastHitArray[i].normal,out hitInfo,capsulecastHitArray[i].distance*1.1f,environmentLayerMask| enemiesLayerMask) )
                {
                    if (capsulecastHitArray[i].collider.isTrigger)
                    {
                        continue;
                    }

                    if (  hitInfo.normal != groundNormal  )
                    {
                        playerSliding = false;
                    }
                   
                }

                // Check if the character is grounded
                if ( !playerGrounded && (capsulecastHitArray[i].distance*groundNormal).y < 0.05f)
                {
                    playerGrounded = true;
                }

                if (playerGrounded && !playerSliding)
                {
                    break;
                }
                
            }
        }
        else
        {
            playerCloseToGround = false; 
            playerSliding = false;
            playerGrounded = false;
            
            groundNormal = Vector3.zero;
            groundAngle = float.MinValue;
            
        }
    }

    void Jump()
    {
        
        if (playerJumping )
        {
            if (playerGrounded)
            {
                playerJumping = false;
                return;
            }
        }

        if (jumpInput && playerGrounded && !playerSliding && !playerJumping )
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,jumpSpeed,playerRigidbody.velocity.z);
            playerJumping = true;          
        }

        jumpInput = false;
    }

    void ConstraintMovementDirection()
    {
        // If there is a movementDirection, capsuleCast in that direction to avoid the player to stick on walls and avoid small terrain variations.
        if (!Vector3Equal(movementDirection, Vector3.zero))
        {
           
            capsulecastHitArray = OptimizedCapsuleCastFromPlayer(radiusScale,movementDirection.normalized,maximumMovementSpeed*Time.fixedDeltaTime,environmentLayerMask | enemiesLayerMask);
        
            // This value is used to mantain the input value after constraining the movementDirection.
            float oldMovementMagnitude = movementDirection.magnitude;

            //foreach (RaycastHit capsulecastHit in capsulecastHitArray)
            for (int i = capsulecastHitArray.Length-1; i >= 0; i--)
            {
                if (capsulecastHitArray[i].collider.isTrigger)
                {
                    continue;
                }
                
                //For colliders that overlap the capsule at the start of the sweep, to avoid problems.
                if (Vector3Equal(Vector3.zero,capsulecastHitArray[i].point))
                {
                    continue;
                }
                
                // If the capsulecast hit a wall/steep:
                //      and the hit height is not allowed
                //      or the normal is "pointing downwards"
                //      or another capsule collider hits any object
                //      or the character is jumping.
                // Constraint movementDirection
                if ( capsulecastHitArray[i].normal.y < 0 || Vector3.Angle(capsulecastHitArray[i].normal, Vector3.up) > steepAngle )
                {
                    float distanceToGround = Mathf.Max(0f,Vector3.Project(capsulecastHitArray[i].point -(point2 -Vector3.up*radius),groundNormal).y); 
                    
                    if ( playerJumping || distanceToGround > stepMaxHeight || capsulecastHitArray[i].normal.y < 0 ||Physics.CapsuleCast(point1+Vector3.up*stepMaxHeight,point2+Vector3.up*stepMaxHeight,radius,movementDirection.normalized,Mathf.Max(capsulecastHitArray[i].normal.y,stepMinDepth),environmentLayerMask | enemiesLayerMask) )
                    {
                        movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(capsulecastHitArray[i].normal,new Vector3(1,0,1)).normalized);
                    }
                    else
                    {
                        if (playerGrounded)
                        {
                            playerRigidbody.MovePosition(playerRigidbody.position+Vector3.up*distanceToGround);
                            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,0f,playerRigidbody.velocity.z);
                            break;
                        }
                        continue;
                    }
                }      
                else
                {

                    if (playerGrounded)
                    {
                        // If the gameObject in front isn't a slope or wall, and the character isn't falling, just use its normal as floor normal.
                        groundNormal = capsulecastHitArray[i].normal;
                        groundAngle = Vector3.Angle(groundNormal,Vector3.up);


                        // And project the movement Direction Again
                        ProjectMovementDirection();

                        
                    }

                    continue;

                }  
                
            }
            
            // This code is to enable movement direction readjust (not recommended for a platforms game)
            // If the new movementDirection isn't 0, scale the movementDirection vector.
            if (changeMovementDirectionOnCollision && !Vector3Equal(movementDirection,Vector3.zero) ) 
            {
                movementDirection *= oldMovementMagnitude/movementDirection.magnitude;
            }
            
           

        }
    }

    void SetVelocity()
    {
        float velocityY = playerRigidbody.velocity.y;
        
        playerRigidbody.velocity = movementDirection*maximumMovementSpeed + Vector3.up*velocityY;

        

        if (!playerGrounded)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,Mathf.Min(velocityY,playerRigidbody.velocity.y),playerRigidbody.velocity.z);
        }


        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,Mathf.Max(playerRigidbody.velocity.y,-maximumFallingSpeed),playerRigidbody.velocity.z);
    }

      void ProjectVelocityDirection()
    {
        // If the player is grounded in a slope and not jumping, adjust the movement direction. If it is on a steep, it should fall.
        if ( playerGrounded && !playerJumping)
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, groundNormal);  
    
            if (playerSliding)
            {  
                playerRigidbody.velocity -= Vector3.Project(playerRigidbody.velocity, Vector3.Scale(groundNormal,new Vector3(1,0,1)).normalized);
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,  -steepSlidingSpeed, playerRigidbody.velocity.z);
            }
            else
            {
                //If the user isn't giving any input, prevent the character from sliding.
                if ( movementInput.magnitude < 0.1f )
                {
                    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Max(playerRigidbody.velocity.y, Physics.gravity.magnitude*gravityScale*Time.fixedDeltaTime), playerRigidbody.velocity.z);
                }
                //Otherwiswe, clamp the velocity.
                else
                {
                    playerRigidbody.velocity = Vector3.ClampMagnitude(playerRigidbody.velocity,maximumMovementSpeed);
                }
            }
        }
    }

    void UpdateAnimatorAndSound()
    {
        // Animations
        animatorWalkSpeedParameter = movementDirection.magnitude*maximumMovementSpeed/(baseMovementSpeed*runSpeedMultiplier); 
        playerAnimator.SetBool("Fall", !playerCloseToGround);
        playerAnimator.SetBool("Slide", playerSliding);        
        playerAnimator.SetFloat("Walk Speed",animatorWalkSpeedParameter); 
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

}