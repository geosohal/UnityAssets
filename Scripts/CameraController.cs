using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Common;
using UnityEngine;

public class CameraController : MonoBehaviour {

	// members
	public GameObject playerShip;
	public float smooth = 0.3f;
	public float height = 20f;
	public float zOffset = 8f;
	public float xOffset = 3f;
	public float minHeight = 6f;
	public float maxHeight = 60f;
	public float zoomSpeed = 40f;

	private Vector3 velocity = Vector3.zero;
	private float heightLeftOverVel = 0; 



	// methods
	void Update()
	{
		if (!playerShip)
			playerShip = GameObject.FindWithTag("Player");
		if (!playerShip)
			return;
//		


//		transform.position = pos;
//
//		Vector3 lookatDir = Vector3.Normalize(playerShip.transform.forward + new Vector3(0, -height/maxHeight, 0));
//		transform.rotation = Quaternion.LookRotation(lookatDir);

		//SetPlayerPos();
		
		float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		if (mouseScroll != 0)
		{
			float newHeight = height - zoomSpeed * mouseScroll;
			heightLeftOverVel += mouseScroll;
			if (newHeight < maxHeight && newHeight > minHeight)
				height = newHeight+heightLeftOverVel;
			heightLeftOverVel *= .85f;
		}
	}

	public void SetPlayerPos()
	{
		Vector3 pos = new Vector3();
		
		pos.x = playerShip.transform.position.x + xOffset;
		pos.y = playerShip.transform.position.y + height;
		pos.z = playerShip.transform.position.z + zOffset;

		transform.position = pos;//Vector3.SmoothDamp(transform.position, pos, ref velocity, smooth);
		transform.rotation = Quaternion.LookRotation(playerShip.transform.position - transform.position);
	}
	

}
