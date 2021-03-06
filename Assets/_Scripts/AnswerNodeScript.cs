﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerNodeScript : Node
{
    public bool rightAnswer = false;
    public ParticleSystem rightAnswerEffect;
    ShinyLineScript line;
    Animator animator;
    private void Start()
    {
        index = GameManager.Instance.nodeNumeration;
        GameManager.Instance.nodeNumeration--;
        animator = GetComponent<Animator>();
        ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            mat = ps.GetComponent<Renderer>().material;
        else Debug.Log("Fuck!");
        col = mat.GetColor("_TintColor");
        originColor = col;
        //inColors.Add(col);
        //numCol++;
        var rightEffect = rightAnswerEffect.main;
        rightEffect.startColor = originColor;
        center = new Vector2(transform.position.x, transform.position.y);
        //animator.SetTrigger("AnsNdFadeIn");
    }
    public void CheckAnswer()
    {

        if (this.col == originColor)
        {
            rightAnswer = true;
            GameManager.Instance.numOfRightAnswers++;
            Debug.Log(GameManager.Instance.numOfRightAnswers + " right answers!");
            rightAnswerEffect.Play();
        }
        else
        {
            if (rightAnswer)
            {
                GameManager.Instance.numOfRightAnswers--;
                rightAnswer = false;
            }
            rightAnswerEffect.Stop();
        }
        if (GameManager.Instance.numOfRightAnswers == GameManager.Instance.requiredAnswers)
        {
            GameManager.Instance.win = true;
            GameManager.Instance.WinAnimation();
            //animator.SetTrigger("AnsNdFadeOut");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ShinyLine"))
            if (!activate)
        {
            line = other.GetComponent<ShinyLineScript>();
            if (!activate && line.startLine)
            {
                OnMouseEnterFunction();
                if (!line.detectedNodes.Contains(gameObject))
                    line.detectedNodes.Add(gameObject);
                line.activateAnswer = true;
                line.targetNode = null;
            }
        }
    }
    private void OnMouseExit()
    {
        //if (line != null && line.startLine)
        //{
        //    line.answerNode = null;
        //    line.activateAnswer = false;
        //}
        OnMouseExitFunction();

    }
}
