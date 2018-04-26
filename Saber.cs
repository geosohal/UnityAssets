using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using Photon.MmoDemo.Common;
using UnityEngine;

public class Saber : MonoBehaviour
{
	private bool isSaberOn;
	private GameObject particle;

	public GameObject particlePrefab;
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
		}
		else if (!isSaberOn && gameObject.GetComponent<MeshRenderer>().enabled == true)
		{
			isSaberOn = true;
			particle = Instantiate(particlePrefab, this.transform.position, Quaternion.AngleAxis(180, Vector3.up));
			particle.transform.localScale = new Vector3(.025f, .025f, .025f);
			Vector3 particlePos = this.transform.position;
			particlePos.z = particlePos.z + particleZoffset;
			particle.transform.position = particlePos;
		}

		if (isSaberOn)
		{
			Vector3 particlePos = this.transform.position;
			particlePos += this.transform.forward * particleZoffset;
			particle.transform.position = particlePos;
			particle.transform.rotation = this.transform.rotation;
		}
	}
}
