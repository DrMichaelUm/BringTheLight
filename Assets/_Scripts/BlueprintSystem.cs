using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using System.IO;
using TMPro;


public class Configs : MonoBehaviour
{   
    //Стартовые конфиги
    protected string levelConfigText = @"[
                                            {'level':'level_1', 'gloworms':0},  
                                            {'level':'level_2','gloworms':0},
                                            {'level':'level_3','gloworms':0},
                                            {'level':'level_4','gloworms':0},
                                            {'level':'level_5','gloworms':0},
                                            {'level':'level_6','gloworms':0},
                                            {'level':'level_7','gloworms':0},
                                            {'level':'level_8','gloworms':0},
                                            {'level':'level_9','gloworms':0},
                                            {'level':'level_10','gloworms':0},
                                            {'level':'level_11','gloworms':0},
                                            {'level':'level_12','gloworms':0},
                                            {'level':'level_13','gloworms':0},
                                            {'level':'level_14','gloworms':0},
                                            {'level':'level_15','gloworms':0},
                                            {'level':'level_16','gloworms':0},
                                            {'level':'level_17','gloworms':0},
                                            {'level':'level_18','gloworms':0},
                                            {'level':'level_19','gloworms':0},
                                            {'level':'level_20','gloworms':0}
                                         ]";

    protected string bottleConfigText = @"[
                                            {'bottleNumber':1,'gloworms':0,'isFull':false},
                                            {'bottleNumber':2,'gloworms':0,'isFull':false},
                                            {'bottleNumber':3,'gloworms':0,'isFull':false},
                                          ]";
    //Словари для записи и оперирования конфигами
    protected Dictionary<string, LevelConfig> levels;   
    protected Dictionary<int, BottleConfig> bottles;

    protected void FindLevelConfig()
    {
        string path = Application.persistentDataPath;
        if (!File.Exists(path + @"\level_config.json"))
        {
            File.WriteAllText(path + @"\level_config.json", levelConfigText);
        }
        var levelConfig = File.ReadAllText(path + @"\level_config.json");

        foreach (var level in JsonConvert.DeserializeObject<LevelConfig[]>(levelConfig))
        {
            levels.Add(level.level, level);
        }
    }

    protected void FindBottleConfig()
    {
        string path = Application.persistentDataPath;
        if (!File.Exists(path + @"\bottle_config.json"))
        {
            File.WriteAllText(path + @"\bottle_config.json", bottleConfigText);
        }
        var bottleConfig = File.ReadAllText(path + @"\bottle_config.json");

        foreach (var bottle in JsonConvert.DeserializeObject<BottleConfig[]>(bottleConfig))
        {
            bottles.Add(bottle.bottleNumber, bottle);
        }
    }
}


public class Managment : Configs
{
    //Функция отключения имейджей светляков. Надо передать список объектов светляков(с УИ) и кол-во которое надо отключить
    public void DisableGloworms(List<GameObject> gloworms, int disableNum)
    {
        foreach (GameObject gloworm in gloworms)
        {
            gloworm.SetActive(true);
        }
        for (int i = 2; i >= 3 - disableNum; i--)
        {
            gloworms[i].SetActive(false);
        }
    }

}

//Фабрика всего что на поле
public class ShinyFactory : MonoBehaviour 
{
    public Color col;           //Текущий цвет(линии или любого узла)
    public Color originColor;   //Изначальный цвет узла
    public Material mat;        //Материал партикла
    public Vector2 center;      //Центр объекта(линии или узла)
    public bool startLine = false; //Булевая переменная, означает что сейчас(или буквально вот фрейм назад) линия ведётся мышкой
    //protected Color clearColor = new Color(float.NaN, float.NaN, float.NaN, float.NaN);

    protected static Color MixColors(List<Color> inColors)
    {
        Color resultColor = new Color(0, 0, 0, 0);
        foreach (Color c in inColors)
        {
            resultColor += c;
        }
        resultColor /= inColors.Count;
        return resultColor;
    } //Смешиваем цвет

    protected Color NormilizeColor(Color color)
    {
        Color.RGBToHSV(color, out float H, out float S, out float V);
        S = 0.8f;
        color = Color.HSVToRGB(H, S, V);
        color.a = 0.5f;
        return color;
    }            //Приводим цвет к единому стандарту
    
