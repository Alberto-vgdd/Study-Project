using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	private bool isGrounded;
	private bool isSliding;
	public bool isJumping;
	private Vector3 groundNormal;

	// External forces/Movemnent
	private Rigidbody beneathRigidbody;
	private Vector3 beneathPosition;
	private List<Rigidbody> externalRigidbodies;


    void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		animator = GetComponentInChildren<Animator>();
	}

	void Start()
	{
		// Initialize the common capsule casts parameters
		radius = capsuleCollider.radius;
		radiusScale = 0.99f;
		pointOffset = Vector3.up*( capsuleCollider.height / 2 - radius);
		layerMask = GlobalData.EnvironmentLayerMask.value;

		// Initialize other variables
		externalRigidbodies = new List<Rigidbody>();

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
		Vector3 point2 = capsuleCenter - pointOffset;
		


		// Ground Test
		// If any of the platforms below the player is close enough to the origin
		// of the bottom sphere, it means that is completely grounded. 
		RaycastHit[] hits = CapsuleCastAll(Vector3.down,radius).Where( hit => (hit.point - point2).magnitude < radius+0.01f ).ToArray();
		if (hits.Length > 0) 
		{
			// Use the closest platform as ground and update the required variables.
			RaycastHit hit = hits[0];
			isGrounded = true;
			isJumping = false;
			groundNormal = hit.normal;
			
			//  Check if the character should slide.
			isSliding = (Vector3.Angle(groundNormal,Vector3.up) >= 45f);

			// Set the variables needed to copy the platform's movement.
			// The position is updated if the character stays still on the platform. (Otherwise, it will move if the platform is rotating)
			beneathRigidbody = hit.rigidbody;
			if ( beneathRigidbody != null && beneathPosition.Equals(Vector3.zero) && inputMovement.magnitude < 0.01f)
			{
				beneathPosition = hit.transform.InverseTransformPoint(rigidbody.position);	
			}
			else if (beneathRigidbody == null || inputMovement.magnitude >= 0.01f)
			{
				beneathPosition = Vector3.zero;
			}
		}
		else
		{
			isGrounded = false;
			isSliding = false;
			groundNormal = Vector3.zero;

			beneathRigidbody = null;
			beneathPosition = Vector3.zero;	
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
		rigidbody.velocity = movementDirection*movementSpeed + Vector3.up*rigidbody.velocity.y;

		// Project Velocity Direction
		if (isGrounded && !isJumping )
		{
			rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, groundNormal);
		}

		// Apply external rigidbodies horizontal velocities.
		foreach( Rigidbody externalRigidbody in externalRigidbodies)
		{
			rigidbody.velocity += Vector3.ProjectOnPlane(externalRigidbody.velocity,Vector3.up);
		}
		// Apply beneath platform's rotation and vertical movement (Because only the beneath platform affects the player's y)
		if (beneathRigidbody != null)
		{
			// PROBLEM Player stick to elevator when ascending
			rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity,Vector3.up) + Vector3.up*beneathRigidbody.velocity.y;
			
			if ( inputMovement.magnitude < 0.01f )
			{
				rigidbody.rotation = rigidbody.rotation*Quaternion.Euler(beneathRigidbody.angularVelocity*Mathf.Rad2Deg*Time.fixedDeltaTime);
				rigidbody.velocity += (beneathRigidbody.transform.TransformPoint(beneathPosition) - rigidbody.position) / Time.fixedDeltaTime;
			}
				
		}

		// Jump
		if (inputJump)
		{
			inputJump = false;
			
			if (isGrounded && !isSliding)
			{
				// Add a jump force. If it is falling, counter the negative vertical speed.
				rigidbody.velocity += Vector3.up*(Mathf.Max(jumpSpeed, jumpSpeed-rigidbody.velocity.y));
				isJumping = true;
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



	void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null)
		{
			externalRigidbodies.Add(collision.rigidbody);
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.rigidbody != null )
		{
			externalRigidbodies.Remove(collision.rigidbody);
		}	
	}
}

// PROBLEM Player stick to elevator when ascending
// is related to the normal of the platform and the way v. velocity is added.