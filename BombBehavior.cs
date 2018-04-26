using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using UnityEngine;

public class BombBehavior : MonoBehaviour
{
	private ParticleSystem[] particlePrefabs;

	private bool exploded;

	private float timeTillDestroy = 4f;

	private float lastTime;
	
	private Item item;
	private bool firstUpdate;
	private float lastMoveUpdateTime;
	private Vector3 lastMoveUpdate;
	private float secondsTillUpdate = .012f; // based on how long server delays in sec to update projectile pos
	
	public void Initialize(Item _item)
	{
		this.item = _item;
		this.name = _item.Id;
		firstUpdate = true;
	}
	
	// Use this for initialization
	void Start ()
	{
		exploded = false;
		particlePrefabs = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in particlePrefabs)
		{
			particleSystem.Stop();
		}
	}

	public void Explode()
	{
		GetComponent<MeshRenderer>().enabled = false;
		foreach (ParticleSystem particleSystem in particlePrefabs)
		{
			particleSystem.Play();
		}

		exploded = true;
	}
	// Update is called once per frame
	void Update () {

		// if item was destroyed
		if (this.item == null && !exploded)
		{
			Debug.Log("bomb waz destroyed");
			Explode();
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
		
		
		if (exploded)
		{
			float elapsedSec = Time.time - lastTime;
			timeTillDestroy -= elapsedSec;

			if (timeTillDestroy < 0)
				Destroy(this);

		}
		lastTime = Time.time;
	}
}
