using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using Controller;

public class GameManager : Management
{
    public static GameManager Instance;
    //Dictionary<string, LevelConfig> levels;
    public string levelConfigName = "";
    string json;
    
    public ApplicationController app;
    [Header("Game")]
    public bool win = false;
    public int levelOpened = 9;
    [Header("LevelPerfomance")]
    public bool inTarget = false;
    public Vector2 endPoint;
    public int requiredAnswers = 3;
    public int numOfRightAnswers = 0;
    public int lineNumber;
    public int firstBound;
    public int secondBound;
    public int thirdBound;
    public int glowormNumber = 3;
    public List<GameObject> gloworms = new List<GameObject>();
    private TextMeshProUGUI currentLinesNumText, boundText;
    //public bool lineActivated = false;
    public List<GameObject> flowers = new List<GameObject>();
    public int nodeNumeration = 50;
    public List<LineData> lines = new List<LineData>();
    Animator rootGlowerAnimator;
    private void Awake()
    {
        #region Singleton
        if (Instance != null)
        {
            return;
        }
        else
        {
            Instance = this;
        }
        #endregion
    }
    private void Start()
    {
        levels = new Dictionary<string, LevelConfig>();
        FindLevelConfig();
    }
    public void FindUI(GameObject curLinesNumText, GameObject curBoundText, GameObject RootGlower, List<GameObject> Gloworms) 
    {
        currentLinesNumText = curLinesNumText.GetComponent<TextMeshProUGUI>();
        boundText = curBoundText.GetComponent<TextMeshProUGUI>();
        gloworms = Gloworms;
        rootGlowerAnimator = RootGlower.GetComponent<Animator>();
    }

    public void FindFlowers(List<GameObject> Flowers)
    {
        flowers = Flowers;
    }

    public void ResetParameters(int _firstBound, int _secondBound, int _thirdBound, int _requiredAnswers)
    {
        firstBound = _firstBound;
        secondBound = _secondBound;
        thirdBound = _thirdBound;
        requiredAnswers = _requiredAnswers;
        lineNumber = 0;
        numOfRightAnswers = 0;
        CheckGloworms(-1);
        currentLinesNumText.text = "0 /";
        boundText.text = firstBound.ToString();
    }

    public void LevelStartAnimation()
    {
        if (flowers != null)
            foreach (GameObject flower in flowers)
            {
                flower.GetComponent<Animator>().SetTrigger("OpenFlowerAnimation");
            }
    }
    public void WinAnimation()
    {
        if (flowers != null && rootGlowerAnimator != null)
        {
            foreach (GameObject flower in flowers)
            {
                flower.GetComponent<Animator>().SetTrigger("EndedFlowerAnimation");
            }
           
            if (levelConfigName != null && levelConfigName != "")
            {
                levels[levelConfigName].gloworms = glowormNumber;
                foreach (var level in levels)
                json = json+JsonConvert.SerializeObject(level.Value)+",\n";

                json = "[\n" + json + "\n]";

                string path = Application.persistentDataPath;
                if (File.Exists(path + @"\level_config.json"))
                {
                    File.WriteAllText(path + @"\level_config.json", json);
                    json = "";
                }
                else
                    Debug.Log("Sorry! Your progress is lost because of dump programmers :-))");
            }

            rootGlowerAnimator.SetTrigger("EndTheLevel");
            //StartCoroutine(LoadMenuCoroutine());
        }
    }

    //IEnumerator LoadMenuCoroutine()
    //{
    //    yield return new WaitForSeconds(4f);
    //    app.ToStartup();
    //}

    public void CheckGloworms(int flag)
    {
        
        if (gloworms != null && boundText != null && currentLinesNumText != null)
        {
            if (flag == 0)
                lineNumber--;
            else if(flag == 1)
                lineNumber++;
            
            currentLinesNumText.text = lineNumber.ToString() + " /";
            if (lineNumber > thirdBound)
            {
                DisableGloworms(gloworms, 3);
                glowormNumber = 0;
                boundText.text = " ...";
            }
            else if (lineNumber > secondBound)
            {
                DisableGloworms(gloworms, 2);
                glowormNumber = 1;
                boundText.text = " " + thirdBound.ToString();
            }
            else if (lineNumber > firstBound)
            {
                DisableGloworms(gloworms, 1);
                glowormNumber = 2;
                boundText.text = " " + secondBound.ToString();
            }
            else
            {
                DisableGloworms(gloworms, 0);
                glowormNumber = 3;
                boundText.text = " " + firstBound.ToString();
            }

        }
    }

    public int CheckRepeatLine(int parentIndex, int targetIndex)
    {
        if (lines != null && lines.Count != 0)
        foreach (LineData line in lines)
        {
            if (line.parent == parentIndex && line.target == targetIndex)
            {
                Debug.Log("repeating!");
                return 1;
            }
        }
        return 0;
    }

    public struct LineData
    {
        public ShinyLineScript line;
        public int parent;
        public int target;

        public LineData(ShinyLineScript shinyLineScript,int Parent, int Target)
        {
            line = shinyLineScript;
            parent = Parent;
            target = Target;
        }
    }

}
