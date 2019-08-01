using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetNodeScript : Node
{
    public bool activate = false;
    public bool initializator = false;
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
                OnMouseEnterFunction();
        }
    }


    private void OnMouseExit()
    {
       if (!initializator)
        OnMouseExitFunction();
    }
}
