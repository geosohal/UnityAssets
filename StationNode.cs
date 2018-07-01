using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North = 1,
    East = 2,
    South = 3,
    West = 4
}

public struct PortBuilding
{
    public Direction dir;
    public HorizontalBuildingType btype;
    public GameObject mesh;
}

public class StationBuilding
{
    public BuildingType type;
    public List<PortBuilding> portsUsed;   // ports that are already used and cannot be further connected
    public Vector3 pos;
    public StationNode parentNode;
    public GameObject buildingMesh;
    public FunctionalBuilding functional;

    //public StationBuilding()
    //{
    //    type = BuildingType.MainConnector;
    //    portsUsed = new List<Tuple<Direction, StationBuilding>>();
    //}

    //public StationBuilding(BuildingType type, Vector3 pos)
    //{
    //    this.type = type;
    //    this.pos = pos;
    //    portsUsed = new List<Tuple<Direction, StationBuilding>>();
    //}

    public StationBuilding(BuildingType btype, Vector3 pos, StationNode parent)
    {
        this.type = btype;
        this.pos = pos;
        portsUsed = new List<PortBuilding>();
        parentNode = parent;
        buildingMesh = null;
        InitializeFunctional(btype);
    }

    public bool IsPortUsed(Direction dir)
    {
        foreach (var port in portsUsed)
        {
            if (port.dir == dir)
                return true;
        }
        return false;
    }

    public HorizontalBuildingType GetPortConnectorType(Direction dir)
    {
        foreach (var port in portsUsed)
        {
            if (port.dir == dir)
                return port.btype;
        }
        return HorizontalBuildingType.None;
    }

    public void InitializeFunctional(BuildingType btype)
    {
        if (btype == BuildingType.Disk)
            functional = new ShieldGenerator();
        if (btype == BuildingType.Disk2)
            functional = new GreenHouse();
        if (btype == BuildingType.SmallDisk)
            functional = new PowerStation();
        if (btype == BuildingType.Room)
            functional = new Residence();
    }
}

// a station node contains a vertical tower of buildings
public class StationNode : MonoBehaviour {



    LinkedList<StationBuilding> buildings;  // first is top, last is bottom
    LinkedList<StationBuilding> hubBuildings;
    //int topLevel;   // current height of top most building--- dont need anymore we now use SpaceStation.maxHeightTop to limit height
    private int maxTop = 2;
    public StationBuilding selectedHub;
    public bool IsBridgeCovered;    //  if this is true then this node isn't a building, its a special case
                                    // where a bridge from another node crosses over this node in the grid
                                    // needed to tell if a spot on the grid is occupied

    public StationNode()
    {
        buildings = new LinkedList<StationBuilding>();
        hubBuildings = new LinkedList<StationBuilding>();
        selectedHub = null;
        IsBridgeCovered = false;
    }

    // hubs used port is the occupied port for this newly initialized node's hub.
    public void Initialize(bool isBigHub, PortBuilding hubsUsedPort, Vector3 pos)
    {
        Initialize(isBigHub, pos);
        selectedHub.portsUsed.Add(hubsUsedPort);
    }

    // use this initialize for the first node of a space station which has no hubports used
    public void Initialize(bool isBigHub, Vector3 pos)
    {
        if (isBigHub)
        {
            selectedHub = new StationBuilding(BuildingType.MainConnector, pos, this);
            selectedHub.buildingMesh = (GameObject)Instantiate(Resources.Load("MainConnector"));
            selectedHub.buildingMesh.transform.position = pos;
        }
        else
        {
            selectedHub = new StationBuilding(BuildingType.SmallDisk, pos, this);

        }

        buildings.AddFirst(selectedHub);
        hubBuildings.AddFirst(selectedHub);
    }

    public bool IsEmpty() { return buildings.Count == 0; }

    public float? GetBottomHeight()
    {
        if (buildings.Last != null)
            return buildings.Last.Value.pos.y;
        return null;
    }

    public float? GetTopHeight()
    {
        if (buildings.First != null)
            return buildings.First.Value.pos.y;
        return null;
    }

    public bool CanAddBuilding(bool top, BuildingType btype, Vector3 buildingPos, float newBuildingHeight)
    {
        if (top)
        {
            if (buildingPos.y + newBuildingHeight > SpaceStation.maxHeightTop)
                return false;
        }
        else
        {
            if (buildingPos.y - newBuildingHeight < SpaceStation.maxHeightBtm)
                return false;
        }
        return true;
    }

    public FunctionalBuilding GetLastBuildingAdded(bool top)
    {
        if (buildings.Count > 0)
            return null;
        if (top)
            return buildings.First.Value.functional;
        return buildings.Last.Value.functional;
    }
    public bool AddBuilding(bool top, BuildingType btype)
    {
        float newBuildingHeight = SpaceStation.buildingHeightTable[btype];
        if (top)
        {
            Vector3 buildingPos = buildings.First.Value.pos;
            if (!CanAddBuilding(top,btype,buildingPos,newBuildingHeight))
                return false;
            buildingPos.y += newBuildingHeight+ SpaceStation.buildingHeightTable[buildings.First.Value.type] / 2;
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos, this);
            newBuilding.buildingMesh = MakeGameObjectInstance(btype);
            newBuilding.buildingMesh.transform.position = buildingPos;
            buildings.AddFirst(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddFirst(newBuilding);
        }
        else
        {
            Vector3 buildingPos = buildings.Last.Value.pos;
            if (!CanAddBuilding(top, btype, buildingPos, newBuildingHeight))
                return false;
            buildingPos.y -= newBuildingHeight + SpaceStation.buildingHeightTable[buildings.Last.Value.type ]/2;
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos, this);
            newBuilding.buildingMesh = MakeGameObjectInstance(btype);
            newBuilding.buildingMesh.transform.position = buildingPos;
            buildings.AddLast(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddLast(newBuilding);
        }
        return true;

    }

