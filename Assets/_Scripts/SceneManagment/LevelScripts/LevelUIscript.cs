using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelUIscript : MonoBehaviour
{
    GameManager gameManager;
    public List<GameObject> gloworms;
    public GameObject RootGlower;
    public GameObject LinesNumText, boundText;
    private void Awake()
    {
        gameManager = GameManager.Instance;
        gameManager.FindUI(LinesNumText, boundText, RootGlower, gloworms);
    }
}
