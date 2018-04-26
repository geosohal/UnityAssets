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
	private float secondsTillUpdate = .012f; // based on how long server delays in sec to update bullet pos

	
	// Use this for initialization
	void Start () {
		
	}

	public void Initialize(Item _item)
	{
		this.item = _item;
		this.name = _item.Id;
		firstUpdate = true;
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
		}
		
		if (newPos != transform.position)
		{
			// Debug.Log("move lerp: " + newPos);
			// move smoothly
			float lerpT = (Time.time - this.lastMoveUpdateTime)/secondsTillUpdate;
			transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
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
}
