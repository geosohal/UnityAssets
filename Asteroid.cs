using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

	public float health = 30f;

	public float currHealth;
    RunBehaviour rb;
	
	// Use this for initialization
	void Start ()
	{
		currHealth = health;
        rb = GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
		if (currHealth <= 0)
		{
			Die();
		}
	}


	public void InflictDamage(float amount)
	{
		currHealth -= amount;
	}

	public void Die()
	{
		// instantiate energy object
		GameObject hydroObj = (GameObject)Instantiate(Resources.Load("GoldDrop"));
		hydroObj.transform.position = transform.position;
        //hydroObj.GetComponent<FreeHydrogen>().quantity = (uint) (health / 3);
        rb.asteroids.Remove(this);
		
		// remove asteroid object
		Destroy(this.gameObject);
	}
}
