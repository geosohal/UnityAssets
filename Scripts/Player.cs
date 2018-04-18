using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyEnums;

public class Player : MonoBehaviour
{

	public float turnSpeed = 7f;
	
	public float points;
	
	public float thrustForce = .6f;
	public float strafeThrustForce = .1f;
	public float inertia = .98f;	// range [0. 1]
	public float maxVelSq = 10f;
	public uint freeHydrogen = 0;
	
	private Vector2 velocity;
	
	
	
	public float MoveSpeed = 5f;
	private OwnerInfo ownerInfo;
	private GameActor actorInfo;
	private bool isMine = false;
	private Vector3 lastReceivedMove;
	private float timeOfLastMoveCmd = 0f;
	
	// Use this for initialization
	void Start ()
	{
		timeOfLastMoveCmd = Time.time;
		lastReceivedMove = transform.position;
		//ownerInfo = GetComponent<OwnerInfo>();
		//actorInfo = GetComponent<GameActor>();
		//isMine = ( ownerInfo.OwnerID == StarCollectorClient.PlayerID );
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		
		// make ship face mouse
	/*	Plane playerPlane = new Plane(Vector3.up, transform.position);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float hitDist = 0.0f;

		if (playerPlane.Raycast(ray, out hitDist))
		{
			Vector3 targetPoint = ray.GetPoint(hitDist);
			targetPoint.y = transform.position.y;
			Vector3 newForward = targetPoint - transform.position;
			GetComponent<ItemBehaviour>().SetRotation(newForward);
			transform.rotation = Quaternion.LookRotation(newForward.normalized, Vector3.up);
//			Quaternion targetRotation = Quaternion.LookRotation(newForward.normalized, Vector3.up);*/
//			targetRotation.x = 0;
//			targetRotation.z = 0;
//			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		//}

/*		Vector2 prevVelocity = velocity;
		// ship thrusting
		if (Input.GetKey(KeyCode.W))
			velocity = new Vector2(velocity.x + thrustForce*transform.up.x, velocity.y + thrustForce*transform.up.z);
		if (Input.GetKey(KeyCode.A))
			velocity = new Vector2(velocity.x - thrustForce*transform.right.x, velocity.y-thrustForce*transform.right.z);
		if (Input.GetKey(KeyCode.S))
			velocity =  new Vector2(velocity.x - thrustForce*transform.up.x, velocity.y - thrustForce*transform.up.z);
		if (Input.GetKey(KeyCode.D))
			velocity = new Vector2(velocity.x + thrustForce*transform.right.x, velocity.y+thrustForce*transform.right.z);
		
		// cancel thrust application if velocity of ship is maxed
		float velLengthSq = velocity.sqrMagnitude;
		if (velLengthSq > maxVelSq || velLengthSq < -maxVelSq)
			velocity = prevVelocity;

		velocity *= inertia;
		
		transform.position = transform.position + (new Vector3(velocity.x, 0, velocity.y) * Time.deltaTime);
		*/
//		if (isMine)
//		{
//			// get movement direction
//			float mX = Input.GetAxis( "Horizontal" ) * MoveSpeed;
//			float mY = Input.GetAxis( "Vertical" ) * MoveSpeed;
//			if( Time.time >= timeOfLastMoveCmd + 0.1f )
//			{
//				timeOfLastMoveCmd = Time.time;
//				// send move command to server every 0.1 seconds
//				Dictionary<byte, object> moveParams = new Dictionary<byte,
//					object>();
//				moveParams[ 0 ] = actorInfo.ActorID;
//				moveParams[ 1 ] = mX;
//				moveParams[ 2 ] = mY;
//				StarCollectorClient.Connection.OpCustom( (byte)
//					StarCollectorRequestTypes.MoveCommand, moveParams, false );
//			}
//		}
//		
//		// lerp toward last received position
//		transform.position = Vector3.Lerp( transform.position,
//			lastReceivedMove, Time.deltaTime * 20f );
		
		

	}

	void UpdatePosition( Vector3 newPos )
	{
		lastReceivedMove = newPos;
	}
	
	private void OnCollisionEnter2D(Collision2D collision)
	{
	/*	if (collision.gameObject.tag == "Free Hydrogen")
		{
			GameObject hydroObj = collision.gameObject;
			freeHydrogen += hydroObj.GetComponent<FreeHydrogen>().quantity;
			Destroy(hydroObj);
		}*/
	}

}
