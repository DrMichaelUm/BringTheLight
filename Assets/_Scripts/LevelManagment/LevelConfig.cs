using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelConfig
{
    public string level;
    public int gloworms;

    public LevelConfig(string levelName, int Gloworms)
    {
        level = levelName;
        gloworms = Gloworms;
    }

}
