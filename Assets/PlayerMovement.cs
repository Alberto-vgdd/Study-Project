using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
	[Header("Character Configuration")]
	public float baseSpeed = 4f;
	public float runMultiplier = 2f;
	public float jumpSpeed = 10f;
	public float gravityMultiplier = 3f;
	public float maxFallingSpeed = 12.5f;
	public float rotationSpeed = 5f;

	// Character components
	private new Rigidbody rigidbody;
	private CapsuleCollider capsuleCollider;
	private Animator animator;

	// Input variables
	private bool inputEnabled;
	private Vector2 inputMovement;
	private bool inputJump;
	private bool inputRun;

	// Movement Variables
	private new Transform camera;
	//private Vector3 plane = new Vector3(1,0,1);
	private Vector3 movementHorizontal;
	private Vector3 movementVertical;
	private Vector3 movementDirection;
	private float movementSpeed;

	//Capsule casts parameters
    private Vector3 capsuleCenter;
	private Vector3 pointOffset;
	private float radius;
	private float radiusScale;
	private int layerMask;

	// Ground Parameters
	public bool closeToGround;
	public bool grounded;
	public bool sliding;
	public bool jumping;
	private Vector3 groundNormal;

	// Other
	private Rigidbody rigidbodyBeneath;
	private Vector3 positionBeneath;


    void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		animator = GetComponentInChildren<Animator>();
	}

	void Start()
	{
		// Initialise the common capsule casts parameters
		radius = capsuleCollider.radius;
		radiusScale = 0.99f;
		pointOffset = Vector3.up*( capsuleCollider.height / 2 - radius);
		layerMask = 1 << (int) Mathf.Log(GlobalData.EnvironmentLayerMask.value,2);

		// Get External variables
		camera = GlobalData.PlayerCamera.transform;

		// Enable Inputs at the beginning
		inputEnabled = true;
	}

	void Update()
	{
		// Read movement inputs
		if (inputEnabled)
		{
			// Limit the movement to a maximum magnitude of 1f.
			inputMovement.x = Input.GetAxis(InputAxis.HorizontalMovement);
			inputMovement.y = Input.GetAxis(InputAxis.VerticalMovement);
			Vector2.ClampMagnitude(inputMovement, 1f);

			// inputJump acts as a flag because the jump is performed in FixedUpdate()
            if (Input.GetButton(InputAxis.Jump)) { inputJump = true;}

			// Just a boolean used as modifier.
			inputRun = Input.GetButton(InputAxis.Run);
		}
		else
		{
			inputMovement = Vector2.zero; inputJump = false; inputRun = false;
		}
		
		
		// Determine movement axes
		if (!GlobalData.IsEnemyLocked)
		{
			movementHorizontal = Vector3.ProjectOnPlane(camera.right, Vector3.up).normalized;
            movementVertical = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
		}
		else
		{
			movementVertical = Vector3.ProjectOnPlane(GlobalData.LockedEnemyTransform.position - transform.position, Vector3.up).normalized;
            movementHorizontal = Vector3.Cross( Vector3.up, movementVertical).normalized;
		}
		movementDirection = movementHorizontal * inputMovement.x + movementVertical * inputMovement.y;
	}

	void FixedUpdate()
	{
		movementSpeed = (inputRun) ? baseSpeed*runMultiplier: baseSpeed;
		capsuleCenter = rigidbody.position + capsuleCollider.center;

		closeToGround = false;
		grounded = false;
		sliding = false;

		rigidbodyBeneath = null;

		groundNormal = Vector3.zero;


		// Ground Test. If there's no platform below at a max. distance of a step,
		// then the character is not grounded. if there's it doesn't necessarily implies 
		// that the user is touching the ground, it is just close to it.
		RaycastHit[] hits = CapsuleCastAll(Vector3.down,radius);

		if (hits.Length > 0) 
		{
			closeToGround = true;

			// If any of the platforms below the player is close enough to the origin
			// of the bottom sphere, it means that is completely grounded.
			Vector3 point2 = capsuleCenter - pointOffset;
			foreach(RaycastHit hit in hits)
			{
				if ((hit.point - point2).magnitude < radius+0.01f )
				{
					grounded = true;
					jumping = false;

					groundNormal = hit.normal;
			
					if (Vector3.Angle(Vector3.up,groundNormal) >= 45f )
					{
						sliding = true;
					}

					
					if (hit.rigidbody != null)
					{
						rigidbodyBeneath = hit.rigidbody;

						if ( inputMovement.magnitude < 0.01f )
						{
							if (positionBeneath == Vector3.zero)
							{
								positionBeneath = hit.transform.InverseTransformPoint(rigidbody.position);	
							}
						}
						else
						{
							positionBeneath = Vector3.zero;
						}
					}

					break;
				}
			}
		}



		// Project movementDirection on groundNormal
		if(grounded && !jumping && inputMovement.magnitude > 0)
		{
			movementDirection = Vector3.ProjectOnPlane(movementDirection, groundNormal);
		}

		// Constraint Movement Direction
		if (inputMovement.magnitude > 0)
		{
			hits = CapsuleCastAll(movementDirection,movementSpeed*Time.fixedDeltaTime);

			foreach(RaycastHit hit in hits)
			{
				if ( Vector3.Angle(Vector3.up,hit.normal)>=45f)
				{
					movementDirection -= Vector3.Project(movementDirection,Vector3.ProjectOnPlane(hit.normal,Vector3.up).normalized);
				}
				// We only check the first one
				break;
			}
		}

		// Apply velocity
		rigidbody.velocity = movementDirection*movementSpeed + Vector3.up*rigidbody.velocity.y;

		// Project Velocity Direction
		if (grounded && !jumping )
		{
			rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundNormal);

			if (sliding)
            {  
				rigidbody.velocity -= Vector3.Project(rigidbody.velocity, Vector3.ProjectOnPlane(groundNormal,Vector3.up));
                rigidbody.velocity = new Vector3(rigidbody.velocity.x,  -5f, rigidbody.velocity.z);
            }
		}

		// Apply beneath platform's velocity & rotation
		if (rigidbodyBeneath != null)
		{
			rigidbody.velocity += rigidbodyBeneath.velocity;
			
			if ( inputMovement.magnitude < 0.01f )
			{
				rigidbody.rotation = rigidbody.rotation*Quaternion.Euler(rigidbodyBeneath.angularVelocity);
				rigidbody.velocity += (rigidbodyBeneath.transform.TransformPoint(positionBeneath) - rigidbody.position) / Time.fixedDeltaTime;;
			}
				
		}
		
		// Jump
		if (inputJump)
		{
			inputJump = false;
			
			if (grounded && !sliding)
			{
				rigidbody.velocity += Vector3.up*jumpSpeed;
				jumping = true;
			}
		}

		// Gravity
		rigidbody.AddForce(Physics.gravity*gravityMultiplier, ForceMode.Acceleration);

		// Rotate the character
		Quaternion targetRotation = Quaternion.RotateTowards(rigidbody.rotation,Quaternion.LookRotation(Vector3.ProjectOnPlane(movementDirection,Vector3.up),Vector3.up),rotationSpeed*360f*Time.fixedDeltaTime);

		if (movementDirection.magnitude > 0.1f)
		{
			// Speed of the turn
			rigidbody.maxAngularVelocity = float.MaxValue;
			Quaternion rotation = targetRotation * Quaternion.Inverse(rigidbody.rotation);
			rigidbody.AddTorque(rotation.x / Time.fixedDeltaTime, rotation.y / Time.fixedDeltaTime, rotation.z / Time.fixedDeltaTime, ForceMode.VelocityChange);
		}
	
		

		// Stop additional rotation.
		rigidbody.angularVelocity = Vector3.zero;

		// Update Animations
        animator.SetBool("Fall", !closeToGround);
        animator.SetBool("Slide", sliding);        
        animator.SetFloat("Walk Speed", movementDirection.magnitude*movementSpeed/(baseSpeed*runMultiplier)); 
	}
	
	RaycastHit[] CapsuleCastAll(Vector3 direction, float distance)
	{
		return Physics.CapsuleCastAll(capsuleCenter+pointOffset,capsuleCenter-pointOffset,radius*radiusScale,direction,distance,layerMask); 
	}
}

// Rotation when sliding (Currently not facing the slope)

// Step walking, look at the old script, check how the "wrong sliding state" was fixed there

// Fix a teleport that appears when landing after a jump while moving.
// Constraing velocity after adding the beneath platforms's. (Similarly to constraint movement direction, right after adding it).
// Elevator

// Antimation: Only transition to  landing  if the player is not also sliding
