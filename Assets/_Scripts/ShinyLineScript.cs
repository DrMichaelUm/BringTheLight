﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShinyLineScript : Line/*, IPointerDownHandler, IPointerUpHandler*/
{
    LineRenderer lineRenderer;
    EdgeCollider2D edgeCol;
    Vector3 mousePosition;
    List<Vector2> points = new List<Vector2>();
    Vector2 dot1, dot2;
    public Vector2 endPoint;
    public TargetNodeScript targetNode;
    public AnswerNodeScript answerNode;
    public TargetNodeScript parentTargetNode;
    public int parentNodeIndex;
    int targetNodeIndex;
    ParticleSystem ps;
    public RectTransform particleTransform;
    RectTransform rt;
    public bool activateAnswer = false;
    float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;
    GameManager gameManager;
    Quaternion _lookRotation;
    Vector3 _direction;
    float tempAxis;
    bool destroyLine = false;
    private void Start()
    {
        rt = GetComponent<RectTransform>();
        lineRenderer = GetComponent<LineRenderer>();
        edgeCol = GetComponent<EdgeCollider2D>();
        ps = GetComponentInChildren<ParticleSystem>();
        mat = ps.GetComponent<Renderer>().material;
        mat.SetColor("_TintColor", col);
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            if (startLine)
            {
                mousePosition = Camera.main.ScreenToWorldPoint((new Vector2(Input.mousePosition.x, Input.mousePosition.y)));

                dot1 = center;
                dot2 = (Vector2)(new Vector3(mousePosition.x, mousePosition.y, 0));

                LineBehaviour();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (startLine)
            {
                if (gameManager.inTarget)
                {
                    endPoint = gameManager.endPoint;
                    dot2 = endPoint;

                    LineBehaviour();

                    this.gameObject.layer = 0;

                    if (activateAnswer)
                    {
                        if (answerNode != null && !answerNode.activated)
                        {
                            targetNodeIndex = answerNode.index;
                            if (gameManager.CheckRepeatLine(parentNodeIndex, targetNodeIndex) != 1)
                            {
                                answerNode.inColors.Add(col);
                                answerNode.col = MixColors(answerNode.inColors);
                                answerNode.activated = true;
                                answerNode.CheckAnswer();
                            }
                            else
                            {
                                destroyLine = true;
                            }
                        }
                    }
                    else
                    if (targetNode != null && !targetNode.initializator)
                    {
                        targetNodeIndex = targetNode.index;
                        if (gameManager.CheckRepeatLine(parentNodeIndex, targetNodeIndex) != 1) {
                            if (!targetNode.activate)
                            {
                                targetNode.activate = true;
                            }
                            targetNode.inColors.Add(col);
                            targetNode.col = MixColors(targetNode.inColors);
                            targetNode.col = NormilizeColor(targetNode.col);
                            targetNode.mat.SetColor("_TintColor", targetNode.col);
                        }
                        else
                        {
                            destroyLine = true;
                        }
                    }

                    if (destroyLine)
                    {
                        DestroyLine();
                    }
                    else
                    {
                        gameManager.CheckGloworms(1);
                        var lineData = new GameManager.LineData(this, parentNodeIndex, targetNodeIndex);
                        gameManager.lines.Add(lineData);

                        if (parentTargetNode != null)
                            parentTargetNode.numOfLines++;
                        gameManager.inTarget = false;
                    }
                }
                else
                {
                    DestroyLine();
                }
                startLine = false;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("TargetNode"))
        {
            targetNode = collision.GetComponent<TargetNodeScript>();
            if (parentTargetNode == targetNode)
            {
                targetNode = null;
            }
            else
            {
                activateAnswer = false;
                answerNode = null;
            }
        }
        if (collision.CompareTag("AnswerNode"))
        {
            answerNode = collision.GetComponent<AnswerNodeScript>();
            activateAnswer = true;
            targetNode = null;
        }
    }

    private void OnMouseDown()
    {
        if (activateAnswer || (targetNode!=null && !targetNode.initializator))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;

            if (clicked > 1 && Time.time - clicktime < clickdelay)
            {
                clicked = 0;
                clicktime = 0;

                if (parentTargetNode != null)
                {
                    parentTargetNode.numOfLines--;
                    if (parentTargetNode.numOfLines == 0)
                        parentTargetNode.initializator = false;
                }

                if (activateAnswer)
                {
                    answerNode.inColors.Remove(col);
                    answerNode.col = MixColors(answerNode.inColors);
                    answerNode.CheckAnswer();
                    activateAnswer = false;
                    answerNode.activated = false;
                }
                else
                {
                    targetNode.inColors.Remove(col);
                    targetNode.col = MixColors(targetNode.inColors);
                    targetNode.col = NormilizeColor(targetNode.col);
                    if (targetNode.inColors.Count != 0)
                    {
                        targetNode.mat.SetColor("_TintColor", targetNode.col);
                    }
                    else
                    {
                        targetNode.mat.SetColor("_TintColor", targetNode.originColor);
                        targetNode.activate = false;

                    }
                }
                var lineData = new GameManager.LineData(this, parentNodeIndex, targetNodeIndex);
                gameManager.lines.Remove(lineData);
                gameManager.CheckGloworms(0);
                DestroyLine();


            }
            else if (Time.time - clicktime > 1) clicked = 0;
        }
    }

    private void LineBehaviour()
    {
        //this.transform.position = new Vector2((dot1.x + dot2.x) / 2, (dot1.y + dot2.y) / 2);
        lineRenderer.SetPosition(0, dot1);
        lineRenderer.SetPosition(1, dot2);

        #region Настраиваем партикл
        particleTransform.position = new Vector2((dot1.x + dot2.x) / 2, (dot1.y + dot2.y) / 2);
        _direction = (dot1 - (Vector2)particleTransform.transform.position).normalized;
        _lookRotation = Quaternion.LookRotation(_direction);
        tempAxis = _lookRotation.x;
        _lookRotation.x = 0f;
        _lookRotation.y = 0f;
        if (dot1.x < dot2.x)
        {
            _lookRotation.z = tempAxis;
        }
        else
        {
            _lookRotation.z = -tempAxis;
        }
        particleTransform.rotation = _lookRotation;
        var psShape = ps.shape;
        psShape.scale = new Vector3(Mathf.Sqrt(Mathf.Pow((dot2.x - dot1.x), 2) + Mathf.Pow((dot2.y - dot1.y), 2))*0.9f, 0f, 0f);
        #endregion
            points.Add(dot1);
            points.Add(dot2);
        edgeCol.points = points.ToArray();
        points.Clear();
    }

    void DestroyLine()
    {
        if (parentTargetNode != null && parentTargetNode.numOfLines <= 0)
            parentTargetNode.initializator = false;
        Destroy(gameObject);

    }
}