    protected void StartCIC(Node target, Color color, bool additive) //цепное изменение цвета для самого первого узла: только здесь цвет _добавляется_ (если additive == true) или _удаляется_ (если additive =- false) (а не замещается) из списка
    {
        Color oldCol = target.col;
        if (additive)
        {
            target.inColors.Add(color);
        }
        else
        {
            if(target.inColors.Contains(color))
                target.inColors.Remove(color);
            else
                Debug.Log("No color in list. You aren't supposed to see this message, something has gone wrong");
        }
        if (target.inColors.Count != 0) //если после этого у нас остались цвета, то смешиваем их
        {
            target.col = NormilizeColor(MixColors(target.inColors));
            target.mat.SetColor("_TintColor", target.col);
        }
        else //если нет - выключаем узел
        {
            target.mat.SetColor("_TintColor", target.originColor);
            target.activate = false;
        }
        CICRefreshColors(target, oldCol); //запускаем обновление цвета в дочерних узлах
    }

    private void CICRefreshColors(Node parent, Color removedCol) //заменяет цвет removedCol всех линий, исходящих из parent, и узлов, к которым они ведут, на цвет родительского узла (parent)
    {
        for (int i = 0; i < parent.outLines.Count; i++) //проходимся по всем исходящим из родителя линиям
        {
            bool isAnswer = false; //индикатор того, работаем мы с обычным узлом или узлом ответа
            Node target = parent.outLines[i].targetNode;
            if (target == null) //если нет обычного, то будет узел ответа
            {
                target = parent.outLines[i].answerNode;
                isAnswer = true;
            }
            Color oldCol = target.col; //сохранить старый цвет
            if (target.inColors.Contains(removedCol)) //замещаем старый цвет на новый
            {
                target.inColors.Remove(removedCol);
                if (parent.activate) //если родительский узел еще активен, то добавляем его новый цвет. Если нет - просто удаляем
                    target.inColors.Add(parent.col);

                if (target.inColors.Count != 0) //если после этого у нас остались цвета, то смешиваем их
                {
                    target.col = NormilizeColor(MixColors(target.inColors));
                    if (!isAnswer) //если узел не является узлом ответа, то обновляем его цвет
                        target.mat.SetColor("_TintColor", target.col);
                    else //если это узел отета, то делаем проверку
                        target.GetComponent<AnswerNodeScript>().CheckAnswer();
                }
                else //если цветов не осталось - выключаем узел
                {
                    target.mat.SetColor("_TintColor", target.originColor);
                    target.activate = false;
                    if (isAnswer)
                    {
                        target.GetComponent<AnswerNodeScript>().CheckAnswer();
                        parent.outLines[i].activateAnswer = false;
                    }
                }
            }
            else
            {
                Debug.Log("RefreshColors(): doesn't contein remColor. You aren't supposed to see this message, something has gone wrong");
            }
            if (parent.activate) //если родительский узел еще активен, то обновляем цвет исходящих линий
            {
                parent.outLines[i].col = parent.col; //изменить цвет исходящей линии
                parent.outLines[i].mat.SetColor("_TintColor", parent.outLines[i].col); //изменить цвет исходящей линии
            }
            else //если нет - уничтожаем их
            {
                parent.outLines[i].StartDestroy();
            }
            CICRefreshColors(target, oldCol); //аналогично меняем цвет в дочерних узлах target
        }
        parent.RefreshOutLines(); //обновляем список исходящих линий - некоторых может уже не быть
    }

