using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int width = 8;
    public int height = 8;
    public List<string> words;
    public float timeLimit = 30f;
}
