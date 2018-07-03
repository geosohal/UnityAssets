using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIController : MonoBehaviour {

    CameraController mainCam;
    bool isBuildMode;
    Button btnBigDisk;
    Button btnSmDisk;
    Button btnRoom;
    Button btnHub;
    Button btnNorth;
    Button btnEast;
    Button btnSouth;
    Button btnWest;
    Button btnBigLongBridge;
    Button btnThinLongBridge;
    Button btnThinBridge;
    Button btnBigBridge;
    Button btnUp;
    Button btnDown;
    Button btnTop;
    Button btnBottom;
    Button btnTurret;
    // Use this for initialization
    void Start () {
        isBuildMode = false;
        btnBigDisk = ((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>());
        btnBigDisk.interactable = false;
        btnSmDisk = ((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>());
        btnSmDisk.interactable = false;
        btnRoom = ((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>());
        btnRoom.interactable = false;
        btnHub = ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>());
        btnHub.interactable = false;
        btnNorth = ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>());
        btnNorth.interactable = false;
        btnEast = ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>());
        btnEast.interactable = false;
        btnSouth = ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>());
        btnSouth.interactable = false;
        btnWest = ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>());
        btnWest.interactable = false;
        btnBigLongBridge = ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>());
        btnBigLongBridge.interactable = false;
        btnThinLongBridge = ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>());
        btnThinLongBridge.interactable = false;
        btnThinBridge = ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>());
        btnThinBridge.interactable = false;
        btnBigBridge = ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>());
        btnBigBridge.interactable = false;
        btnUp = ((Button)GameObject.FindGameObjectWithTag("btnUp").GetComponent<Button>());
        btnDown = ((Button)GameObject.FindGameObjectWithTag("btnDown").GetComponent<Button>());
        btnTop = ((Button)GameObject.FindGameObjectWithTag("btnTop").GetComponent<Button>());
        btnBottom = ((Button)GameObject.FindGameObjectWithTag("btnBottom").GetComponent<Button>());
        mainCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
        ((Toggle)GameObject.FindGameObjectWithTag("toggleBuild").GetComponent<Toggle>()).isOn = isBuildMode;
        ((Toggle)GameObject.FindGameObjectWithTag("toggleBuild").GetComponent<Toggle>()).onValueChanged.AddListener((value) => this.OnToggleSwitch(value));
        btnTurret = ((Button)GameObject.FindGameObjectWithTag("btnTurret").GetComponent<Button>());
       // btnTurret.interactable = false;
        ToggleAllButtons(false);
    }
	
    void OnToggleSwitch(bool val)
    {
        ToggleAllButtons(val);
        mainCam.isBuildMode = val;
        isBuildMode = val;
        ((RunBehaviour)GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>()).SetBuildMode( val);

        if (isBuildMode)
        {
            
            SwitchCamToBuildMode();
            SpaceStation ss = ((SpaceStation)GameObject.FindGameObjectWithTag("SpaceStation").GetComponent<SpaceStation>());
            ss.InitializeButtons();
            ss.stationMan.ReportStationStatsToHUD();
        }
    }

    // move camera up and at more of an angle to player or if it exists, the spacestation's selected node
    void SwitchCamToBuildMode()
    {
        mainCam.isBuildMode = true;
        StationNode currNode = ((SpaceStation)GameObject.FindGameObjectWithTag("SpaceStation").GetComponent<SpaceStation>()).selectedNode;
        if (currNode != null)
        {
            mainCam.SetPositionOffsetFromPoint(currNode.selectedHub.pos, new Vector3(70f, 70f, 70f));
            mainCam.SetLookAtPos(currNode.selectedHub.pos);
        }
    }

    
    void ToggleAllButtons(bool val)
    {
        btnBigDisk.gameObject.SetActive(val);
        btnSmDisk.gameObject.SetActive(val);
        btnRoom.gameObject.SetActive(val);
        btnHub.gameObject.SetActive(val);
        btnNorth.gameObject.SetActive(val);
        btnEast.gameObject.SetActive(val);
        btnSouth.gameObject.SetActive(val);
        btnWest.gameObject.SetActive(val);
        btnBigLongBridge.gameObject.SetActive(val);
        btnThinLongBridge.gameObject.SetActive(val);
        btnThinBridge.gameObject.SetActive(val);
        btnBigBridge.gameObject.SetActive(val);
        btnUp.gameObject.SetActive(val);
        btnDown.gameObject.SetActive(val);
        btnTop.gameObject.SetActive(val);
        btnBottom.gameObject.SetActive(val);
        btnTurret.gameObject.SetActive(val);
    }
	// Update is called once per frame
	void Update () {
		if (isBuildMode)
        {
            
        }
	}
}
