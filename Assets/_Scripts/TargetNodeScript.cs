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
            //Debug.Log(line.dot2.x + " " + center.x + "    " + line.dot2.y + " " + center.y);
            //if ((line.dot2.x < center.x + 0.6f && line.dot2.x > center.x - 0.6f) && (line.dot2.y < center.y + 0.6f && line.dot2.y > center.y - 0.6f))
            //{
            if (line.startLine)
            {

                line.targetNode = GetComponent<TargetNodeScript>();
                if (line.parentTargetNode != line.targetNode)
                {

                    line.activateAnswer = false;
                    line.answerNode = null;
                    OnMouseEnterFunction();
                }
                else
                {
                    line.targetNode = null;
                }
            }
            //}
        }
    }


    private void OnMouseExit()
    {
        if (!initializator)
        {
            if (line != null && line.startLine)
            {
                line.targetNode = null;
            }
            OnMouseExitFunction();
        }
    }
}
