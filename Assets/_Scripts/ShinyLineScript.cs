using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShinyLineScript : Line/*, IPointerDownHandler, IPointerUpHandler*/
{
    LineRenderer lineRenderer;
    EdgeCollider2D edgeCol;
    Vector3 mousePosition;
    //List<Vector2> points = new List<Vector2>();
    public List<GameObject> detectedNodes = new List<GameObject>();
    //public Vector2 dot1, dot2;
    //public Vector2 endPoint;
    private Vector2[] points = new Vector2[2]; //[0] - начало линии, [1] - конец
    
    public TargetNodeScript targetNode;
    public AnswerNodeScript answerNode;
    public Node parentTargetNode;
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
        gameManager = GameManager.Instance;
        rt = GetComponent<RectTransform>();
        lineRenderer = GetComponent<LineRenderer>();
        edgeCol = GetComponent<EdgeCollider2D>();
    }

    public void Restart()
    {
        Debug.Log("Line Enabled");
        ps = GetComponentInChildren<ParticleSystem>();
        mat = ps.GetComponent<Renderer>().material;
        mat.SetColor("_TintColor", col);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) //пока мышь нажата, просто прорисовываем линию следом за ней
        {
            if (startLine)
            {
                mousePosition = Camera.main.ScreenToWorldPoint((new Vector2(Input.mousePosition.x, Input.mousePosition.y)));

                points[0] = center;
                points[1] = (Vector2)(new Vector3(mousePosition.x, mousePosition.y, 0));

                LineBehaviour();
            }
        }

        if (Input.GetMouseButtonUp(0)) //если отпустить мышь
        {
            if (startLine) //если линия была "начата"
            {
                if (gameManager.inTarget) //если линия попала в какой-то узел
                {
                    if (activateAnswer) //если задели узел ответа?
                    {   
                        if (answerNode != null && !answerNode.activate)
                        {
                            points[1] = answerNode.center;
                            targetNodeIndex = answerNode.index;
                            if (gameManager.CheckRepeatLine(parentNodeIndex, targetNodeIndex) != 1)
                            {
                                answerNode.inColors.Add(col);
                                answerNode.col = MixColors(answerNode.inColors);
                                answerNode.activate = true;
                                gameManager.CheckGloworms(1);
                                answerNode.CheckAnswer();
                            }
                            else
                            {
                                destroyLine = true;
                            }
                        }
                    }
                    else
                    {
                        targetNode = CheckTargetNode(detectedNodes); //выбрать оптимальную TargetNode из задетых
                        if (targetNode != null && parentTargetNode != targetNode && CheckForLoops(parentTargetNode, targetNode) == 0/*&& !targetNode.initializator*/) //если выбран узел и он не равен узлу, с которого мы начали, и мы не попадем в петлю
                        {
                            if (parentTargetNode == null)
                                Debug.Log("no parent");
                            else if (parentTargetNode.outLines == null)
                                Debug.Log("no list");
                            else
                                parentTargetNode.outLines.Add(this); //устанавливаем исходящую линию

                            points[1] = targetNode.center;
                            targetNodeIndex = targetNode.index;

                            if (gameManager.CheckRepeatLine(parentNodeIndex, targetNodeIndex) != 1) //не повторяется ли линия
                            {
                                if (!targetNode.activate) //если к узлу еще не проводились линии
                                {
                                    targetNode.activate = true;
                                }
                                Debug.Log("StartCIC()");
                                StartCICAdditive(targetNode, col);
                                gameManager.CheckGloworms(1);
                            }
                            else
                            {
                                destroyLine = true;
                            }
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
                        LineBehaviour();

                        this.gameObject.layer = 0;

                        var lineData = new GameManager.LineData(this, parentNodeIndex, targetNodeIndex);
                        gameManager.lines.Add(lineData);

                        //if (parentTargetNode != null)
                            //parentTargetNode.numOfLines++;
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

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //if (collision.CompareTag("TargetNode"))
    //{
    //    targetNode = collision.GetComponent<TargetNodeScript>();
    //    if (parentTargetNode == targetNode)
    //    {
    //        targetNode = null;
    //    }
    //    else
    //    {
    //        activateAnswer = false;
    //        answerNode = null;
    //    }
    //}
    //if (collision.CompareTag("AnswerNode"))
    //{
    //    answerNode = collision.GetComponent<AnswerNodeScript>();
    //    activateAnswer = true;
    //    targetNode = null;
    //}
    //}

    private void OnMouseDown()
    {
        if (activateAnswer || (targetNode != null /*!targetNode.initializator*/))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;

            if (clicked > 1 && Time.time - clicktime < clickdelay)
            {
                clicked = 0;
                clicktime = 0;
                if (activateAnswer)
                {
                    answerNode.inColors.Remove(col);
                    answerNode.col = MixColors(answerNode.inColors);
                    answerNode.CheckAnswer();
                    activateAnswer = false;
                    answerNode.activate = false;
                }
                else
                {
                    StartCICRemovable(targetNode, col);
                    parentTargetNode.outLines.Remove(this);
                    parentTargetNode.RefreshOutLines();

                }
                StartDestroy();
                /*if (parentTargetNode != null)
                {
                    parentTargetNode.numOfLines--;
                    if (parentTargetNode.numOfLines == 0)
                        parentTargetNode.initializator = false;
                }*/

                /*if (activateAnswer)
                {
                    answerNode.inColors.Remove(col);
                    answerNode.col = MixColors(answerNode.inColors);
                    answerNode.CheckAnswer();
                    activateAnswer = false;
                    answerNode.activate = false;
                }
                else
                {
                    StartCIC(targetNode, col, false);
                    /* targetNode.col = MixColors(targetNode.inColors);
                    targetNode.col = NormilizeColor(targetNode.col);*/
                   
                /*}
                var lineData = new GameManager.LineData(this, parentNodeIndex, targetNodeIndex);
                gameManager.lines.Remove(lineData);
                gameManager.CheckGloworms(0);
                DestroyLine();*/


            }
            else if (Time.time - clicktime > 1) clicked = 0;
        }
    }

    private void LineBehaviour() //отвечает за прорисовку линии следом за мышкой
    {
        //this.transform.position = new Vector2((dot1.x + dot2.x) / 2, (dot1.y + dot2.y) / 2);
        lineRenderer.SetPosition(0, points[0] /*dot1*/);
        lineRenderer.SetPosition(1, points[1] /*dot2*/);

        #region Настраиваем партикл
        particleTransform.position = new Vector2((points[0].x + points[1].x) / 2, (points[0].y + points[1].y) / 2);
        _direction = (points[0] - (Vector2)particleTransform.transform.position).normalized;
        _lookRotation = Quaternion.LookRotation(_direction);
        tempAxis = _lookRotation.x;
        _lookRotation.x = 0f;
        _lookRotation.y = 0f;
        if (points[0].x < points[1].x)
        {
            _lookRotation.z = tempAxis;
        }
        else
        {
            _lookRotation.z = -tempAxis;
        }
        particleTransform.rotation = _lookRotation;
        var psShape = ps.shape;
        psShape.scale = new Vector3(Mathf.Sqrt(Mathf.Pow((points[0].x - points[1].x), 2) + Mathf.Pow((points[0].y - points[1].y), 2)) * 0.9f, 0f, 0f);
        #endregion
        // points.Add(dot1);
        // points.Add(dot2);
        edgeCol.points = points; //.ToArray();
        //points.Clear();
    }


    public void StartDestroy()
    {
        var lineData = new GameManager.LineData(this, parentNodeIndex, targetNodeIndex);
        gameManager.lines.Remove(lineData);
        gameManager.CheckGloworms(0);
        DestroyLine();
    }


    void DestroyLine()
    {
        /*if (parentTargetNode != null && parentTargetNode.numOfLines <= 0)
            parentTargetNode.initializator = false;
            */
        WhenDestroyed();
        gameObject.SetActive(false);
    }

    private void WhenDestroyed() //обнуляет переменные
    {
        //points = new List<Vector2>();
        points = new Vector2[2];
        targetNode = null;
        answerNode = null;
        parentTargetNode = null;
        activateAnswer = false;
        destroyLine = false;
        detectedNodes.Clear();
    }

    private TargetNodeScript CheckTargetNode(List<GameObject> nodes)
    {
        if (nodes.Count > 0)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint((new Vector2(Input.mousePosition.x, Input.mousePosition.y)));
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Vector2.Distance(mousePos, nodes[i].transform.position) < 0.6f)
                {
                    Debug.Log("Yeahh!It's Working!");
                    return nodes[i].GetComponent<TargetNodeScript>();
                }

            }
        }
        return null;
        
    }
}
