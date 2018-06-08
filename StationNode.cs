using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StationNode : MonoBehaviour {

    public enum Direction
    {
        North = 1,
        East = 2,
        South = 3,
        West = 4
    }

    class StationBuilding
    {
        public BuildingType type;
        public List<Direction> portsUsed;   // ports that are already used and cannot be further connected
        public Vector3 pos;

        public StationBuilding()
        {
            type = BuildingType.MainConnector;
            portsUsed = new List<Direction>();
        }

        public StationBuilding(BuildingType type, Vector3 pos)
        {
            this.type = type;
            this.pos = pos;
            portsUsed = new List<Direction>();
        }
    }

    LinkedList<StationBuilding> buildings;  // first is top, last is bottom
    LinkedList<StationBuilding> hubBuildings;
    int topLevel;   // current height of top most building
    private int maxTop = 2;
    StationBuilding selectedHub;

    // hubs used port is the occupied port for this newly initialized node's hub.
    public void Initialize(bool isBigHub, Direction hubsUsedPort, Vector3 pos)
    {
        StationBuilding mainBuilding;
        if (isBigHub)
        {
            mainBuilding = new StationBuilding(BuildingType.MainConnector, pos);
        }
        else
        {
            mainBuilding = new StationBuilding(BuildingType.SmallDisk, pos);
        }

        buildings.AddFirst(mainBuilding);
        hubBuildings.AddFirst(mainBuilding);

        mainBuilding.portsUsed.Add(hubsUsedPort);
        selectedHub = mainBuilding;
    }

    public bool AddBuilding(bool top, BuildingType btype)
    {
        float newBuildingHeight = SpaceStation.buildingHeightTable[btype];
        if (top)
        {
            Vector3 buildingPos = buildings.First.Value.pos;
            if (buildingPos.y + newBuildingHeight > SpaceStation.maxHeightTop)
                return false;
            buildingPos.y += newBuildingHeight;
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos);
            buildings.AddFirst(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddFirst(newBuilding);
        }
        else
        {
            Vector3 buildingPos = buildings.Last.Value.pos;
            if (buildingPos.y - newBuildingHeight < SpaceStation.maxHeightBtm)
                return false;
            buildingPos.y -= SpaceStation.buildingHeightTable[buildings.Last.Value.type];
            StationBuilding newBuilding = new StationBuilding(btype, buildingPos);
            buildings.AddLast(newBuilding);
            if (SpaceStation.IsHub(btype))
                hubBuildings.AddLast(newBuilding);
        }
        return true;

    }

    public List<Direction> GetPortsUsedOnSelectedHub()
    {
        return selectedHub.portsUsed;
    }

    // returns false if no hub is available going up
    public bool SelectNextHubUp()
    {
        if (hubBuildings.Find(selectedHub).Previous == null)
            return false;
        else
            selectedHub = hubBuildings.Find(selectedHub).Previous.Value;
        return true;
    }

    // returns false if no hub is available going down
    public bool SelectNextHubDown()
    {
        if (hubBuildings.Find(selectedHub).Next == null)
            return false;
        else
            selectedHub = hubBuildings.Find(selectedHub).Next.Value;
        return true;
    }

    public bool BuildBridgeOnSelectedHub(Direction dir)
    {
        if (selectedHub.portsUsed.Contains(dir))
            return false;
        
        f
        return true;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
