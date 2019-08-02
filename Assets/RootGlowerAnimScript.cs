using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;
public class RootGlowerAnimScript : MonoBehaviour
{
    GameManager gameManager;
    ApplicationController app;
    string levelName;
    int levelNum;
    Animator animator;
    bool menuLoading = false;
    private void Start()
    {
        gameManager = GameManager.Instance;
        app = ApplicationController.app;
        animator = GetComponent<Animator>();
        menuLoading = false;
    }

    public void LoadNextLevel()
    {
        if (!menuLoading)
        {
            levelName = gameManager.levelConfigName;
            levelNum = int.Parse((levelName.Substring(levelName.IndexOf("_") + 1))) + 1;
            levelName = "level_" + levelNum.ToString();
            app.LoadLevel(levelName);
            animator.ResetTrigger("EndTheLevel");
        }
    }

    public void LoadLevelMenu()
    {
        animator.SetTrigger("EndTheLevel");
        menuLoading = true;
        app.ToStartup();
    }
}
