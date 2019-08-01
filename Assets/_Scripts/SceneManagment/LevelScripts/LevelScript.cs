using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    GameManager gameManager;
    public string levelName;
    public List<GameObject> flowers;
    public int firstBound;
    public int secondBound;
    public int thirdBound;
    public int requiredAnsewers;
    private void Awake()
    {
        gameManager = GameManager.Instance;
        gameManager.levelConfigName = levelName;
        gameManager.FindFlowers(flowers);
        gameManager.ResetParameters(firstBound, secondBound, thirdBound, requiredAnsewers);
    }
}
