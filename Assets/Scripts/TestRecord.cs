using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Test record class object script
[System.Serializable]
public class TestRecord
{
    //Record attributes
    public MazeGenerator.Algorithm algorithmType { get; }
    public MazeGenerator.MazeDifficulty mazeDifficulty { get; }
    public int loopCount { get; }
    public int pathLength{ get; }
    public int mazeWidth{ get; }
    public int mazeHeight{ get; }
    public float algorithmDurationSeconds { get; }
    public float runRate { get; }
    //Constructor - Test data is separated by commas in order of: algorithmType, mazeDifficulty, loopCount, pathLength, mazeWidth, mazeHeight, algorithmDurationSeconds, runRate
    public TestRecord(                    
        MazeGenerator.Algorithm _algorithmType, 
        MazeGenerator.MazeDifficulty _mazeDifficulty,
        int _loopCount,
        int _pathLength,
        int _mazeWidth, 
        int _mazeHeight,
        float _algorithmDurationSeconds, 
        float _runRate
        )    
    {
        algorithmType = _algorithmType;
        mazeDifficulty = _mazeDifficulty;
        loopCount = _loopCount;
        pathLength = _pathLength;
        mazeWidth = _mazeWidth;
        mazeHeight = _mazeHeight;
        algorithmDurationSeconds = _algorithmDurationSeconds;
        runRate = _runRate;

    }
}