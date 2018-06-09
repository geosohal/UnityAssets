using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North = 1,
    East = 2,
    South = 3,
    West = 4
}


public class StationBuilding
{
    public BuildingType type;
    public List<Tuple<Direction, StationBuilding>> portsUsed;   // ports that are already used and cannot be further connected
    public Vector3 pos;
    public StationNode parentNode;

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

    public StationBuilding(BuildingType type, Vector3 pos, StationNode parent)
    {
        this.type = type;
        this.pos = pos;
        portsUsed = new List<Tuple<Direction, StationBuilding>>();
        parentNode = parent;
    }

    public bool IsPortUsed(Direction dir)
    {
        foreach (var port in portsUsed)
        {
            if (port.first == dir)
                return true;
        }
        return false;
    }
}


// a station node contains a vertical tower of buildings
public class StationNode : MonoBehaviour {



    LinkedList<StationBuilding> buildings;  // first is top, last is bottom
    LinkedList<StationBuilding> hubBuildings;
    //int topLevel;   // current height of top most building--- dont need anymore we now use SpaceStation.maxHeightTop to limit height
    private int maxTop = 2;
    public StationBuilding selectedHub;

    public StationNode()
    {
        buildings = new LinkedList<StationBuilding>();
        hubBuildings = new LinkedList<StationBuilding>();
        selectedHub = null;
    }

    // hubs used port is the occupied port for this newly initialized node's hub.
    public void Initialize(bool isBigHub, Tuple<Direction, StationBuilding> hubsUsedPort, Vector3 pos)
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
        }
        else
        {
            selectedHub = new StationBuilding(BuildingType.SmallDisk, pos, this);

        }

        buildings.AddFirst(selectedHub);
        hubBuildings.AddFirst(selectedHub);
    }

    public bool IsEmpty() { return buildings.Count > 0; }

    public float GetBottomHeight()
    {
        return buildings.Last.Value.pos.y;
    }

    public float GetTopHeight()
    {
        return buildings.First.Value.pos.y;
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

    public bool AddBuilding(bool top, BuildingType btype)
    {
        float newBuildingHeight = SpaceStation.buildingHeightTable[btype];
        if (top)
        {
            Vector3 buildingPos = buildings.First.Value.pos;
            if (CanAddBuilding(top,btype,buildingPos,newBuildingHeight))
                return false;
            buildingPos.y += newBuildingHeight;
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos, this);
            buildings.AddFirst(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddFirst(newBuilding);
        }
        else
        {
            Vector3 buildingPos = buildings.Last.Value.pos;
            if (CanAddBuilding(top, btype, buildingPos, newBuildingHeight))
                return false;
            buildingPos.y -= SpaceStation.buildingHeightTable[buildings.Last.Value.type];
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos, this);
            buildings.AddLast(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddLast(newBuilding);
        }
        return true;

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

    public List<Tuple<Direction,StationBuilding>> GetPortsUsedOnSelectedHub()
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
    public bool BuildBridgeOnSelectedHub(Direction dir, StationBuilding connectedNode)
    {
        if (selectedHub.IsPortUsed(dir))
            return false;

        selectedHub.portsUsed.Add( new Tuple<Direction, StationBuilding>(dir,connectedNode) );
        return true;
    }

    public Vector3 GetNextNodePosFromSelectedHub(Direction dir, HorizontalBuildingType hBuildingType)
    {
        float bridgeLen;
        if (hBuildingType == HorizontalBuildingType.LargeBridge || hBuildingType == HorizontalBuildingType.SmallBridge)
            bridgeLen = SpaceStation.bigBridgeLength;
        else if (hBuildingType == HorizontalBuildingType.SmallBridgeDouble || hBuildingType == HorizontalBuildingType.LargeBridgeDouble)
            bridgeLen = SpaceStation.bigBridgeLength * 2;
        else //if (hBuildingType == HorizontalBuildingType.Turret)
            return Vector3.zero;

        Vector3 ans;
        if (selectedHub.IsPortUsed(dir))
        {
            if (dir == Direction.North)
                ans = selectedHub.pos + bridgeLen * Vector3.forward;
            else if (dir == Direction.East)
                ans = selectedHub.pos + bridgeLen * Vector3.right;
            else if (dir == Direction.South)
                ans = selectedHub.pos + bridgeLen * Vector3.back;
            else if (dir == Direction.West)
                ans = selectedHub.pos + bridgeLen * Vector3.left;
            else
                ans = Vector3.zero;
            return ans;
        }
        return Vector3.zero;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
