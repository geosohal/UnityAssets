using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// generates random asteroid prefabs along the x axis, with a velocity primarily in the -y direction
public class AsteroidMaker : MonoBehaviour
{

	public float xradius = 10f; // new asteroids get a random position xradius from the x position of the maker
	public float speedX = 0f;
	public float speedVarianceX = .1f;
	public float speedY = 1f;
	public float speedVarianceY = .3f;
	public float timeToMake = .6f;
	public GameObject[] asteroids;
    public float torqueRange = 1;

	private float timeSinceLastMake = float.MaxValue;
    RunBehaviour rb;
	
	
	// Use this for initialization
	void Start ()
	{
        rb = GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>();
    }
	
	// Update is called once per frame
	void Update ()
	{
		timeSinceLastMake += Time.deltaTime;
		// if enough time elapsed to make asteroid
		if (timeSinceLastMake > timeToMake)
		{
			int numAsteroids = asteroids.Length;
			int randIndex = Random.Range(0, numAsteroids - 1);

			Transform asteroidTransform = this.transform;
			Vector3 astPos = 
				new Vector3(transform.position.x + Random.RandomRange(-xradius, xradius), transform.position.y, transform.position.z);
			GameObject newAsteroid = Instantiate(asteroids[randIndex], astPos, Quaternion.identity);
			float xVel = speedX + Random.RandomRange(-speedVarianceX, speedVarianceX);
			float yVel = speedY + Random.RandomRange(-speedVarianceY, speedVarianceY);
			Vector3 asteroidVel =  new Vector3(xVel,0,yVel);
			newAsteroid.GetComponent<Rigidbody>().velocity = asteroidVel;
            newAsteroid.GetComponent<Rigidbody>().AddTorque(
                Random.RandomRange(-torqueRange, torqueRange), Random.RandomRange(-torqueRange, torqueRange), Random.RandomRange(-torqueRange, torqueRange));
			timeSinceLastMake = 0;
            rb.asteroids.Add(newAsteroid.GetComponent<Asteroid>());
		}
	}
}
