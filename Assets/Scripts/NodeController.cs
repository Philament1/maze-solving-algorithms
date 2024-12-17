using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Node Controller script - used for selecting the start and end nodes
public class NodeController : MonoBehaviour
{
    //Preview of the selector when hovering over the node
    public GameObject nodePreview;
    //Check if start node or end node is selected
    bool isStartNode;
    //Maze generator script
    MazeGenerator mazeGenerator;
    //Material preview for start and end node when hovering over the node
    Material startPreviewMaterial, endPreviewMaterial;
    // Start is called before the first frame update
    void Start()
    {
        //Finding each object in the resources folder
        mazeGenerator = GameObject.Find("Maze").GetComponent<MazeGenerator>();
        startPreviewMaterial = Resources.Load<Material>("Materials/PreviewMaterials/Start");
        endPreviewMaterial = Resources.Load<Material>("Materials/PreviewMaterials/End");
        nodePreview = Instantiate(Resources.Load<GameObject>("Prefabs/Floor"));
        nodePreview.name = "Node Preview";
        Destroy(nodePreview.GetComponent<BoxCollider>());
        nodePreview.GetComponent<MeshRenderer>().material = startPreviewMaterial;
        nodePreview.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //If the node preview is active
        if (nodePreview.activeSelf)
        {
            //Fire a ray from the mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //If the ray hits an object
            if (Physics.Raycast(ray, out hit, 100))
            {
                //Checking the name of the object and splitting its name into substrings by space
                string[] hitName = hit.transform.gameObject.name.Split(' ');
                //If the object hit is the floor object of a node
                if (hitName[0] == "Floor")
                {
                    //The node preview is displayed over the floor object
                    nodePreview.transform.position = new Vector3(hit.transform.gameObject.transform.position.x, 0, hit.transform.gameObject.transform.position.z);
                    //If mouse is clicked
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Choose the node hovered as the start/end node
                        if (isStartNode)
                        {
                            mazeGenerator.SetStartNode(int.Parse(hitName[1]));
                        }
                        else
                        {
                            mazeGenerator.SetEndNode(int.Parse(hitName[1]));
                        }
                        //Disable the node preview
                        nodePreview.SetActive(false);
                    }
                }
            }
        }
    }

    public void SelectNode(bool _isStartNode)
    {
        isStartNode = _isStartNode;
        nodePreview.SetActive(true);
        if (isStartNode)
        {
            nodePreview.GetComponent<MeshRenderer>().material = startPreviewMaterial;
        }
        else
        {
            nodePreview.GetComponent<MeshRenderer>().material = endPreviewMaterial;
        }
    }
}