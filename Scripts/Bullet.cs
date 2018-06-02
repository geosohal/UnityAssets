using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using UnityEngine;

public class Bullet : MonoBehaviour
{

	public float speed = 7f;
	public float maxTime = 5f;
	public float currTime = 0;
	public float damage = 1f;
	public uint playerID; // player who created bullet

	private Item item;
	private float lastMoveUpdateTime;
	private Vector3 lastMoveUpdate;
	private bool firstUpdate;
	private float secondsTillUpdate = .05f; // based on how long server delays in sec to update bullet pos

	public static int pbuffsize = 80;
	private NStateBuffer nbuffer;
	
	private GameObject vectorGrid;
	private VectorGrid gridComponent;
	private Vector2 velocityEstimate; // estimate of velocity used for grid force
	
	// Use this for initialization
	void Start () {
		
	}

	public void Initialize(Item _item)
	{
		this.item = _item;
		this.name = _item.Id;
		firstUpdate = true;
		nbuffer = new NStateBuffer(pbuffsize);
		Show(false);
		
		vectorGrid = GameObject.FindWithTag("vectorgrid");
		gridComponent = vectorGrid.GetComponentInChildren<VectorGrid>();
		velocityEstimate = new Vector2(item.Rotation.X, item.Rotation.Y)*2f;
	}
	
	// Update is called once per frame
	void Update () {
		
		// if item was destroyed
		if (this.item == null)
		{
			Debug.Log("bullet waz destroyed");
			//Destroy(this);
			return;
			
		}


		Vector3 newPos = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y)*RunBehaviour.WorldToUnityFactor;

		if (firstUpdate)
		{
			transform.position = newPos;
			firstUpdate = false;
		}


		if (newPos != this.lastMoveUpdate)
		{
			this.lastMoveUpdate = newPos;
			this.lastMoveUpdateTime = Time.time;
			
			nbuffer.AddNetworkState(newPos,lastMoveUpdateTime);
			if (nbuffer.posSetCount > 0)
				Show(true);
			ApplyGridForce(velocityEstimate,5);
		}
		
		transform.position =  nbuffer.GetRewindedPos(Time.time - .1f);
		
//		if (newPos != transform.position)
//		{
//			// Debug.Log("move lerp: " + newPos);
//			// move smoothly
//			float lerpT = (Time.time - this.lastMoveUpdateTime)/secondsTillUpdate;
//			transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
//		}



	}
	
	public void ApplyGridForce(float force, float radius)
	{
		if (vectorGrid != null)
			gridComponent.AddGridForce(this.transform.position, force, radius, Color.red, true, false);
	}
	public void ApplyGridForce(Vector2 force, float radius)
	{
		if (vectorGrid != null)
		{
			Color alphared = Color.red;
			alphared.a = .2f;
			gridComponent.AddGridForce(this.transform.position, force, radius, alphared, true, true);
		}
	}



	private void OnCollisionEnter2D(Collision2D collision)
	{
		// MOVED TO SERVER
		/*
		if (collision.rigidbody.tag == "Enemy")
		{
			triggeringEnemy = collision.rigidbody.gameObject;
			triggeringEnemy.GetComponent<Enemy>().health -= damage;
			Destroy(this.gameObject);
		}

		if (collision.rigidbody.tag == "Asteroid")
		{
			GameObject triggeringAst = collision.rigidbody.gameObject;
			triggeringAst.GetComponent<Asteroid>().InflictDamage(damage);
			Destroy(this.gameObject);
		}*/
	}
/*
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Enemy")
		{
			triggeringEnemy = other.gameObject;
			triggeringEnemy.GetComponent<Enemy>().health -= damage;
			Destroy(this.gameObject);
		}

		if (other.tag == "Asteroid")
		{
			GameObject triggeringAst = other.gameObject;
			triggeringAst.GetComponent<Asteroid>().health -= damage;
			Destroy(this.gameObject);
		}
	}*/
	
	
	private bool Show(bool show)
	{
		var renderers = GetComponentsInChildren<Renderer>();
		if (renderers[0].enabled != show)
		{
			foreach (var render in renderers)
			{
				render.enabled = show;
			}

			return true;
		}
		return false;
	}
}
