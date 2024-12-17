using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

//Maze generator script - main script for maze generation and each algorithm
public class MazeGenerator : MonoBehaviour
{
    //Enumeration for algorithm type
    public enum Algorithm { Dijkstras, AStar, Greedy, RightHand}
    //Enumeration for maze difficulty
    public enum MazeDifficulty { Perfect, Hard, Medium, Easy}

    //Current maze difficuly
    public MazeDifficulty currentDifficulty;

    //UI handler script
    UIHandler uiHandler;
    //Array of all nodes
    Node[] nodes;
    //Array of all walls
    GameObject[] allWalls;
    //Wall and node holders are empty GameObjects that are parents to all the walls and nodes
    //Prefabs are models for every wall and floor
    GameObject wallHolder, nodeHolder, wallPrefab, floorPrefab;
    //Stopwatch to measure how long each algorithm takes to run
    System.Diagnostics.Stopwatch stopwatch;
    //isRunning - if the algorithm is running, isResetting - if the maze is being reset
    bool isRunning = false, isResetting = false;
    //Length of wall, height of wall, time delay in seconds between each iteration
    float wallLength, wallHeight, loopDelay;
    //Maze width, maze height, start node array position, end node array position
    int xSize, ySize, startNode, endNode;
    //Total node count
    int totalNodes { get { return xSize * ySize; } }
    //Test data to be sent to text file
    string testData;
    //Time delay in time format between each iteration
    WaitForSeconds loopDelayWait;

    //Method is called as soon as the scene is loaded
    public void Start()         
    {
        //Finding the prefabs in the resources folder, the UI handler, and setting the wall dimensions
        wallPrefab = Resources.Load<GameObject>("Prefabs/Wall");
        wallLength = wallPrefab.GetComponent<Transform>().localScale.z;
        wallHeight = wallPrefab.GetComponent<Transform>().localScale.y;
        floorPrefab = Resources.Load<GameObject>("Prefabs/Floor");
        uiHandler = GameObject.Find("Canvas").GetComponent<UIHandler>();
    }

    //Public methods - called by other scripts

    //Method to generate a new maze based off parameters with a random start end, calling each private method in order
    public void GenerateRandomMaze(int width, int height, MazeDifficulty difficulty)   
    {
        foreach (Transform child in transform)  //Destroys previously generated maze
        {
            Destroy(child.gameObject);
        }
        xSize = width;      //Set new width of maze
        ySize = height;     //Set new height of maze
        currentDifficulty = difficulty;     //Set new difficulty of maze
        //Calls each method in order
        CreateWalls();  
        CreateNodes();  
        DesignMaze();   
        GenerateStartEnd(); 
    }

    //Method to run the chosen algorithm
    public void RunAlgorithm(float rate, Algorithm algorithm) 
    {
        isRunning = true;   //Algorithm is now running
        isResetting = false;    //Algorithm is not being reset
        loopDelay = 1f / rate;      //Set new delay time between each loop of maze using inverse rate
        loopDelayWait = new WaitForSeconds(loopDelay);  //Convert from float to time format
        if (algorithm == Algorithm.RightHand)   //Checks which coroutine to run based off chosen algorithm
        {
            StartCoroutine(FindRightHandPath());
        }
        else
        {
            StartCoroutine(FindMazeSolvingPath(algorithm));
        }
    }
    //Method to reset maze
    public void ResetMaze()                 
    {
        //Each node is set to default node type and its attributes are reset
        for (int i = 0; i < nodes.Length; i++) 
        {
            nodes[i].ResetNodeAttributes();
            if (!( i == startNode || i == endNode))
            {
                nodes[i].SetNodeTypeMaterial(Node.NodeType.Default);
            }
        }
        //All display counters are reset
        stopwatch.Reset();
        uiHandler.SetStopWatch(stopwatch.Elapsed);
        uiHandler.SetLoopCount(0);
        uiHandler.SetFinalDistance(0);
        isResetting = true;
    }
    //Method is run by NodeController script when player chooses a new start node
    public void SetStartNode(int nodeIndex)     
    {
        if (nodeIndex != endNode)   //Start node can not also be end node
        {
            nodes[startNode].SetNodeTypeMaterial(Node.NodeType.Default);    //Old start node is set to default
            startNode = nodeIndex;
            nodes[startNode].SetNodeTypeMaterial(Node.NodeType.Start);  //New chosen start node set
        }
    }
    //Method is run by NodeController script when player chooses a new end node
    public void SetEndNode(int nodeIndex)       
    {
        if (nodeIndex != startNode) //End node can not also be start node
        {
            nodes[endNode].SetNodeTypeMaterial(Node.NodeType.Default);  //Old end node is set to default
            endNode = nodeIndex;
            nodes[endNode].SetNodeTypeMaterial(Node.NodeType.End);  //New chosen end node set
        }
    }
    //Method to pause algorithm
    public void PauseAlgorithm()
    {
        isRunning = false;
    }
    //Method to continue running algorithm
    public void PlayAlgorithm()
    {
        isRunning = true;
    }
    //Method to save the algorithm test into the text file
    public void SaveTest()
    {
        StreamWriter testsFileWriter = new StreamWriter(Application.dataPath + TestFileHandler.TEST_FILE_PATH, true);
        testsFileWriter.WriteLine(testData);
        testsFileWriter.Close();
    }

