using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetNodeScript : Node
{
    public bool activate = false;
    public bool initializator = false;
    ShinyLineScript line;
    private void Start()
    {
        InitFunction();
        index = GameManager.Instance.nodeNumeration;
        GameManager.Instance.nodeNumeration--;
    }

    private void OnMouseDown()
    {
        if (activate)
        {
            InitLine(this.transform);
            initializator = true;
            activeLine.parentTargetNode = GetComponent<TargetNodeScript>();
            activeLine.parentNodeIndex = index;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ShinyLine") && !initializator)
        {
            //Debug.Log("Line collided!");
            line = collision.GetComponent<ShinyLineScript>();
            {
                if (line.startLine)
                {
                    OnMouseEnterFunction();
                    if (!line.detectedNodes.Contains(gameObject))
                    line.detectedNodes.Add(gameObject);
                }
            }
        }
    }

    //public void AssignTargetNode()
    //{
    //    line.targetNode = GetComponent<TargetNodeScript>();
    //    if (line.parentTargetNode != line.targetNode)
    //    {
    //        line.activateAnswer = false;
    //        line.answerNode = null;
    //    }
    //    else
    //    {
    //        line.targetNode = null;
    //    }
    //}
    private void OnMouseExit()
    {
        if (!initializator)
        {
            //if (line != null && line.startLine)
            //{
            //    line.targetNode = null;
            //}
            OnMouseExitFunction();
        }
    }
}
