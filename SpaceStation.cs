using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    MainConnector, // hub that connects large bridge and other buildings up or down
    Disk,   // shield generator
    Disk2,  // greenhouse
    SmallDisk,      // power generator also a hub but only connects small bridges
    Room,      // residence
}

public enum HorizontalBuildingType
{
    LargeBridge,
    LargeBridgeDouble,
    SmallBridge,
    SmallBridgeDouble,
    Turret, // can only be built of main connectors at the base height
           //one design option is to allow them on main connectors of other heights but not now
    None
}

public class StationManager
{
    private List<PowerStation> powerStations;
    private List<GreenHouse> greenHouses;
    private List<ShieldGenerator> shieldGens;
    private List<Residence> residences;
    private List<Person> homeless;
    private bool housesFull;



    public StationManager()
    {
        powerStations = new List<PowerStation>();
        greenHouses = new List<GreenHouse>();
        shieldGens = new List<ShieldGenerator>();
        residences = new List<Residence>();
    }

    // note this doesnt store hub types
    public void StoreFunctionalBuildingRef(BuildingType btype, FunctionalBuilding building)
    {
        if (btype == BuildingType.Disk)
            shieldGens.Add((ShieldGenerator)building);
        if (btype == BuildingType.Disk2)
            greenHouses.Add((GreenHouse)building);
        if (btype == BuildingType.SmallDisk)
            powerStations.Add((PowerStation)building);
        if (btype == BuildingType.Room)
            residences.Add((Residence)building);
    }

    public bool AddResidents(int amount)
    {
        int numAdded = 0;
        for (; numAdded < amount; numAdded++)
        {
            Person p = new Person();
            if (!AddPersonToHouse(p))
            {
                housesFull = true;
                break;
            }
        }
        while (numAdded < amount)
        {
            Person p = new Person();
            homeless.Add(p);
        }
        if (housesFull)
            return false;
        return true;
    }

    public bool AddPersonToHouse(Person p)
    {
        foreach (Residence house in residences)
        {
            if (!house.IsFull())
            {
                house.AddPerson(p);
                return true;
            }
        }
        return false;
    }
}

public class SpaceStation : MonoBehaviour {

    // set by unity:
    public GameObject UpdateText;

    // STATIC GLOBALS:
    // heights of buildings are from top to bottom, the hub connectors are halfway between top and bottom
    // small disk should be slightly less than half but will work for now.
    public static Dictionary<BuildingType, float> buildingHeightTable;
    public static float bigBridgeLength = 165f;
    public static float smBridgeLength = 100f;
  //  public static float smallBridgeLength = 122f;//deperecated - needs to be same as other bridge
    public static float maxHeightTop = 30f;
    public static float maxHeightBtm = -200f;

