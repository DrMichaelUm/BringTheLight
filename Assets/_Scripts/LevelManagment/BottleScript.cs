using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using TMPro;
public class BottleScript : Management
{
    GameManager gameManager;

    public GameObject currentGlowNum;
    public GameObject restrictionLianas1;
    public GameObject restrictionLianas2;
    public RectTransform scrollPane;
    public ParticleSystem ps;
    public int inLevels = 9;
    TextMeshProUGUI currentGlowNumText;
    float expandScrolling_num;
    int glowNumber = 0;
    public int requiredBound;
    public int bottleNumber;
    int startLevelOfBlock;
    int iterations = 0;
    int levelBlockNumber = 9;
    string json;
    private void Awake()
    {
        gameManager = GameManager.Instance;
        currentGlowNumText = currentGlowNum.GetComponent<TextMeshProUGUI>();
        ps = GetComponentInChildren<ParticleSystem>();
        bottles = new Dictionary<int, BottleConfig>();
        levels = new Dictionary<string, LevelConfig>();
        expandScrolling_num = Screen.currentResolution.height;
    }
    private void Start()
    {
        FindBottleConfig();
        FindLevelConfig();
        iterations = levelBlockNumber * bottleNumber;
        startLevelOfBlock = iterations - levelBlockNumber;

        int k = 0;
        foreach (var level in levels)
        {
            k++;
            if ((k > startLevelOfBlock))
            if ((startLevelOfBlock < iterations))
            {
                glowNumber += levels[level.Key].gloworms;
                startLevelOfBlock++;
            }
            else break;
            
        }
        //Debug.Log(bottleNumber + " " + glowNumber);

        if (bottleNumber != 1)
        for (int i = 1; i < bottleNumber; i++)
        {
            glowNumber = glowNumber + bottles[i].gloworms;
                //Debug.Log(bottleNumber + " " + glowNumber+" "+ bottles[i].gloworms);
        }

        bottles[bottleNumber].gloworms = glowNumber;
        currentGlowNumText.text = glowNumber.ToString()+" /";
        var particleMain =ps.main; 
        particleMain.maxParticles = glowNumber;

        if (bottles[bottleNumber].gloworms >= requiredBound)
        {
            ActivateFullnessAnimation();
            ActivateFullness();
        }

        foreach (var bottle in bottles)
            json = json + JsonConvert.SerializeObject(bottle.Value) + ",\n";

        json = "[\n" + json + "\n]";

        string path = Application.persistentDataPath;
        
        if (File.Exists(path + @"\bottle_config.json"))
        {
            File.WriteAllText(path + @"\bottle_config.json", json);
            json = "";
        }
        else
            Debug.Log("Sorry! Your progress is lost because of dump programmers :-))");
    }

    protected void ActivateFullness()
    {
        gameManager.levelOpened += inLevels;
        restrictionLianas1.SetActive(false);
        restrictionLianas2.SetActive(false);
    }

    protected void ActivateFullnessAnimation()
    {
        bottles[bottleNumber].isFull = true;
        //Debug.Log("Well.."+(scrollPane.GetComponent<RectTransform>().rect.height + expandScrolling_num).ToString());
        scrollPane.sizeDelta = new Vector2(scrollPane.sizeDelta.x, scrollPane.sizeDelta.y + Screen.currentResolution.height);


    }
}
