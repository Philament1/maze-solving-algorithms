using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

//Test file handler script - handles the text file and tests
public class TestFileHandler : MonoBehaviour
{
    //File path to text file
    public const string TEST_FILE_PATH = "/Resources/Tests.txt";
    //Empty object that holds the test content
    [SerializeField] GameObject TestContentHolder;
    //List of tests
    [SerializeField] List<TestRecord> tests;
    //List of tests in string format
    List<string> testDataList;
    //Prefab of test record
    GameObject testRecordPrefab;
    //Start called when scene loads
    void Start()
    {
        //Finding test record prefab in resources folder
        testRecordPrefab = Resources.Load<GameObject>("Prefabs/TestRecord");
        LoadTestsTextFile();
    }
    //Loads the tests from the text file into the table
    public void LoadTestsTextFile()
    {
        //Destroys previously loaded tests
        foreach (Transform previousRecord in TestContentHolder.transform)
        {
            Destroy(previousRecord.gameObject);
        }
        //Declaring the tests list
        tests = new List<TestRecord>();
        //Declaring the tests list in string format
        testDataList = new List<string>();
        //Using StreamReader to read in the text file
        StreamReader testsFileReader = new StreamReader(Application.dataPath + TEST_FILE_PATH);
        string tempTestData;
        //For each non-empty line of the text file
        while ((tempTestData = testsFileReader.ReadLine()) != null)    
        {
            //Split the line into an array of substrings split by a comma
            string[] testAttributes = tempTestData.Split(',');
            //Attempt to import the test file data into the test record list using the constructor
            try
            {
                TestRecord tempTest = new TestRecord(
                (MazeGenerator.Algorithm)int.Parse(testAttributes[0]),
                (MazeGenerator.MazeDifficulty)int.Parse(testAttributes[1]),
                int.Parse(testAttributes[2]),
                int.Parse(testAttributes[3]),
                int.Parse(testAttributes[4]),
                int.Parse(testAttributes[5]),
                float.Parse(testAttributes[6]),
                float.Parse(testAttributes[7])
                );
                tests.Add(tempTest);
                testDataList.Add(tempTestData);
            }
            catch
            {
            }
        }
        //Close the file
        testsFileReader.Close();
        //For each test in the list, create the record and add it to the table
        for (int i = 0; i < tests.Count; i++)
        {
            GameObject testRecordObject = Instantiate(testRecordPrefab) as GameObject;
            testRecordObject.transform.SetParent(TestContentHolder.transform, false);
            testRecordObject.GetComponent<TestRecordObject>().SetAttributesText(tests[i], this);
            testRecordObject.name = "TestRecord" + i;
        }

    }
    //Deletes the test from the table and text file
    public void DeleteTest(int index)
    {
        testDataList[index] = "deleted";
    }
    //Reloads in the text file to refresh the table
    public void UpdateTextFile()
    {
        StreamWriter testsFileWriter = new StreamWriter(Application.dataPath + TestFileHandler.TEST_FILE_PATH, false);
        for (int i = 0; i < testDataList.Count; i++)
        {
            testsFileWriter.WriteLine(testDataList[i]);
        }
        testsFileWriter.Close();
        LoadTestsTextFile();
    }
    //Returns to the maze generator scene
    public void ReturnToMaze()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