    protected int CheckForLoops(Node origin, Node target) //проверяет, не попадем ли мы в замкнутый круг
    {                                                     //возвращает: 0 - нет кругов, 1 - есть круг, 2 и 3 - ошибка
        if (origin == null)
        { 
            Debug.Log("no origin");
            return 2;
        }
        int depth = 0, maxDepth = 30; //чтобы остановить цикл, если он зайдет слишком далеко
        List<Node> toCheck = new List<Node>();
        toCheck = InsertIntoList(toCheck, target);
        while (depth < maxDepth && toCheck != null && toCheck.Count != 0 && target != null && target.index != origin.index)
        {
            //Debug.Log("org: " + origin.index + " targ: " + target.index);
            //Debug.Log("Checking loops...");
            depth = depth + 1;
            target = toCheck[0];
            toCheck.RemoveAt(0);
            if (target != null)
                toCheck = InsertIntoList(toCheck, target);
        }
        if (depth >= maxDepth)
        {
            Debug.Log("You are loh in infinity. You aren't supposed to be a loh, something has gone wrong");
            return 1;
        }
        else if (target == origin)
        {
            Debug.Log("Loop");
            return 1;
        }
        else if (toCheck.Count == 0)
        {
            Debug.Log("All checked, no loop");
            return 0;
        }
        else if (target == null)
        {
            Debug.Log("target == null. You aren't supposed to see this message, something has gone wrong");
            return 3;
        }
        Debug.Log("Hello from CheckForLoops()! You aren't supposed to see this message, something has gone wrong");
        return 3;
    }

    private List<Node> InsertIntoList(List<Node> toCheck, Node target) //добавляет для функции CheckForLoops() узлы для проверки, с которыми соединен узел target
    {
        for (int i = 0; i < target.outLines.Count; i++)
        {
            Node toAdd = target.outLines[i].targetNode;
            if (toAdd == null) //если у нас не было target-узла, значит должен быть answer-узел
            {
                toAdd = target.outLines[i].answerNode;
            }
            toCheck.Add(toAdd);
        }
        return toCheck;
    }

}

//Абстрактный класс узла, наследуем от фабрикм
    public abstract class Node : ShinyFactory
    {
        public List<Color> inColors = new List<Color>();  //Список всех цветов которые входят в узел
        public List<ShinyLineScript> outLines = new List<ShinyLineScript>(); //Список всех линий, которые выходят из узла
        public int numOfLines = 0;                        //Количество лмнмй которые выходят из узла
        public bool inTarget = false;                     //Аналог переменной из gameManager, показывает попадает ли сейчас линия на узел
        protected Renderer ren;                           //Рендер
        public ParticleSystem ps;                         //Наш партикл
        public GameObject linePrefab;                     //Префаб линии, чтоб создать её, в случае чего
        protected ShinyLineScript activeLine;             //Скрипт линии, которую узел только создал
        public int index;                                 //Индекс узла
        public bool activate = false;
    protected void OnMouseEnterFunction()                 //Функция, выполняемая при пресесении линии и узла
    {
            inTarget = true;
            Debug.Log("InTarget!");
            GameManager.Instance.inTarget = inTarget;     //обязательно говорим менеджеру что линия попала на узел
            GameManager.Instance.endPoint = center;       //Передаём в менеджер конечную точку для линии
    }
    protected void OnMouseExitFunction()                  //Функция, выполняемая при выходе линии с узла
    {
            inTarget = false;
            Debug.Log("OutOfTarget!");
            GameManager.Instance.inTarget = inTarget;
    }

    protected void InitFunction()                         //Функция, которую вызывает узел в Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            mat = ps.GetComponent<Renderer>().material;
        col = mat.GetColor("_TintColor");
        originColor = col;
        //inColors.Add(col);
        //numCol++;
        center = new Vector2(transform.position.x, transform.position.y);
    }

    protected void InitLine(Transform parent)            //Инициализируем линию
    {
        GameObject Line = ObjectPoolingManager.Instance.GetShinyLine(parent);
        activeLine = Line.GetComponent<ShinyLineScript>();
        col = NormilizeColor(col);
        activeLine.col = col;
        Debug.Log("Color is set");
        activeLine.center = center;
        activeLine.startLine = true;
        activeLine.Restart();
    }

    public void RefreshOutLines()
    {
        List<ShinyLineScript> toDelete = new List<ShinyLineScript>();
        for (int i = 0; i < outLines.Count; i++)
        {
            if (!outLines[i].gameObject.activeInHierarchy)
                toDelete.Add(outLines[i]);
        }
        for (int i = 0; i < toDelete.Count; i++)
            outLines.Remove(toDelete[i]);
    }
}
    
    public abstract class Line : ShinyFactory
    {
        
        
    }

