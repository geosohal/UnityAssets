using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Enemy : MonoBehaviour
{

	public float health;
	public GameObject player;
	public float numPoints;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
		if (health <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		print("enemy " + this.gameObject.name + " has died");
		Destroy(this.gameObject);

		//player.GetComponent<Player>().points += numPoints;
	}
}