    private GameObject MakeGameObjectInstance(BuildingType btype)
    {
        if (btype == BuildingType.SmallDisk)
            return (GameObject)Instantiate(Resources.Load("SmallDisk"));
        else if (btype == BuildingType.MainConnector)
            return (GameObject)Instantiate(Resources.Load("MainConnector"));
        else if (btype == BuildingType.Room)
            return (GameObject)Instantiate(Resources.Load("Room"));
        else if (btype == BuildingType.Disk)
            return (GameObject)Instantiate(Resources.Load("Disk1"));
        Debug.Log("building not supported1");
        return null;
    }

    private GameObject MakeBridgeGOInstance(HorizontalBuildingType btype)
    {
        if (btype == HorizontalBuildingType.LargeBridge)
            return (GameObject)Instantiate(Resources.Load("hbThickBridge"));
        if (btype == HorizontalBuildingType.LargeBridgeDouble)
            return (GameObject)Instantiate(Resources.Load("hbThickBridgeDouble"));
        if (btype == HorizontalBuildingType.SmallBridge)
            return (GameObject)Instantiate(Resources.Load("hbThinBridge"));
        if (btype == HorizontalBuildingType.SmallBridgeDouble)
            return (GameObject)Instantiate(Resources.Load("hbThinBridgeDouble"));
        Debug.Log("building not supported");
        return null;
    }
  
    public StationBuilding GetBuildingTypeAtHeight(float height)
    {
        // remember positions of buildings are measured from their ceiling, so a building
        // is at a certain height if its less than pos.y but greater than pos.y-buildingheight
        foreach (var building in buildings)
        {
            if (height < building.pos.y && height > building.pos.y - SpaceStation.buildingHeightTable[building.type])
                return building;
        }
        return null;
    }

    public List<PortBuilding> GetPortsUsedOnSelectedHub()
    {
        return selectedHub.portsUsed;
    }

    public bool CanSelectUp()
    {
        if (hubBuildings.Find(selectedHub).Previous == null)
            return false;
        return true;
    }

    public bool CanSelectDown()
    {
        if (hubBuildings.Find(selectedHub).Next == null)
            return false;
        return true;
    }
    // returns false if no hub is available going up
    public bool SelectNextHubUp()
    {
        if (CanSelectUp())
            return false;
        else
            selectedHub = hubBuildings.Find(selectedHub).Previous.Value;
        return true;
    }


    // returns false if no hub is available going down
    public bool SelectNextHubDown()
    {
        if (CanSelectDown())
            return false;
        else
            selectedHub = hubBuildings.Find(selectedHub).Next.Value;
        return true;
    }

    // user of function ensures that correct bridge type is used for the building
    public bool BuildBridgeOnSelectedHub(Direction dir, HorizontalBuildingType connectedNodeType)
    {
        if (selectedHub.IsPortUsed(dir))
            return false;

        PortBuilding newBridge = new PortBuilding();
        newBridge.dir = dir;
        newBridge.btype = connectedNodeType;
        newBridge.mesh = MakeBridgeGOInstance(connectedNodeType);
        newBridge.mesh.transform.position = GetBridgePosFromPort(dir, connectedNodeType);
        if (dir == Direction.North || dir == Direction.South)
            newBridge.mesh.transform.rotation = Quaternion.Euler(180, 90, 90);
        else
            newBridge.mesh.transform.rotation = Quaternion.Euler(180, 0, 90);
        selectedHub.portsUsed.Add( newBridge );
        return true;
    }

    // if we build a hBuildingType building from the currently selected hub in the direction dir
    // return the position we end up at
    public Vector3 GetNextNodePosFromSelectedHub(Direction dir, HorizontalBuildingType hBuildingType)
    {
        float bridgeLen;
        if (hBuildingType == HorizontalBuildingType.LargeBridge || hBuildingType == HorizontalBuildingType.SmallBridge)
            bridgeLen = SpaceStation.bigBridgeLength;
        else if (hBuildingType == HorizontalBuildingType.SmallBridgeDouble || hBuildingType == HorizontalBuildingType.LargeBridgeDouble)
            bridgeLen = SpaceStation.bigBridgeLength * 2;
        else //if (hBuildingType == HorizontalBuildingType.Turret)
            return Vector3.zero;

        if (selectedHub.IsPortUsed(dir))
        {
            return selectedHub.pos + SpaceStation.GetDisplacementVector(dir, bridgeLen);
        }
        return Vector3.zero;
    }

    public Vector3 GetBridgePosFromPort(Direction dir, HorizontalBuildingType btype)
    {
        float bridgeLen;
        if (btype == HorizontalBuildingType.LargeBridge || btype == HorizontalBuildingType.SmallBridge)
            bridgeLen = SpaceStation.smBridgeLength;
        else if (btype == HorizontalBuildingType.SmallBridgeDouble || btype == HorizontalBuildingType.LargeBridgeDouble)
            bridgeLen = SpaceStation.bigBridgeLength;
        else //if (hBuildingType == HorizontalBuildingType.Turret)
            return Vector3.zero;

        Vector3 ans = Vector3.zero;
        ans = selectedHub.pos + SpaceStation.GetDisplacementVector(dir, bridgeLen/2);
        ans.y -= SpaceStation.buildingHeightTable[BuildingType.MainConnector] / 2;
        return ans;
    }

    public PortBuilding? GetPortBuilding(Direction dir)
    {
        foreach (PortBuilding building in selectedHub.portsUsed)
        {
            if (building.dir == dir)
                return building;
        }
        return null;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
