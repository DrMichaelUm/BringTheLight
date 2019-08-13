using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerNodeScript : Node
{
    public bool rightAnswer = false;
    public ParticleSystem rightAnswerEffect;
    ShinyLineScript line;
    private void Start()
    {
        index = GameManager.Instance.nodeNumeration;
        GameManager.Instance.nodeNumeration--;
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ShinyLine"))
            if (!activate)
            {
                OnMouseEnterFunction();
                line = other.GetComponent<ShinyLineScript>();
                line.answerNode = GetComponent<AnswerNodeScript>();
                line.activateAnswer = true;
                line.targetNode = null;
            }
    }
    private void OnMouseExit()
    {
        if (line != null && line.startLine)
        {
            line.answerNode = null;
            line.activateAnswer = false;
        }
        OnMouseExitFunction();

    }
}
