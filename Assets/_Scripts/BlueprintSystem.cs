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
    

 /*   protected void StartCICAdditive(Node target, Color remCol, Color newCol) //цепное изменение цвета при добавлении цвета
    {
        Color oldCol = target.col;

        Debug.Log("Mixing CIC");
        if (target.inColors.Contains(remCol))
        {
            target.inColors.Remove(remCol);
            if (newCol != Color.clear && newCol != target.originColor)
                target.inColors.Add(newCol);
            else
                Debug.Log("Clear color");
        }
        else
        {
            Debug.Log("Doesn't contain remCol");
        }
        target.col = NormilizeColor(MixColors(target.inColors));
        target.mat.SetColor("_TintColor", target.col);
        for (int i = 0; i < target.outLines.Count; i++)
        {
           // StartCICAdditive(target.outLines[i].targetNode, newCol);
            target.outLines[i].mat.SetColor("_TintColor", target.col); //у линии, которая исходит из узла, должен быть новый цвет узла
        }



    }*/

    protected void StartCICAdditive(Node target, Color newCol) //цепное изменение цвета (добавление цвета) для самого первого узла (только здесь цвет _добавляется_ (а не замещается) в список)
    {
        Color oldCol = target.col;
        target.inColors.Add(newCol);
        target.col = NormilizeColor(MixColors(target.inColors));
        target.mat.SetColor("_TintColor", target.col);
        CICRefreshColors(target, oldCol);
    }

    protected void StartCICRemovable(Node target, Color delCol) //цепное изменение цвета (удаление цвета) для самого первого узла: только здесь цвет _удаляется_ (а не замещается) из списка
    {
        Color oldCol = target.col;
        if (target.inColors.Contains(delCol))
        {
            target.inColors.Remove(delCol);
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
        }
        else
        {
            Debug.Log("DelCol not found. YOU DON'T SUPPOSE TO SEE THIS MESSAGE");
        }
        CICRefreshColors(target, oldCol); //запускаем обновление цвета в дочерних узлах
    }

    /*protected void StartCICRemovable(Node target, Color remCol, Color newCol) //цепное изменение цвета при удалении цвета
    {
        Color oldCol = target.col; //сохранить старый цвет

        if(target.inColors.Contains(remCol))
        {
            target.inColors.Remove(remCol);
            if (newCol != Color.clear && newCol != target.originColor)
                target.inColors.Add(newCol);
            else
                Debug.Log("Clear color");
            if (target.inColors.Count != 0)
            {
                target.col = NormilizeColor(MixColors(target.inColors));
                target.mat.SetColor("_TintColor", target.col);
            }
            else
            {
                target.mat.SetColor("_TintColor", target.originColor);
                target.activate = false;
            }
        }
        else
        {
            Debug.Log("StartCICRem no color");
        }
        for (int i = 0; i < target.outLines.Count; i++)
        {
            if (target.activate)
            {
                Debug.Log("target.active == true");
                target.outLines[i].mat.SetColor("_TintColor", target.col);
                StartCICRemovable(target.outLines[i].targetNode, oldCol, target.col);
            }
            else if (target.inColors.Count == 0)
            {
                Debug.Log("target.inColors.Count == 0");
                StartCICRemovable(target.outLines[i].targetNode, oldCol, Color.clear);
                target.outLines[i].StartDestroy();
            }
            else
            {
                Debug.Log("Target " + target.index + " count: " + target.inColors.Count);
            }
        }
        target.RefreshOutLines();
    }*/

    protected void CICRefreshColors(Node parent, Color removedCol) //заменяет цвет removedCol всех линий, исходящих из parent, и узлов, к которым они ведут, на цвет родительского узла (parent)
    {
        for (int i = 0; i < parent.outLines.Count; i++)
        {
            Node target = parent.outLines[i].targetNode;
            Color oldCol = target.col;
            if (target.inColors.Contains(removedCol)) //замещаем старый цвет на новый
            {
                target.inColors.Remove(removedCol);
                if (parent.activate) //если родительский узел еще активен, то добавляем его новый цвет. Если нет - просто удаляем
                    target.inColors.Add(parent.col);
                //установить новый цвет
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
            }
            else
            {
                Debug.Log("RefrColors: doesn't contein remColor. YOU DON'T SUPPOSE TO SEE THIS MESSAGE");
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

        parent.RefreshOutLines();
    }

    #region старый нерабочий код ЦИЦ
    /*
    protected void RefreshColors(Node target, Color oldCol) //проходит по цепочке от col и обновляет все цвета в соответствии с ней
    {
        for (int i = 0; i < target.outLines.Count; i++)
        {
            if (target.outLines[i].targetNode.inColors.Contains(oldCol))
            {
                target.outLines[i].mat.SetColor("_TintColor", target.col); //обновить цвет линии
                Color prevCol = target.outLines[i].targetNode.col; //сохранить старый цвет следующего узла
                target.outLines[i].targetNode.inColors.Remove(oldCol); //удалить старый цвет
                target.outLines[i].targetNode.inColors.Add(target.col); //добавить новый
                target.outLines[i].col = NormilizeColor(MixColors(target.outLines[i].targetNode.inColors));
                target.outLines[i].mat.SetColor("_TintColor", target.col);
                RefreshColors(target.outLines[i].targetNode, prevCol); //уходим в рекурсию
            }
            else
            {
                Debug.Log("oldCol not found");
            }
        }
    }*/

    /*  protected void StartCIC(Node target, Color newCol, bool flag) //цепное изменение цвета 
      {                                                             //target - изначально узел, к которому проводится новая линия; при спуске в рекурсию - узел, которому мы меняем цвет
          Debug.Log("Mixing CIC");
          if (flag)                                                 //если flag == true, то мы добавляем новый цвет, если false - отнимаем
              target.inColors.Add(newCol);
          else
              target.inColors.Remove(newCol);
          if (target.inColors.Count != 0)
          {
              target.col = NormilizeColor(MixColors(target.inColors, false));
              target.mat.SetColor("_TintColor", target.col);
          }
          else
          {
              target.mat.SetColor("_TintColor", target.originColor);
              target.activate = false;

          }
          for (int i = 0; i < target.outLines.Count; i++)
          {
              if (flag)
              {
                  StartCIC(target.outLines[i].targetNode, newCol, flag);
                  target.outLines[i].mat.SetColor("_TintColor", target.col); //у линии, которая исходит из узла, должен быть новый цвет узла
              }
              else
              {
                  StartCIC(target.outLines[i].targetNode, target.outLines[i].col, flag);
                  if (target.inColors.Count == 0)
                      target.outLines[i].StartDestroy();
                  else
                      target.outLines[i].mat.SetColor("_TintColor", target.col);
              }
          }
          target.RefreshOutLines();
      }*/
    #endregion
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
            Debug.Log("You are loh in infinity");
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
            Debug.Log("target == null, don't know how, probably no loop");
            return 0;
        }
        Debug.Log("Hello from CheckForLoops()! You are not supposed to see this message, something has gone wrong");
        return 3; //сюда по идее не должно доходить
    }
    private List<Node> InsertIntoList(List<Node> toCheck, Node target) //добавляет для функции CheckForLoops() узлы для проверки, с которыми соединен узел target
    {
        for (int i = 0; i < target.outLines.Count; i++)
        {
            toCheck.Add(target.outLines[i].targetNode);
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

