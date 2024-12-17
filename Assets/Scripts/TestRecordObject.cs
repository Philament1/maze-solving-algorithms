using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Test record object script - handles the records in the table of tests
public class TestRecordObject : MonoBehaviour
{
    //Test file handler script
    TestFileHandler testFileHandler;
    Text[] testFieldText;
    public void SetAttributesText(TestRecord testRecord, TestFileHandler _testFileHandler)
    {
        testFieldText = GetComponentsInChildren<Text>();
        testFieldText[0].text = testRecord.algorithmType.ToString();
        testFieldText[1].text = testRecord.mazeDifficulty.ToString();
        testFieldText[2].text = testRecord.loopCount.ToString();
        testFieldText[3].text = testRecord.pathLength.ToString();
        testFieldText[4].text = testRecord.mazeWidth.ToString();
        testFieldText[5].text = testRecord.mazeHeight.ToString();
        testFieldText[6].text = testRecord.algorithmDurationSeconds.ToString();
        testFieldText[7].text = testRecord.runRate.ToString();

        testFileHandler = _testFileHandler;
    }

    public void DeleteTest()
    {
        int testIndex = int.Parse(gameObject.name.Substring(10));
        testFileHandler.DeleteTest(testIndex);
        Destroy(gameObject);
    }
}