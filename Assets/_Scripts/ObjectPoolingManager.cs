using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager Instance;

    private List<GameObject> shinyLines;
    [SerializeField] [Tooltip("Префаб линии")]
    private GameObject shinyLinePrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        shinyLines = new List<GameObject>();
    }

    public GameObject GetShinyLine(Transform parent)
    {
        for (int i = 0; i < shinyLines.Count; i++) //ищем доступный для переиспользования объект
        {
            if (shinyLines[i] != null && !shinyLines[i].activeInHierarchy)
            {
                shinyLines[i].SetActive(true);
                shinyLines[i].transform.SetParent(parent);
                return shinyLines[i];
            }
        }
        //если таких нет
        GameObject newObj = Instantiate(shinyLinePrefab);
        newObj.transform.SetParent(parent);
        shinyLines.Add(newObj);
        return newObj;
    }
}
