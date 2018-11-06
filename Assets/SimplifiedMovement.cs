using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifiedMovement : MonoBehaviour 
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
	private Vector2 inputMovement;
	private bool inputJump;
	private bool inputRun;

	// Movement Variables
	private new Transform camera;
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
	public bool isGrounded;
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
		layerMask = GlobalData.EnvironmentLayerMask.value;

		// Get External variables
		camera = GlobalData.PlayerCamera.transform;
	}

	void Update()
	{
		// inputJump acts as a flag because the jump is performed in FixedUpdate()
		if (Input.GetButton(InputAxis.Jump)) { inputJump = true;}
	}

	void FixedUpdate()
	{
		// Limit the movement to a maximum magnitude of 1f.
		inputMovement.x = Input.GetAxis(InputAxis.HorizontalMovement);
		inputMovement.y = Input.GetAxis(InputAxis.VerticalMovement);
		Vector2.ClampMagnitude(inputMovement, 1f);
		
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

		// Just a boolean used as modifier.
		inputRun = Input.GetButton(InputAxis.Run);





		movementSpeed = (inputRun) ? baseSpeed*runMultiplier: baseSpeed;
		capsuleCenter = rigidbody.position + capsuleCollider.center;

		


		// Ground Test
		isGrounded = false;
		bool isSliding = false;
		rigidbodyBeneath = null;
		groundNormal = Vector3.zero;
		RaycastHit[] hits = CapsuleCastAll(Vector3.down,radius);
		

		if (hits.Length > 0) 
		{
			// If any of the platforms below the player is close enough to the origin
			// of the bottom sphere, it means that is completely grounded.
			Vector3 point2 = capsuleCenter - pointOffset;
			foreach(RaycastHit hit in hits)
			{
				if ((hit.point - point2).magnitude < radius+0.01f )
				{
					isGrounded = true;
					jumping = false;
					groundNormal = hit.normal;

					if (Vector3.Angle(groundNormal,Vector3.up) >= 45f)
					{
						isSliding = true;
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
							//
							positionBeneath = Vector3.zero;
						}
					}
					else
					{
						//
						positionBeneath = Vector3.zero;
					}

					break;
				}
			}
		}
		else
		{
			//
			positionBeneath = Vector3.zero;			
		}

		// Constraint Movement Direction
		if (inputMovement.magnitude > 0)
		{
			hits = CapsuleCastAll(movementDirection,movementSpeed*Time.fixedDeltaTime);

			foreach(RaycastHit hit in hits)
			{
				if (Vector3.Angle(Vector3.up,hit.normal)>=45f )
				{
					movementDirection -= Vector3.Project(movementDirection,Vector3.ProjectOnPlane(hit.normal,Vector3.up).normalized);
				}
				// We only check the first one
				break;
			}
		}

		// Apply velocity
		rigidbody.velocity = movementDirection*movementSpeed +  Vector3.up*rigidbody.velocity.y;

		// Project Velocity Direction
		if (isGrounded && !jumping )
		{
			rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundNormal);
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
			
			if (isGrounded && !isSliding)
			{
				// If the player is not moving, use a regular jump
				if (inputMovement.Equals(Vector3.zero))
				{
					rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up) + Vector3.up*jumpSpeed;
				}
				// otherwise, make the jump ground dependent
				else
				{
					rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundNormal) + groundNormal*jumpSpeed;
				}
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
	}
	
	RaycastHit[] CapsuleCastAll(Vector3 direction, float distance)
	{
		return Physics.CapsuleCastAll(capsuleCenter+pointOffset,capsuleCenter-pointOffset,radius*radiusScale,direction,distance,layerMask); 
	}
}


// Mirar lo de las plataformas debajo del jugador. Se puede intentar simplificar el código guardando el último rigidbody.
