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
}
//Абстрактный класс узла, наследуем от фабрикм
    public abstract class Node : ShinyFactory
    {
        public List<Color> inColors = new List<Color>();  //Список всех цветов которые входят в узел
        public int numOfLines = 0;                        //Количество лмнмй которые выходят из узла
        public bool inTarget = false;                     //Аналог переменной из gameManager, показывает попадает ли сейчас линия на узел
        protected Renderer ren;                           //Рендер
        public ParticleSystem ps;                         //Наш партикл
        public GameObject linePrefab;                     //Префаб линии, чтоб создать её, в случае чего
        protected ShinyLineScript activeLine;             //Скрипт линии, которую узел только создал
        public int index;                                 //Индекс узла
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
}
    
    public abstract class Line : ShinyFactory
    {
        
        
    }

