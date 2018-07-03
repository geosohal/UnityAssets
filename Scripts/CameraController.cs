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
	public float lookAheadAmount;

	public float backFromPlayer;

	private Vector3 velocity = Vector3.zero;
	private float heightLeftOverVel = 0;


	public bool isThirdPerson;
	public bool isSemiThirdPerson;
    public bool isBuildMode;

    // build mode related members:
    public float turnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
    public float panSpeed = 4.0f;       // Speed of the camera when being panned
    public float zoomSpeed2 = 4.0f;      // Speed of the camera going back and forth
    private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
    private bool isPanning;     // Is the camera being panned?
    private bool isRotating;    // Is the camera being rotated?
    private bool isZooming;     // Is the camera zooming?

    private void Start()
    {
        isBuildMode = false;
    }
    // methods
    void Update()
	{
        if (isBuildMode)
        {
            UpdateBuildModeCam();
            return;
        }
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

    void UpdateBuildModeCam()
    {
        Camera currcam = this.GetComponent<Camera>();
        // Get the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isRotating = true;
        }

        // Get the right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isPanning = true;
        }

        // Get the middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isZooming = true;
        }

        // Disable movements on button release
        if (!Input.GetMouseButton(0)) isRotating = false;
        if (!Input.GetMouseButton(1)) isPanning = false;
        if (!Input.GetMouseButton(2)) isZooming = false;

        // Rotate camera along X and Y axis
        if (isRotating)
        {
            Vector3 pos = currcam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
            transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);
        }

        // Move the camera on it's XY plane
        if (isPanning)
        {
            Vector3 pos = currcam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
            transform.Translate(move, Space.Self);
        }

        // Move the camera linearly along Z axis
        if (isZooming)
        {
            Vector3 pos = currcam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = pos.y * zoomSpeed2 * transform.forward;
            transform.Translate(move, Space.World);
        }
    }

	public void SetPlayerPos()
	{
        if (isBuildMode)
            return;
		Vector3 pos = new Vector3();
		
	//	pos.x = playerShip.transform.position.x + xOffset;
		Vector3 ourLookAtPos;
		
		if (isThirdPerson)
		{
			pos = playerShip.transform.position + playerShip.transform.forward * -1f * backFromPlayer;

			pos.y = playerShip.transform.position.y + height;
			ourLookAtPos  = playerShip.transform.position + playerShip.transform.forward * lookAheadAmount;
			transform.position = pos;//Vector3.SmoothDamp(transform.position, pos, ref velocity, smooth);
			transform.rotation = Quaternion.LookRotation(ourLookAtPos - transform.position);
		}
		else
		{
			pos.x = playerShip.transform.position.x + xOffset;
			pos.y = playerShip.transform.position.y + height;
			pos.z = playerShip.transform.position.z + zOffset;

			transform.position = pos;//Vector3.SmoothDamp(transform.position, pos, ref velocity, smooth);
			if (isSemiThirdPerson)
				ourLookAtPos = playerShip.transform.position + playerShip.transform.forward * lookAheadAmount;
			else
				ourLookAtPos = playerShip.transform.position;
			transform.rotation = Quaternion.LookRotation(ourLookAtPos - transform.position);
		}
	}

    public void SetPositionOffsetFromPoint(Vector3 point, Vector3 offset)
    {
        transform.position = point + offset;
    }
    public void SetLookAtPos(Vector3 ourLookAtPos)
    {
        transform.rotation = Quaternion.LookRotation(ourLookAtPos - transform.position);
    }
}