    // OTHER MEMBERS:
    public StationNode selectedNode;
    private StationNode prevSelectedNode;
    private Direction lastDirChange;
    private int gridSize = 20;  // should keep this an even number
    public StationNode[,] stationGrid;
    private Tuple<int, int> currentStation;
    public bool IsEmpty;
    private bool buildOnBottom; // if false then builder is set to build on top
    private Direction selectedDir;
    StationManager stationMan;


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
            SpaceStation.buildingHeightTable.Add(BuildingType.Room, 36f);
            SpaceStation.buildingHeightTable.Add(BuildingType.MainConnector, 25f);
            SpaceStation.buildingHeightTable.Add(BuildingType.Disk, 18f);
            SpaceStation.buildingHeightTable.Add(BuildingType.Disk2, 25f);
        }
        stationMan = new StationManager();
        stationGrid = new StationNode[gridSize,gridSize];
        currentStation = new Tuple<int, int>(gridSize / 2, gridSize / 2); // make middle node selected
        stationGrid[currentStation.first, currentStation.second] = new StationNode();
        SetSelectedNode();
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



        ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).onClick.AddListener(() => this.OnHubButton());
        ((Button)GameObject.FindGameObjectWithTag("btnBigDisk").GetComponent<Button>()).onClick.AddListener(() => this.OnTowerBuildingButton(BuildingType.Disk));
        ((Button)GameObject.FindGameObjectWithTag("btnSmDisk").GetComponent<Button>()).onClick.AddListener(() => this.OnTowerBuildingButton(BuildingType.SmallDisk));
        ((Button)GameObject.FindGameObjectWithTag("btnRoom").GetComponent<Button>()).onClick.AddListener(() => this.OnTowerBuildingButton(BuildingType.Room));

        ((Button)GameObject.FindGameObjectWithTag("btnTop").GetComponent<Button>()).onClick.AddListener(() => this.OnTopSelected());
        ((Button)GameObject.FindGameObjectWithTag("btnBottom").GetComponent<Button>()).onClick.AddListener(() => this.OnBottomSelected());
        ((Button)GameObject.FindGameObjectWithTag("btnNorth").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.North));
        ((Button)GameObject.FindGameObjectWithTag("btnEast").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.East));
        ((Button)GameObject.FindGameObjectWithTag("btnSouth").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.South));
        ((Button)GameObject.FindGameObjectWithTag("btnWest").GetComponent<Button>()).onClick.AddListener(() => this.OnDirectionSelect(Direction.West));

        ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).onClick.AddListener(() => this.OnBridgeBuildButton(HorizontalBuildingType.LargeBridgeDouble));
        ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).onClick.AddListener(() => this.OnBridgeBuildButton(HorizontalBuildingType.SmallBridgeDouble));
        ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).onClick.AddListener(() => this.OnBridgeBuildButton(HorizontalBuildingType.SmallBridge));
        ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).onClick.AddListener(() => this.OnBridgeBuildButton(HorizontalBuildingType.LargeBridge));

        ((Button)GameObject.FindGameObjectWithTag("btnUp").GetComponent<Button>()).onClick.AddListener(() => this.OnUpPress());
        ((Button)GameObject.FindGameObjectWithTag("btnDown").GetComponent<Button>()).onClick.AddListener(() => this.OnDownPress());
    }

    public void CreateFirstHub(Vector3 pos)
    {
        //   StationNode newNode = new StationNode();
        // newNode.Initialize(true, pos);
        //stationGrid[currentStation.first, currentStation.second] = newNode;
        //SetSelectedNode();
        selectedNode.Initialize(true, pos);
        
        OnHubSelect();
    }

    public void CreateHubOnCurrentNode()
    {
        if (selectedNode.IsEmpty())
        {
        //    GetCurrentNode().Initialize(true, GetCurrentNode().GetNextNodePosFromSelectedHub);

        }
    }
    public void OnHubButton()
    {
        if (IsEmpty)    // create first hub for space station
        {

            Vector3 playerPos = GetPlayerPos();
            if (playerPos == Vector3.zero)  // player doesnt exist
                return;
            CreateFirstHub(playerPos);
            IsEmpty = false;
            OnBottomSelected();

        }
        else
        {

            //  if they are also a hub with the adjacent port used - in both cases it means the new hubs port will need to be set to used upon creation

            //note that we could allow more hubs to be created on the node if the tower is empty at this height, this might be cooler
            if (selectedNode.selectedHub == null)   // no hub created on this node, 
            {

                PortBuilding? connectingBridge = prevSelectedNode.GetPortBuilding(lastDirChange);
                if (connectingBridge != null)   // prev node has a bridge going to this node
                {
                    Vector3 prevNodePos = prevSelectedNode.selectedHub.pos;
                    Vector3 bridgeDisplacement = GetDisplacementVector(lastDirChange, GetBridgeLength(prevSelectedNode.selectedHub.GetPortConnectorType(lastDirChange)));
                    Vector3 newHubPos = prevNodePos + bridgeDisplacement;

                    PortBuilding connectingBridgeReg = (PortBuilding)connectingBridge;
                    connectingBridgeReg.dir = ReverseDirection(connectingBridgeReg.dir);
                    selectedNode.Initialize(true, connectingBridgeReg,newHubPos);

                    OnHubSelect();
                    OnBottomSelected();
                }

            }
        }
    }

    

    private Vector3 GetPlayerPos()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (((ItemBehaviour)player.GetComponent<ItemBehaviour>()).item.IsMine)
            {
                return player.transform.position;
            }
        }
        return Vector3.zero;
    }


    public void OnTowerBuildingButton(BuildingType bType)
    {
        selectedNode.AddBuilding(!buildOnBottom, bType);
        stationMan.StoreFunctionalBuildingRef(bType, selectedNode.GetLastBuildingAdded(!buildOnBottom));
    }

    public void OnBridgeBuildButton(HorizontalBuildingType bridgeType)
    {
        if (bridgeType == HorizontalBuildingType.LargeBridgeDouble || bridgeType == HorizontalBuildingType.SmallBridgeDouble)
        {
            if (GetNodeInSelectedDir() == null)
                stationGrid[currentStation.first + DirectionEastWest(selectedDir), currentStation.second + DirectionNorthSouth(selectedDir)] = new StationNode();
            GetNodeInSelectedDir().IsBridgeCovered = true;
            selectedNode.BuildBridgeOnSelectedHub(selectedDir, bridgeType);
            bool isValidNode = true; ;
            StationNode connectingNode = GetNodeInDirection(selectedDir, 2, ref isValidNode );
            if (isValidNode && connectingNode == null) // node was in bounds and has not been constructed
            {
                connectingNode = new StationNode();
            }
        }
        if (bridgeType == HorizontalBuildingType.LargeBridge || bridgeType == HorizontalBuildingType.SmallBridge)
        {
            if (GetNodeInSelectedDir() == null)
                stationGrid[currentStation.first + DirectionEastWest(selectedDir), currentStation.second + DirectionNorthSouth(selectedDir)] = new StationNode();
            selectedNode.BuildBridgeOnSelectedHub(selectedDir, bridgeType);
        }

        
        // note that any hubs we add, besides the first, will ned to check adjacent grid squares to see if they are covered by a bridge
        // or if they are also a hub with the adjacent port used - in both cases it means the new hubs port will need to be set to used upon creation
    }

    StationNode GetNodeInSelectedDir()
    {
        return stationGrid[currentStation.first + DirectionEastWest(selectedDir), currentStation.second + DirectionNorthSouth(selectedDir)];
    }

    // return currently selected node
    private void SetSelectedNode()
    {
        selectedNode = stationGrid[currentStation.first, currentStation.second];
    }

    StationNode GetNodeInDirection(Direction dir, int stepAmount, ref bool validNode)
    {
        int x = currentStation.first + DirectionEastWest(dir) * stepAmount;
        int y = currentStation.second + DirectionNorthSouth(dir) * stepAmount;
        if (IsIndexInGridBounds(x) && IsIndexInGridBounds(y))
        {
            validNode = true;
            return stationGrid[x, y];
        }
        validNode = false;
        return null;
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
            DisableDirection(port.dir);
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

    // switch currently selected node to the neighbor in the given direction. if the direction isnt out 
    // of grid bounds and if port is used with a bridge coming out, then we move out in the direction
    // by one or two node (depending on its a double bridge or not) and select that node
    public void SelectNeighborNode(Direction dir)
    {
        if (selectedNode.selectedHub.IsPortUsed(dir))
        {
            HorizontalBuildingType bridgeType = selectedNode.selectedHub.GetPortConnectorType(dir);
            int bridgeLen;
            if (bridgeType == HorizontalBuildingType.LargeBridge || bridgeType == HorizontalBuildingType.SmallBridge)
                bridgeLen = 1;
            else if (bridgeType == HorizontalBuildingType.LargeBridgeDouble || bridgeType == HorizontalBuildingType.SmallBridgeDouble)
                bridgeLen = 2;
            else
                return; // port does not connect to bridge type, must be turret or something else

            int x = currentStation.first + DirectionEastWest(dir)* bridgeLen;
            int y = currentStation.second + DirectionNorthSouth(dir)* bridgeLen;
            if (!IsIndexInGridBounds(x) || !IsIndexInGridBounds(y))
                return; // no node exists in this direction because it's out of bounds
            currentStation.first = x;
            currentStation.second = y;
            prevSelectedNode = selectedNode;
            lastDirChange = dir;
            SetSelectedNode();  
        }
    }

    public float GetCurrentNodesHubHeight()
    {
        return selectedNode.selectedHub.pos.y - .5f;
    }

    // after a port has been selected, update the bridge buttons to show what bridge types are allowed
    // note this does not change what station node is selected
    public void OnDirectionSelect(Direction dir)
    {
        // one case where a bridge type wouldnt be allowed is where you try to do a long bridge over a node
        // another case is where you do a bridge and instead of connecting to a node on the other side it 
        // connects to some other building type
        // so we just need to check for these cases:
        bool outOfBounds = false;
        StationNode nodeToTest = GetStationInDirection(dir, 1, ref outOfBounds); // gets the next node over in the given direction
        float heightOfSelectedHub = GetCurrentNodesHubHeight();  // offset height so its at about the center of the hub
        
        bool canStepByOne = false;  // indicates whether we can build
        
        // test for non-long bridges
        if (!outOfBounds)
        {
            // if no building at node for this height because the node is empty or node has no building at height
            if (nodeToTest == null || nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub) == null)   
            {
                canStepByOne = true;
                // okay to build any single size (non-long) bridges
                ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = true;
                ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
            else if (nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub).type == BuildingType.MainConnector)
            {
                canStepByOne = true;
                // okay to build thick bridge todog
                ((Button)GameObject.FindGameObjectWithTag("btnBigBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
            else if (nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub).type == BuildingType.SmallDisk)
            {
                canStepByOne = true;
                // okay to build thin bridge
                ((Button)GameObject.FindGameObjectWithTag("btnThinBridge").GetComponent<Button>()).interactable = true;
                selectedDir = dir;
            }
        }
        else
        {
            // can't build any bridge because this is off grid range
            UpdateText.GetComponent<Text>().text = "can't build any bridge because this is off grid range  ";
        }

        outOfBounds = false;
        // now test the same for long bridges
        nodeToTest = GetStationInDirection(dir, 2, ref outOfBounds);
        if (!outOfBounds && canStepByOne)
        {

            if (nodeToTest == null || nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub) == null)
            {
                // okay to build any long bridge
                ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = true;
                ((Button)GameObject.FindGameObjectWithTag("btnThinLongBridge").GetComponent<Button>()).interactable = true;

            }
            else if (nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub).type == BuildingType.MainConnector)
            {
                // okay to build thick longbridge todog
                ((Button)GameObject.FindGameObjectWithTag("btnBigLongBridge").GetComponent<Button>()).interactable = true;
            }
            else if (nodeToTest.GetBuildingTypeAtHeight(heightOfSelectedHub).type == BuildingType.SmallDisk)
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
        float? btmHeight = stationGrid[currentStation.first, currentStation.second].GetBottomHeight();
        if (btmHeight == null)  // no buildings on node, so we can only make first building
        {
            ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).interactable = true;
            buildOnBottom = true;
            return;
        }
        bool buildable = btmHeight > maxHeightBtm;
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
        float? topHeight = stationGrid[currentStation.first, currentStation.second].GetTopHeight();
        if (topHeight == null)  // no buildings on node, so we can only make first building
        {
            ((Button)GameObject.FindGameObjectWithTag("btnHub").GetComponent<Button>()).interactable = true;
            return;
        }
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

    private StationNode GetStationInDirection(Direction dir, int amount, ref bool outOfBounds)
    {
        int x = currentStation.first + DirectionEastWest(dir) * amount;
        int y = currentStation.second + DirectionNorthSouth(dir) * amount;
        if (!IsIndexInGridBounds(x) || !IsIndexInGridBounds(y))
        {
            outOfBounds = true;
            return null;
        }
        return stationGrid[x, y];
    }

    // assumes square grid, returns whether index into array is within the bounds of the stationnode array
    private bool IsIndexInGridBounds(int x)
    {
        if (x >= 0 && x < gridSize)
            return true;
        return false;
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
		if (Input.GetKeyDown(KeyCode.UpArrow))
            SelectNeighborNode(Direction.North);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            SelectNeighborNode(Direction.South);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelectNeighborNode(Direction.West);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelectNeighborNode(Direction.East);
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

    public static float GetBridgeLength(HorizontalBuildingType bridge)
    {
        if (bridge == HorizontalBuildingType.LargeBridge || bridge == HorizontalBuildingType.SmallBridge)
            return smBridgeLength;
        if (bridge == HorizontalBuildingType.LargeBridgeDouble || bridge == HorizontalBuildingType.SmallBridgeDouble)
            return bigBridgeLength;
        Debug.Log("bridge length on non bridge type");
        return 0;//turret
    }
    // may want to refactor these two functions into a Direction class 
    public static Direction ReverseDirection(Direction dir)
    {
        if (dir == Direction.East)
            return Direction.West;
        if (dir == Direction.West)
            return Direction.East;
        if (dir == Direction.South)
            return Direction.North;
        //if (dir == Direction.North)
        return Direction.South;
    }


    public static Vector3 GetDisplacementVector(Direction dir, float length)
    {

        if (dir == Direction.North)
            return length * Vector3.forward;
        else if (dir == Direction.East)
            return length* Vector3.right;
        else if (dir == Direction.South)
            return length * Vector3.back;
      //  else if (dir == Direction.West)
            return length * Vector3.left;
    }
}
