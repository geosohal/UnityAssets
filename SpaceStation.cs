using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    MainConnector, // hub that connects large bridge and other buildings up or down
    Disk,
    Disk2,
    SmallDisk,      // also a hub but only connects small bridges
    Room,      
}

public enum HorizontalBuildingType
{
    LargeBridge,
    LargeBridgeDouble,
    SmallBridge,
    SmallBridgeDouble,
    Turret // can only be built of main connectors at the base height
           //one design option is to allow them on main connectors of other heights but not now
}

public class SpaceStation : MonoBehaviour {

    // set by unity:
    public GameObject UpdateText;

    // STATIC GLOBALS:
    // heights of buildings are from top to bottom, the hub connectors are halfway between top and bottom
    // small disk should be slightly less than half but will work for now.
    public static Dictionary<BuildingType, float> buildingHeightTable;
    public static float bigBridgeLength = 162f;
  //  public static float smallBridgeLength = 122f;//deperecated - needs to be same as other bridge
    public static float maxHeightTop = 30f;
    public static float maxHeightBtm = -200f;

    // OTHER MEMBERS:
    public StationNode selectedNode;
    private int gridSize = 20;  // should keep this an even number
    public StationNode[,] stationGrid;
    private Tuple<int, int> currentStation;
    public bool IsEmpty;
    private bool buildOnBottom; // if false then builder is set to build on top
    private Direction selectedDir;


    public SpaceStation()
    {
    }

