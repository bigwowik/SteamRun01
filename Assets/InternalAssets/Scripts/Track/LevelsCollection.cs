using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.VisualBasic;
using System.IO;
using System.Globalization;

[CreateAssetMenu(fileName = "NewLevelsData", menuName = "Levels/LevelsData", order = 1)]
public class LevelsCollection : ScriptableObject
{
    public TextAsset[] levels;
    public float[] startSpeedLevels;

    public Dictionary<int, LevelData> levelDataDict = new Dictionary<int, LevelData>();
    


    public void UpdateLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            var levelName = "Level " + i;
            var newLevelData = new LevelData(levelName, startSpeedLevels[i], ReadCSV(levels[i]));
            
            levelDataDict.Add(i, newLevelData);
            Debug.Log($"Level {levelName} was added.");
        }

    }

    List<LevelTileData> ReadCSV(TextAsset text)
    {

        using (TextReader tr = new StringReader(text.ToString()))
        {
            string str;
            int lineIndex = 0;
            List<LevelTileData> level = new List<LevelTileData>();

            while ((str = tr.ReadLine()) != null)
            {

                var values = str.Split(',');
                level.Add(new LevelTileData(values[0], values[1]));
                //level.Insert(lineIndex,new LevelTileData(values[0], values[1]));



                //do something with your line
            }
            return level;


        }
    }

    

    
}

public class LevelData 
{
    public string name;
    public float startSpeed;
    public List<LevelTileData> levelTileDatas;

    public LevelData(string name, float startSpeed, List<LevelTileData> levelTileDatas)
    {
        this.name = name;
        this.startSpeed = startSpeed;
        this.levelTileDatas = levelTileDatas;
    }
}


public class LevelTileData 
{
    public string leftSide;
    public string rightSide;
    public LevelTileData(string leftSide, string rightSide)
    {
        this.leftSide = leftSide;
        this.rightSide = rightSide;
    }
    
}


