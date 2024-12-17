using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    //Change the scene to the Maze Generator scene
    public void StartPathfinder()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Close the application
    public void Quit()
    {
        Application.Quit();
    }

}