	// Use this for initialization
	void Start () {
		if (SpaceStation.buildingHeightTable == null)
        {
            Debug.Log("init height table");
            SpaceStation.buildingHeightTable = new Dictionary<BuildingType, float>();
            SpaceStation.buildingHeightTable.Add(BuildingType.SmallDisk, 20f);
            SpaceStation.buildingHeightTable.Add(BuildingType.Room, 53f);
            SpaceStation.buildingHeightTable.Add(BuildingType.MainConnector, 25f);
            SpaceStation.buildingHeightTable.Add(BuildingType.Disk, 18f);
            SpaceStation.buildingHeightTable.Add(BuildingType.Disk2, 25f);
        }
        stationGrid = new StationNode[gridSize / 2,gridSize / 2];
        currentStation = new Tuple<int, int>(gridSize / 2, gridSize / 2);
        IsEmpty = true;
        buildOnBottom = true;

        ((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = false;


        //((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>()).interactable = false;
        //((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>()).interactable = false;
        //((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).onClick.AddListener(() => this.OnHubButton());
        ((Button)GameObject.FindGameObjectWithTag("btnTop").GetComponent<Button>()).onClick.AddListener(() => this.OnTopSelected());
        ((Button)GameObject.FindGameObjectWithTag("btnBottom").GetComponent<Button>()).onClick.AddListener(() => this.OnBottomSelected());
        ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.North));
        ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.East));
        ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.South));
        ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.West));
        ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = false;
        //((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = false;
        //((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = false;
        //((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = false;

        ((Button)GameObject.FindGameObjectWithTag("btnUp").GetComponent<Button>()).onClick.AddListener(() => this.OnUpPress());
        ((Button)GameObject.FindGameObjectWithTag("btnDown").GetComponent<Button>()).onClick.AddListener(() => this.OnDownPress());
    }

    public void CreateFirstHub(Vector3 pos)
    {
        StationNode newNode = new StationNode();
        newNode.Initialize(true, pos);
        stationGrid[currentStation.first, currentStation.second] = newNode;
        selectedNode = newNode;
    }

    public void OnHubButton()
    {
        if (IsEmpty)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (((ItemBehaviour)player.GetComponent<ItemBehaviour>()).item.IsMine)
                {
                    Vector3 playerPos = player.transform.position;
                    CreateFirstHub(playerPos);
                    OnBottomSelected();
                    continue;
                }
            }
        }
        else
        {


            // note that any hubs we add, besides the first - on a new tower, will ned to check adjacent grid squares to see if they are covered by a bridge
            // or if they are also a hub with the adjacent port used - in both cases it means the new hubs port will need to be set to used upon creation
        }
    }

    public void OnBigLongBridgeButton()
    {
        selectedNode.BuildBridgeOnSelectedHub(selectedDir,null);
        // also need to fill in empty grid slots so that we know the grid slot is covered by a bridge
            // note that any hubs we add, besides the first, will ned to check adjacent grid squares to see if they are covered by a bridge
            // or if they are also a hub with the adjacent port used - in both cases it means the new hubs port will need to be set to used upon creation
        stationGrid[currentStation.first + DirectionEastWest(selectedDir), currentStation.second + DirectionNorthSouth(selectedDir)].
    }



    // update the N S E W buttons to show what ports are allowed
    public void OnHubSelect()
    {
        // note: to build a node out of a hub's open port you first must build a bridge
        //  once there is a bridge then an option appears to build a node (unless one was already there)
        // double length bridges are allowed

        EnableDirections();
        foreach (var port in selectedNode.selectedHub.portsUsed)
        {
            DisableDirection(port.first);
        }

    }

    public void OnUpPress()
    {
        if (stationGrid[currentStation.first, currentStation.second].SelectNextHubUp())
            OnHubSelect();
    }

    public void OnDownPress()
    {
        if (stationGrid[currentStation.first, currentStation.second].SelectNextHubDown())
            OnHubSelect();
    }

    // after a port has been selected, update the bridge buttons to show what bridge types are allowed
    public void OnDirectionSelect(Direction dir)
    {
        // one case where a bridge type wouldnt be allowed is where you try to do a long bridge over a node
        // another case is where you do a bridge and instead of connecting to a node on the other side it 
        // connects to some other building type
        // so we just need to check for these cases:

        StationNode nodeToTest = GetStationInDirection(dir, 1);
        float heightOfSelectedHub = stationGrid[currentStation.first, currentStation.second].selectedHub.pos.y - .5f;
        bool canStepByOne = false;
        
        // test for non-long bridges
        if (nodeToTest != null)
        {
            StationBuilding potentialHub = nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub);
            canStepByOne = true;
            if (potentialHub == null)
            {
                // okay to build any bridge
                ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = true;
                ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
            else if (potentialHub.type == BuildingType.MainConnector)
            {
                // okay to build thick bridge todog
                ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
            else if (potentialHub.type == BuildingType.SmallDisk)
            {
                // okay to build thin bridge
                ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
            else
                canStepByOne = false;

        }
        else
        {
            // can't build any bridge because this is off grid range
            UpdateText.GetComponent<Text>().text = "can't build any bridge because this is off grid range  ";
        }

        // now test the same for long bridges
        nodeToTest = GetStationInDirection(dir, 2);
        if (nodeToTest != null && canStepByOne)
        {
            StationBuilding potentialHub = nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub);
            if (potentialHub == null)
            {
                // okay to build any long bridge
                ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = true;
                ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = true;

            }
            else if (potentialHub.type == BuildingType.MainConnector)
            {
                // okay to build thick longbridge todog
                ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = true;
            }
            else if (potentialHub.type == BuildingType.SmallDisk)
            {
                // okay to build thin long bridge
                ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = true;
            }

        }
        else
        {
            // can't build any bridge because this is off grid range
            UpdateText.GetComponent<Text>().text = "can't build any long bridge because this is off grid range  ";
        }

    }

    // after bottom button has been selected update the station building buttons that are allowed
    public void OnBottomSelected()
    {
        bool buildable = stationGrid[currentStation.first, currentStation.second].GetBottomHeight() > maxHeightBtm;
        if (buildable)
        {
            // make all buildings buildable
            ((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>()).interactable = true;
            ((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>()).interactable = true;
            ((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>()).interactable = true;
            ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).interactable = true;

            buildOnBottom = true;
        }
    }

    public void OnTopSelected()
    {
        buildOnBottom = false;
        float topHeight = stationGrid[currentStation.first, currentStation.second].GetTopHeight();
        if (topHeight + buildingHeightTable[BuildingType.Disk] < maxHeightTop)
        {
            // disk is buildable
            ((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>()).interactable = true;
        }
        if (topHeight + buildingHeightTable[BuildingType.SmallDisk] < maxHeightTop)
        {
            // sm disk is buildable
            ((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>()).interactable = true;
        }
        if (topHeight + buildingHeightTable[BuildingType.Room] < maxHeightTop)
        {
            // room is buildable
            ((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>()).interactable = true;
        }
        if (topHeight + buildingHeightTable[BuildingType.MainConnector] < maxHeightTop)
        {
            // main connector is buildable
            ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).interactable = true;
        }
    }

    private StationNode GetStationInDirection(Direction dir, int amount)
    {
        int x = currentStation.first + DirectionEastWest(dir) * amount;
        int y = currentStation.second + DirectionNorthSouth(dir) * amount;
        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
            return null;
        return stationGrid[x, y];
    }

    private int DirectionEastWest(Direction dir)
    {
        if (dir == Direction.East)
            return 1;
        if (dir == Direction.West)
            return -1;
        return 0;
    }
    private int DirectionNorthSouth(Direction dir)
    {
        if (dir == Direction.North)
            return 1;
        if (dir == Direction.South)
            return -1;
        return 0;
    }


    // Update is called once per frame
    void Update () {
		
	}

    public static bool IsHub(BuildingType type)
    {
        if (type == BuildingType.MainConnector || type == BuildingType.SmallDisk)
            return true;
        return false;
    }

// GUI helpers
    private void EnableDirections()
    {
        ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>()).interactable = true;
        ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>()).interactable = true;
        ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>()).interactable = true;
        ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>()).interactable = true;
    }

    private void DisableBridges()
    {
        ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = false;
        ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = false;
    }

    private void DisableDirection(Direction dir)
    {
        if (dir == Direction.North)
        {
            ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>()).interactable = false;
        }
        if (dir == Direction.East)
        {
            ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>()).interactable = false;
        }
        if (dir == Direction.South)
        {
            ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>()).interactable = false;
        }
        if (dir == Direction.West)
        {
            ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>()).interactable = false;
        }
    }



}
