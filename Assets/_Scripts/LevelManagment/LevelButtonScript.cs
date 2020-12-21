using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
public class LevelButtonScript : Management
{

    public List<GameObject> gloworms = new List<GameObject>();
    public string levelName;
    private void Awake()
    {
        levels = new Dictionary<string, LevelConfig>();
    }

    private void Start()
    {

        FindLevelConfig();

        if (gloworms != null && gloworms.Count != 0)
        {
            DisableGloworms(gloworms, 3 - levels[levelName].gloworms);
        }
    }


}
