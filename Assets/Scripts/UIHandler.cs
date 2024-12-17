using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
//UI handler - handles inputs and outputs of the maze generator
public class UIHandler : MonoBehaviour
{
    //Default and max maze values
    const int DEFAULT_WIDTH = 10, DEFAULT_HEIGHT = 10;
    const int MAX_GRID_SIZE = 100;
    const float DEFAULT_RATE = 100f, MAX_RATE = 120f, MIN_RATE = 0.5f;
    //Buttons
    public Button ResetButton, GenerateButton, RunButton, PauseButton, PlayButton, StartChooserButton, EndChooserButton, SaveTestButton, ViewTestsButton;
    //Text outputs
    public TextMeshProUGUI LoopCountDisplay, StopwatchDisplay, FinalDistanceDisplay;
    //Text inputs
    public TMP_InputField xInput, yInput, rateInput;
    //Input value variables
    int tempWidth, tempHeight, chosenAlgorithm, chosenDifficulty;
    //Run rate
    float tempRate;
    //Other scripts
    MazeGenerator mazeGenerator;
    NodeController nodeController;
    // Start is called before the first frame update
    void Start()
    {
        //Finding the other scripts
        mazeGenerator = GameObject.Find("Maze").GetComponent<MazeGenerator>();
        nodeController = GameObject.Find("Maze").GetComponent<NodeController>();
        //Setting the default values
        tempWidth = DEFAULT_WIDTH;
        xInput.placeholder.GetComponent<TextMeshProUGUI>().text = DEFAULT_WIDTH.ToString();
        tempHeight = DEFAULT_HEIGHT;
        yInput.placeholder.GetComponent<TextMeshProUGUI>().text = DEFAULT_HEIGHT.ToString();
        tempRate = DEFAULT_RATE;
        rateInput.placeholder.GetComponent<TextMeshProUGUI>().text = DEFAULT_RATE.ToString();
    }
    //BOTTOM PANEL INPUTS:
    //Procedure is run by the Algorithm chooser dropdown - decides which algorithm to be run
    public void ChooseAlgorithm(int input)  
    {
        chosenAlgorithm = input;
    }
    //Procedure is run by the RunAlgorithm button - runs the chosen algorithm
    public void RunOnClick()            
    {
        //Can not run the algorithm when right hand rule is chosen but the maze isn't perfect
        if (!(chosenAlgorithm == 3 && mazeGenerator.currentDifficulty != MazeGenerator.MazeDifficulty.Perfect))
        {
            mazeGenerator.RunAlgorithm(tempRate, (MazeGenerator.Algorithm)chosenAlgorithm);

            PauseButton.interactable = true;

            RunButton.interactable = false;
            GenerateButton.interactable = false;
            ResetButton.interactable = false;
            StartChooserButton.interactable = false;
            EndChooserButton.interactable = false;
            nodeController.nodePreview.SetActive(false);
        }

    }
    //Procedure is run by the Reset button - resets maze to default nodes
    public void ResetOnClick()  
    {
        mazeGenerator.ResetMaze();

        GenerateButton.interactable = true;
        RunButton.interactable = true;
        StartChooserButton.interactable = true;
        EndChooserButton.interactable = true;

        ResetButton.interactable = false;
        PlayButton.interactable = false;
        SaveTestButton.interactable = false;
    }
    //Procedure is run by the Generate Maze button - generates the maze
    public void GenerateOnClick()       
    {
        mazeGenerator.GenerateRandomMaze(tempWidth, tempHeight, (MazeGenerator.MazeDifficulty)chosenDifficulty);
        RunButton.interactable = true;
    } 
    //Procedure is run by Pause button - pauses the algorithm running
    public void PauseOnClick()
    {
        PauseButton.interactable = false;

        PlayButton.interactable = true;
        ResetButton.interactable = true;

        mazeGenerator.PauseAlgorithm();
    }
    //Procedure is run by Play button - continues to play the paused algorithm
    public void PlayOnClick()
    {
        PlayButton.interactable = false;
        ResetButton.interactable = false;

        PauseButton.interactable = true;

        mazeGenerator.PlayAlgorithm();
    }
    //Procedure is run by Save Test button - saves the completed algorithm run as a new test
    public void SaveOnClick()
    {
        SaveTestButton.interactable = false;
        mazeGenerator.SaveTest();
    }
    //Procedure is run by View Tests button - views the list of previous tests from the text file
    public void ViewOnClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    //Procedure is run by Return to Main Menu button - returns to main menu
    public void ReturnOnClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    //SIDE PANEL:
    //Procedure is run by the Xinput text input, setting the width of the maze
    public void WidthInput(string input)        
    {
        if (int.TryParse(input, out tempWidth))
        {
            if (tempWidth <= 1)
            {
                tempWidth = 2;
                xInput.text = "2";
            }
            else if (tempWidth >= MAX_GRID_SIZE)
            {
                tempWidth = MAX_GRID_SIZE;
                xInput.text = MAX_GRID_SIZE.ToString();
            }
        }
        else
        {
            tempWidth = DEFAULT_WIDTH;
            xInput.text = null;
        }
    }
    //Procedure is run by the Yinput text input, setting the height of the maze
    public void HeightInput(string input)        
    {
        if (int.TryParse(input, out tempHeight))
        {
            if (tempHeight <= 1)
            {
                tempHeight = 2;
                yInput.text = "2";
            }
            else if (tempHeight >= MAX_GRID_SIZE)
            {
                tempHeight = MAX_GRID_SIZE;
                yInput.text = MAX_GRID_SIZE.ToString();
            }
        }
        else
        {
            tempHeight = DEFAULT_HEIGHT;
            yInput.text = null;
        }
    }
    //Procedure is run by the Rate text input, setting the wait time between each loop of an algorithm
    public void RateInput(string input)     
    {
        if (float.TryParse(input, out tempRate))
        {
            if (tempRate < MIN_RATE)
            {
                tempRate = MIN_RATE;
                rateInput.text = $"{MIN_RATE}";
            }
            else if (tempRate > MAX_RATE)
            {
                tempRate = MAX_RATE;
                rateInput.text = $"{MAX_RATE}";
            }
        }
        else
        {
            tempRate = DEFAULT_WIDTH;
            rateInput.text = null;
        }
    }
    //Procedure is run by the Difficulty chooser dropdown, deciding which difficulty to generate the maze on
    public void ChooseMazeDifficulty(int input)      
    {
        chosenDifficulty = input;
    }
    //ENABLES AND DISPLAY:
    //Procedure run after algorithm finishes running
    public void EndAlgorithmEnables()
    {
        ResetButton.interactable = true;
        SaveTestButton.interactable = true;

        PlayButton.interactable = false;
        PauseButton.interactable = false;
    }
    //Procedure to set loop counter
    public void SetLoopCount(int loopCount)
    {
        LoopCountDisplay.text = loopCount.ToString();
    }
    //Procedure to set final distance
    public void SetFinalDistance(int finalDistance)
    {
        FinalDistanceDisplay.text = finalDistance.ToString();
    }
    //Procedure to set stopwatch time
    public void SetStopWatch(TimeSpan ts)
    {
        int totalSeconds = (int)Math.Floor(ts.TotalSeconds);
        StopwatchDisplay.text = String.Format("{0:00}:{1:00}", totalSeconds, ts.Milliseconds / 10);
    }
}
