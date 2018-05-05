using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using Photon.MmoDemo.Common;
using UnityEngine;

public class Saber : MonoBehaviour
{
	private bool isSaberOn;
	private GameObject particle;
	private GameObject particle2;

	public GameObject particlePrefab;
	public GameObject particle2Prefab;
	public float particleZoffset = -1f;

	// Use this for initialization
	void Start ()
	{
//		if (gameObject.GetComponent<MeshRenderer>().enabled)
//			isSaberOn = true;
//		else
//		{
//			gameObject.GetComponentInChildren<ParticleSystem>().Clear();
//			gameObject.GetComponentInChildren<ParticleSystem>().Stop();
//		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isSaberOn && gameObject.GetComponent<MeshRenderer>().enabled == false)
		{
			isSaberOn = false;
			Destroy(particle);
			Destroy(particle2);
		}
		else if (!isSaberOn && gameObject.GetComponent<MeshRenderer>().enabled == true)
		{
			isSaberOn = true;
			particle = Instantiate(particlePrefab, this.transform.position, Quaternion.AngleAxis(180, Vector3.up));
			particle.transform.localScale = new Vector3(.005f, .005f, .005f);
			Vector3 particlePos = this.transform.position;
			particlePos.z = particlePos.z + particleZoffset;
			particle.transform.position = particlePos;
			
			particle2 = Instantiate(particle2Prefab, this.transform.position, Quaternion.AngleAxis(180, Vector3.up));
			particle2.transform.localScale = new Vector3(.25f, .05f, .05f);
			particle2.transform.position = particlePos;
		}

		if (isSaberOn)
		{
			Vector3 particlePos = this.transform.position;
			particlePos += this.transform.forward * particleZoffset;
			particle.transform.position = particlePos;
			particle.transform.rotation = this.transform.rotation;
			
			particlePos += this.transform.forward * particleZoffset*-4;
			particle2.transform.position = particlePos;
			particle2.transform.rotation = this.transform.rotation * Quaternion.AngleAxis(90,Vector3.up);
		}
	}
}
