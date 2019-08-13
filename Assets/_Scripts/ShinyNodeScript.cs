using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShinyNodeScript : Node
{

    private void Start()
    {
        InitFunction();
        index = GameManager.Instance.nodeNumeration;
        GameManager.Instance.nodeNumeration--;
    }

    private void OnMouseDown()
    {
        InitLine(this.transform);
        activeLine.parentTargetNode = GetComponent<Node>();
        activeLine.parentNodeIndex = index;
    }

}
