using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Node class object script
[System.Serializable]
public class Node
{
    //Enumeration of node types to decide what material they are
    public enum NodeType { Default, Start, End, Visited, Current, Open, FinalPath}
    //Check if node has been visited
    public bool hasBeenVisited;

    //The upper wall of this node (1 in BreakWall switch statement)
    public GameObject northWall;
    //The right wall of this node (2 in BreakWall switch statement)
    public GameObject eastWall;
    //The bottom wall of this node (3 in BreakWall switch statement)
    public GameObject southWall;
    //The left wall of this node (4 in BreakWall switch statement)
    public GameObject westWall;

    //Floor gameObject of node, also the node's material
    public GameObject nodeFloor;

    //Global position of node
    public Vector3 nodePosition;       

    public int  gCost,      //Path cost from current node to start node - A*, Dijstra's
                hCost,      //Distance from current node to end node (heuristic) - A*
                xGrid,      //X coordinate position in grid
                yGrid,      //Y coordinate position in grid
                parentNode; //Previous node in shortest path
    public int fCost { get { return gCost + hCost; } }  //Minimised function - A*

    //Dictionary data structure used to determine node material depending on node type
    Dictionary<NodeType, Material> nodeTypeMaterial = new Dictionary<NodeType, Material>()
    {
        {0, Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)0}")},
        {(NodeType)1, Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)1}")},
        {(NodeType)2,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)2}")},
        {(NodeType)3,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)3}")},
        {(NodeType)4,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)4}")},
        {(NodeType)5,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)5}")},
        {(NodeType)6,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)6}")},
        {(NodeType)7,  Resources.Load<Material>($"Materials/NodeMaterials/{(NodeType)7}")}
    };

    //Constructor method for the walls, node object and node position
    public void SetWalls(GameObject north, GameObject east, GameObject south, GameObject west, GameObject nodeObject, Vector3 position)
    {
        northWall = north;
        eastWall = east;
        southWall = south;
        westWall = west;

        nodeFloor = nodeObject;
        nodePosition = position;
    }

    //Change the node type and material based off dictionary
    public void SetNodeTypeMaterial(NodeType nodeType)          //Set colour of node
    {
        nodeFloor.GetComponent<MeshRenderer>().material = nodeTypeMaterial[nodeType];
    }

    //Reset the node attributes to default values
    public void ResetNodeAttributes()
    {
        gCost = 0;
        hCost = 0;
        parentNode = 0;
    }
}