    //Private methods - called within the script

    //Creates the walls of the maze initially in a grid
    void CreateWalls()                      
    {
        //Wall holder empty object created
        wallHolder = new GameObject
        {
            name = "MazeWalls"
        };
        //It is the parent to every wall
        wallHolder.transform.parent = this.transform;

        //Initial position of first wall is chosen as the bottom left side wall
        Vector3 initialPos = new Vector3(-xSize * wallLength / 2 + wallLength / 2, wallPrefab.GetComponent<Transform>().localPosition.y, -ySize * wallLength / 2 + wallLength / 2);
        //Temporary position of wall variable declared
        Vector3 currentPos;
        //Temporary wall variable declared
        GameObject tempWall;

        //Instantiating the side walls first
        for (int i = 0; i < ySize; i++)                                    
        {
            for (int j = 0; j <= xSize; j++)
            {
                //Each wall position is calculated based off the width of the wall prefab
                currentPos = new Vector3(initialPos.x + j * wallLength - wallLength / 2, wallPrefab.GetComponent<Transform>().localPosition.y, initialPos.z + i * wallLength);
                //Wall is instantiated
                tempWall = Instantiate(wallPrefab, currentPos, Quaternion.identity);
                tempWall.transform.parent = wallHolder.transform;
                //If the side wall is on the edge, it is tagged with "OuterWall"
                if (j == 0 || j == xSize)
                {
                    tempWall.tag = "OuterWall";
                }
            }
        }

        //Instantiating the north south walls second
        for (int i = 0; i <= ySize; i++)                                    
        {
            for (int j = 0; j < xSize; j++)
            {
                //Each wall position is calculated based off the width of the wall prefab
                currentPos = new Vector3(initialPos.x + j * wallLength, wallPrefab.GetComponent<Transform>().localPosition.y, initialPos.z + i * wallLength - wallLength / 2);
                //Wall is instantiated
                tempWall = Instantiate(wallPrefab, currentPos, Quaternion.Euler(0.0f, 90.0f, 0.0f));
                tempWall.transform.parent = wallHolder.transform;
                //If the north south wall is on the edge, it is tagged with "OuterWall"
                if (i == 0 || i == ySize)
                {
                    tempWall.tag = "OuterWall";
                }
            }
        }
    }
    //Assigns walls and positions to nodes, puts them into an array, and creates the floor of the maze
    void CreateNodes()                  
    {
        //Node array declared
        nodes = new Node[totalNodes];

        //Node holder empty object created
        nodeHolder = new GameObject
        {
            name = "MazeNodes"
        };
        nodeHolder.transform.parent = this.transform;

        //Getting all the walls into an array
        int wallCount = wallHolder.transform.childCount;                        
        allWalls = new GameObject[wallCount];
        for (int i = 0; i < wallCount; i++)
        {
            allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
        }

        //Counter for the west wall of each node (starts from 0 as the side walls are in the first part of the array of walls)
        int westCount = 0;
        //Counter for the south wall of each node (starts from (xSize + 1) * ySize as the north south walls are in the second part of the array of walls) 
        int southCount = (xSize + 1) * ySize;
        //Counter to check for when we have reached the end of the row
        int rowCount = 0;
        //Putting every node into the array from the bottom left going across each row, and assigning each node with its walls and floor
        for (int nodeCount = 0; nodeCount < nodes.Length; nodeCount++)
        {
            //If the row counter has reached the width of the maze, increment the side wall counter and set the row counter back to 0
            if (rowCount == xSize)
            {
                westCount++;
                rowCount = 0;
            }

            //Node added to array
            nodes[nodeCount] = new Node();
            //Node global position assigned
            Vector3 nodePosition = new Vector3(allWalls[westCount].transform.position.x + wallLength / 2, allWalls[westCount].transform.position.y - wallHeight / 2, allWalls[westCount].transform.position.z);

            //Node floor instantiated and contained in the node holder
            GameObject nodeObject = Instantiate(floorPrefab, nodePosition, Quaternion.identity) as GameObject;
            nodeObject.transform.parent = nodeHolder.transform;
            nodeObject.name = $"Floor {nodeCount}";

            //Node walls set based off the west wall counter and south wall counter (east wall and north wall is each one incremented by 1)
            //Node is constructed
            nodes[nodeCount].SetWalls(allWalls[southCount + xSize], allWalls[westCount + 1], allWalls[southCount], allWalls[westCount], nodeObject, nodePosition);

            //Increment each counter
            westCount++;
            southCount++;
            rowCount++;
        }

        //Setting each node in the node array's x position and y position separately
        int nodeC = 0;
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                nodes[nodeC].xGrid = x;
                nodes[nodeC].yGrid = y;
                nodeC++;
            }
        }

    }
    //Destroys the walls of the grids using the Depth first search algorithm, in order to create a perfect maze
    void DesignMaze()           
    {
        int stackCount = 1;   //Keeping track of the number of nodes from the current node to the start node
        int currentNode = UnityEngine.Random.Range(0, totalNodes);  //Start node can be any random node
        List<int> stackPath = new List<int> //Creating a list of all the nodes in the path from the start node to the current node using a stack
        {
            currentNode //Adding start node to list
        };
        nodes[currentNode].hasBeenVisited = true; //Setting start node as visited

        do
        {
            var neighbour = GetNeighbour(currentNode);  //Get a random unvisited neighour and the wall to break to reach it
            int currentNeighbour = neighbour.currentNeighbour;  //Get the random unvisited neighbour (-1 if there is none)
            if (currentNeighbour == -1)  //If there are no unvisited neighbours, backtrack from the current node back through the path by popping from the stack
            {
                stackCount--;
                stackPath.RemoveAt(stackCount);
                currentNode = stackPath[stackCount - 1];
            }
            else                                                //Otherwise if there is an unvisited neighbour...
            {
                BreakWall(currentNode, neighbour.walltoBreak);  //...Break the wall between the current node and the neighbour
                currentNode = currentNeighbour;                 //...Make the neighbour the current node
                stackPath.Add(currentNode);                     //...Add it to the stack and increment stack counter
                stackCount++;                                   
                nodes[currentNode].hasBeenVisited = true;              //...Mark it as visited
            }
        }
        while (stackCount > 1);                                 //Loop repeats until no more nodes in stack (returned to the original node)

        if (currentDifficulty != MazeDifficulty.Perfect)        //If maze difficulty is perfect, no extra walls will have to be destroyed
        {
            int percentageToDestroy = 0;                        //The chance of each wall being destroyed in the maze increases the easier the maze is
            switch (currentDifficulty)
            {
                case MazeDifficulty.Hard:                       
                    percentageToDestroy = 5;                    //5% chance of each wall being destroyed in hard difficulty
                    break;
                case MazeDifficulty.Medium:
                    percentageToDestroy = 15;                   //15% chance of each wall being destroyed in medium difficulty
                    break;
                case MazeDifficulty.Easy:
                    percentageToDestroy = 25;                   //25% chance of each wall being destroyed in easy difficulty
                    break;
            }
            System.Random rnd = new System.Random();            //Random generator
            foreach (GameObject wall in allWalls)
            {
                if (!wall.CompareTag("OuterWall"))
                {

                    int roll = rnd.Next(1, 100);                //A random number is chosen between 1 and 100 for each wall, if this number is less than the percentage value, the wall is destroyed
                    if (roll <= percentageToDestroy)
                    {
                        wall.SetActive(false);
                    }
                }
            }
        }


    }
    //Method that returns a random valid neighbour, and the wall to break in order to reach the neighbour - called by DesignMaze
    (int currentNeighbour, int walltoBreak) GetNeighbour(int currentNode)   
    {
        int neighbourCount = 0;
        int[] neighbours = new int[4];
        int[] connectingWall = new int[4];

        //Finding the north neighbour wall if there is one
        if (currentNode < totalNodes - xSize)
        {
            if (nodes[currentNode + xSize].hasBeenVisited == false)
            {
                neighbours[neighbourCount] = currentNode + xSize;
                connectingWall[neighbourCount] = 1;
                neighbourCount++;
            }
        }

        //Finding the east neighbour wall if there is one
        if ((currentNode + 1) % xSize != 0)
        {
            if (nodes[currentNode + 1].hasBeenVisited == false)
            {
                neighbours[neighbourCount] = currentNode + 1;
                connectingWall[neighbourCount] = 2;
                neighbourCount++;
            }
        }

        //Finding the south neighbour wall if there is one
        if (currentNode >= xSize)
        {
            if (nodes[currentNode - xSize].hasBeenVisited == false)
            {
                neighbours[neighbourCount] = currentNode - xSize;
                connectingWall[neighbourCount] = 3;
                neighbourCount++;
            }
        }

        //Finding the west neighbour wall if there is one
        if (!(currentNode % xSize == 0 || currentNode == 0))
        {
            if (nodes[currentNode - 1].hasBeenVisited == false)
            {
                neighbours[neighbourCount] = currentNode - 1;
                connectingWall[neighbourCount] = 4;
                neighbourCount++;
            }
        }


        //Choosing a random valid neighbour, else returning -1 if there is none
        if (neighbourCount != 0)
        {
            int chosenNeighbour = UnityEngine.Random.Range(0, neighbourCount);
            return (neighbours[chosenNeighbour], connectingWall[chosenNeighbour]);
        }
        else
        {
            return (-1, 0); 
        }
    }
    //'Breaks' the wall of the node depending on the number returned by setting the gameObject as inactive, order is north east south west - called by DesignMaze
    void BreakWall(int currentNode, int walltoBreak)   
    {
        switch (walltoBreak)
        {
            case 1:
                nodes[currentNode].northWall.SetActive(false);
                break;
            case 2:
                nodes[currentNode].eastWall.SetActive(false);
                break;
            case 3:
                nodes[currentNode].southWall.SetActive(false);
                break;
            case 4:
                nodes[currentNode].westWall.SetActive(false);
                break;
        }
    }
    //Method to generate a random start and end node
    void GenerateStartEnd()
    {
        startNode = UnityEngine.Random.Range(0, totalNodes - 1);
        do
        {
            endNode = UnityEngine.Random.Range(0, totalNodes - 1);
        }
        while (endNode == startNode);

        nodes[startNode].SetNodeTypeMaterial(Node.NodeType.Start);
        nodes[endNode].SetNodeTypeMaterial(Node.NodeType.End);
    }
    //Coroutine for Dijkstra's, A*, and Greedy search algorithms
    IEnumerator FindMazeSolvingPath(Algorithm chosenAlgorithm)  
    {
        //List of open nodes that have been inspected but not yet visited
        List<int> openNodes = new List<int>();
        //List of already visited nodes that can not be visited again
        List<int> visitedNodes = new List<int>();
        //Add the start node to the open nodes list
        openNodes.Add(startNode);
        //Set start node as current node
        int currentNode = startNode;        

        //Loop counter and stopwatch declared and started
        int loopCount = 0;
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();

        //Loop repeats until end node visited
        while (currentNode != endNode)      
        {
            //Only perform loop if algorithm is running
            if (isRunning)
            {
                //Continue running stopwatch
                stopwatch.Start();

                //Get all the neighbours to the current node
                List<int> neighbourNodes = GetListOfNeighbours(currentNode);
                //Current node is now visited
                visitedNodes.Add(currentNode);
                //Current node is no longer open
                openNodes.Remove(currentNode);
                //Setting material of current node
                if (currentNode != startNode)
                {
                    nodes[currentNode].SetNodeTypeMaterial(Node.NodeType.Visited);
                }
                //Inspecting each neighbour of current node
                foreach (int neighbourNode in neighbourNodes)
                {
                    //If neighbour has not been visited
                    if (!visitedNodes.Contains(neighbourNode))
                    {
                        //Setting the new attributes of the unvisited neighbour based off chosen algorithm (gCost is shortest distance from start node via current node, hCost is heuristic distance from end node)
                        int newGCost;
                        switch (chosenAlgorithm)
                        {
                            case Algorithm.Dijkstras:
                                //Calculates new gCost
                                newGCost = nodes[currentNode].gCost + GetGridDistance(currentNode, neighbourNode);
                                //If neighbour has a new lower gCost, set its gCost to the new one
                                if (newGCost < nodes[neighbourNode].gCost || !openNodes.Contains(neighbourNode))
                                {
                                    nodes[neighbourNode].gCost = newGCost;
                                    nodes[neighbourNode].parentNode = currentNode;
                                    //Neighbour has been inspected
                                    if (!openNodes.Contains(neighbourNode))
                                    {
                                        openNodes.Add(neighbourNode);
                                        nodes[neighbourNode].SetNodeTypeMaterial(Node.NodeType.Open);
                                    }
                                }
                                break;
                            case Algorithm.AStar:
                                //Calculates new gCost
                                newGCost = nodes[currentNode].gCost + GetGridDistance(currentNode, neighbourNode);
                                //If neighbour has a new lower gCost, set its gCost to the new one and calculate its hCost
                                if (newGCost < nodes[neighbourNode].gCost || !openNodes.Contains(neighbourNode))
                                {
                                    nodes[neighbourNode].gCost = newGCost;
                                    nodes[neighbourNode].hCost = GetGridDistance(neighbourNode, endNode);
                                    nodes[neighbourNode].parentNode = currentNode;
                                    //Neighbour has been inspected
                                    if (!openNodes.Contains(neighbourNode))
                                    {
                                        openNodes.Add(neighbourNode);
                                        nodes[neighbourNode].SetNodeTypeMaterial(Node.NodeType.Open);
                                    }
                                }
                                break;
                            case Algorithm.Greedy:
                                //If neighbour has not yet been inspected, calculate its hCost
                                if (!openNodes.Contains(neighbourNode))
                                {
                                    nodes[neighbourNode].hCost = GetGridDistance(neighbourNode, endNode);
                                    nodes[neighbourNode].parentNode = currentNode;
                                    //Neighbout has been inspected
                                    openNodes.Add(neighbourNode);
                                    nodes[neighbourNode].SetNodeTypeMaterial(Node.NodeType.Open);
                                }
                                break;
                        }
                    }
                }

                //Choosing the new current node from the list of open nodes based off algorithm
                currentNode = openNodes[0];
                foreach (int openNode in openNodes)
                {
                    switch (chosenAlgorithm)
                    {
                        //Dijkstras chooses the lowest gCost from the open nodes
                        case Algorithm.Dijkstras:
                            if (nodes[openNode].gCost < nodes[currentNode].gCost)
                            {
                                currentNode = openNode;
                            }
                            break;
                        //A* chooses the lowest fCost (gCost + hCost) from the open nodes, breaking ties with the lowest hCost
                        case Algorithm.AStar:
                            if (nodes[openNode].fCost < nodes[currentNode].fCost || nodes[openNode].fCost == nodes[currentNode].fCost && nodes[openNode].hCost < nodes[currentNode].hCost)
                            {
                                currentNode = openNode;
                            }
                            break;
                        //Greedy chooses the lowest hCost from the open nodes
                        case Algorithm.Greedy:
                            if (nodes[openNode].hCost < nodes[currentNode].hCost)
                            {
                                currentNode = openNode;
                            }
                            break;
                    }
                }

                //Setting new chosen node has current node
                nodes[currentNode].SetNodeTypeMaterial(Node.NodeType.Current);

                //Incrementing loop counter and setting stopwatch timer
                loopCount++;
                uiHandler.SetLoopCount(loopCount);
                uiHandler.SetStopWatch(stopwatch.Elapsed);
                //Delay next loop
                yield return loopDelayWait;
            }
            //If algorithm is paused
            else
            {
                //Stop the stopwatch
                stopwatch.Stop();
                //If algorithm has been reset while paused, end the algorithm by setting current node as start node
                if (isResetting)
                {
                    currentNode = endNode;
                }
                //Loop with no delay
                yield return null;
            }
        }
        //Once algorithm is has stopped running, stop the stopwatch
        stopwatch.Stop();
        isRunning = false;

        if (!isResetting)
        {
            //Add all the visited and open nodes together to the same array
            visitedNodes.AddRange(openNodes);
            //Calculate final distance from all the nodes
            int finalDistance = GetFinalDistance(visitedNodes);
            //End the algorithm with display counters
            EndAlgorithm(chosenAlgorithm, loopCount, finalDistance, stopwatch.Elapsed, 1f / loopDelay);
        }
        else
        {
            //Algorithm is no longer resetting
            isResetting = false;
        }
    }
    //Coroutine for right hand rule
    IEnumerator FindRightHandPath()
    {
        //Current node is start node, default direction is north
        int currentNode = startNode;
        char direction = 'N';
        //Visited nodes list declared
        List<int> visitedNodes = new List<int>();
        //Loop counter and stopwatch declared and started
        int loopCount = 0;
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();

        //Loop repeats until end node visited
        while (currentNode != endNode)
        {
            //Only perform loop if algorithm is running
            if (isRunning)
            {
                //Start stopwatch
                stopwatch.Start();

                //If current node has not been visited yet, add it to list
                if (!visitedNodes.Contains(currentNode))
                {
                    visitedNodes.Add(currentNode);
                }
                //If current node is not start node, set its type to visited
                if (currentNode != startNode)
                {
                    nodes[currentNode].SetNodeTypeMaterial(Node.NodeType.Visited);
                }
                //Choosing the next node to move to
                int newNode = -1;
                bool canMove = false;
                do
                {
                    switch (direction)
                    {
                        //If you are facing north...
                        case 'N':
                            //... and there is no wall to the east
                            if (nodes[currentNode].eastWall.activeSelf == false)        
                            {
                                //currentNode + 1 is the east node
                                newNode = currentNode + 1;
                                //Move to the east node
                                canMove = true;
                                //Rotate clockwise (towards east)
                                direction = 'E';                                       
                            }
                            else
                            {
                                //Otherwise rotate anticlockwise (towards west)
                                direction = 'W';                                        
                            }
                            break;
                        //If you are facing east...
                        case 'E':
                            //... and there is no wall to the south
                            if (nodes[currentNode].southWall.activeSelf == false)
                            {
                                //currentNode - xSize is the south node
                                newNode = currentNode - xSize; 
                                //Move to the south node
                                canMove = true;
                                //Rotate clockwise (towards south)
                                direction = 'S';
                            }
                            else
                            {
                                //otherwise rotate anticlockwise (towards north)
                                direction = 'N';
                            }
                            break;
                        //If you are facing south...
                        case 'S':
                            //... and there is no wall to the west
                            if (nodes[currentNode].westWall.activeSelf == false)
                            {
                                //currentNode - 1 is the west node
                                newNode = currentNode - 1;     
                                //Move to the west node
                                canMove = true;
                                //Rotate clockwise (towards west)
                                direction = 'W';
                            }
                            else
                            {
                                //otherwise rotate anticlockwise (towards east)
                                direction = 'E';
                            }
                            break;
                        //If you are facing west...
                        case 'W':
                            //... and there is no wall to the north
                            if (nodes[currentNode].northWall.activeSelf == false)
                            {
                                //currentNode + xSize is the north node
                                newNode = currentNode + xSize;           
                                //Move to the north node
                                canMove = true;
                                //Rotate clockwise (towards north)
                                direction = 'N';
                            }
                            else
                            {
                                //otherwise rotate anticlockwise (towards south)
                                direction = 'S';
                            }
                            break;
                    }
                } while (!canMove); //repeat until you can move
                //If the chosen node has not been visited
                if (!visitedNodes.Contains(newNode))
                {
                    //Set its parent as the current node
                    nodes[newNode].parentNode = currentNode;
                }
                //Set current node as new chosen node
                currentNode = newNode;
                nodes[currentNode].SetNodeTypeMaterial(Node.NodeType.Current);
                //Incrementing loop counter and setting stopwatch timer
                loopCount++;
                uiHandler.SetLoopCount(loopCount);
                uiHandler.SetStopWatch(stopwatch.Elapsed);
                //Delay next loop
                yield return loopDelayWait;
            }
            //If algorithm is paused
            else
            {
                //Stop stopwatch
                stopwatch.Stop();
                //If algorithm has been reset while paused, end the algorithm by setting current node as start node
                if (isResetting)
                {
                    currentNode = endNode;
                }
                //Loop with no delay
                yield return null;
            }
        }
        //Once algorithm has stopped running, stop the stopwatch
        stopwatch.Stop();

        if (!isResetting)
        {
            //Calculate final distance from all the nodes
            int finalDistance = GetFinalDistance(visitedNodes);
            //End the algorithm with display counters
            EndAlgorithm(Algorithm.RightHand, loopCount, finalDistance, stopwatch.Elapsed, 1f / loopDelay);
        }
        else
        {
            //Algorithm is no longer resetting
            isResetting = false;
        }
    }
    //Gets a list of all the neighbouring nodes of a node
    List<int> GetListOfNeighbours(int currentNode)
    {
        //List of neighbour nodes
        List<int> neighbourNodes = new List<int>();
        //Checks node is not in top row, and there is no north wall
        if (currentNode < totalNodes - xSize && !nodes[currentNode].northWall.activeSelf)   
        {
            neighbourNodes.Add(currentNode + xSize); //Add north neighbour
        }
        //Checks node is not in right column, and there is no east wall
        if ((currentNode + 1) % xSize != 0 && !nodes[currentNode].eastWall.activeSelf)  
        {
            neighbourNodes.Add(currentNode + 1); //Add east neighbour
        }
        //Checks node is not in bottom row, and there is no south wall
        if (currentNode >= xSize && !nodes[currentNode].southWall.activeSelf)   
        {
            neighbourNodes.Add(currentNode - xSize); //Add south neighbour
        }
        //Checks node is not in left column, and there is no west wall
        if (currentNode % xSize != 0 && !nodes[currentNode].westWall.activeSelf)   
        {
            neighbourNodes.Add(currentNode - 1); //Add west neighbour
        }
        //Returns list of neighbours
        return neighbourNodes;  
    }
    //Get Manhattan grid distance between two nodes, used for heuristic cost
    int GetGridDistance(int nodeA, int nodeB)   
    {
        return (Math.Abs(nodes[nodeA].xGrid - nodes[nodeB].xGrid) + Math.Abs(nodes[nodeA].yGrid - Math.Abs(nodes[nodeB].yGrid)));
    }
    //Gets the shortest path distance and sets the material of all the nodes
    int GetFinalDistance(List<int> leftoverNodes) 
    {
        //List of the indexes of the nodes in the final path
        List<int> finalPath = new List<int>();
        //Start with the end node
        int currentNode = endNode;
        //Adding the parent of each node (backtracking through the path), terminating once the start node is reached
        while (currentNode != startNode)
        {
            finalPath.Add(currentNode);
            currentNode = nodes[currentNode].parentNode;
            nodes[currentNode].SetNodeTypeMaterial(Node.NodeType.FinalPath);
        }
        //Add start node to the final path
        finalPath.Add(startNode);
        nodes[startNode].SetNodeTypeMaterial(Node.NodeType.Start);
        nodes[endNode].SetNodeTypeMaterial(Node.NodeType.End);
        //Erasing working out by setting non-path nodes to default colour
        foreach (int leftoverNode in leftoverNodes)         
        {
            if (!finalPath.Contains(leftoverNode))
            {
                nodes[leftoverNode].SetNodeTypeMaterial(Node.NodeType.Default);
            }
        }
        //Returning final path cost
        return finalPath.Count - 1;
    }
    //Ending the algorithm
    void EndAlgorithm(Algorithm chosenAlgorithm, int _loopCount, int finalDistance, TimeSpan timeTaken, float rate)
    {
        //Stop stopwatch
        stopwatch.Stop();
        uiHandler.SetStopWatch(timeTaken); 
        //Enable UI buttons
        uiHandler.EndAlgorithmEnables();
        //Display final distance
        uiHandler.SetFinalDistance(finalDistance); 
        //Save test data
        testData = ((int)chosenAlgorithm).ToString() + ',' + ((int)currentDifficulty).ToString() + ',' + _loopCount + ',' + finalDistance + ',' + xSize + ',' + ySize + ',' + timeTaken.TotalSeconds.ToString("n2") + ',' + rate.ToString("n2");
    }
}