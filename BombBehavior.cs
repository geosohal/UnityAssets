using System;
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
	private float secondsTillUpdate = .05f; // based on how long server delays in sec to update projectile pos
	private AudioSource source;

	public AudioClip explodeSound;
	
	public void Initialize(Item _item)
	{
		this.item = _item;
		this.name = _item.Id;
		firstUpdate = true;
		source = GetComponent<AudioSource>();
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
        RunBehaviour rb = (RunBehaviour)GameObject.FindWithTag("GameController").GetComponent<RunBehaviour>();
        foreach (ParticleSystem particleSystem in particlePrefabs)
		{
			particleSystem.Play();
            rb.Expirableffects.Add(new Tuple<GameObject,float>(particleSystem.gameObject,1f));
		}
        

        float distToClientPlayer2 = (this.transform.position -
		                             rb.clientsPlayer.transform.position).sqrMagnitude;


		float volume = Mathf.Lerp(1, 0, distToClientPlayer2 / 10000f);
		
		source.PlayOneShot(explodeSound, volume);
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
            {
                Destroy(this.gameObject);
                return;
            }

		}
		lastTime = Time.time;
	}
}
