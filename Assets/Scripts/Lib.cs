using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary> Интерфейс расширенного узла для A* </summary>
interface IAstarNode : INode
{
    ObjectNode Parent { get; set; }
    float F { get; }   
    float G { get; set; }        
    float H { get; set; }
}

/// <summary> Интерфейс приведения к пространсву Unity </summary>
interface IUnityTransform
{
    Vector2 ToVector2();
    Vector3 ToVector3();
}




//======================================NODE=============================================================


/// <summary> Интерфейс узла </summary>
interface INode { }

/// <summary> Класс, описывающий УЗЕЛ (вершину) графа в 2D декартовой системе координат. </summary>
public class Node2 : INode
{
    public float x, y;      //Координаты узла

    public Node2 () { }

    public Node2 (float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void Update(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}



/// <summary> Класс узла, принадлежащего игровому полю или объекту </summary>
public class ObjectNode :Node2, IAstarNode, IUnityTransform
{
    public GameObject AttachedObject { get; set; }      //Игровой объект, которому принадлежит нод
    public ObjectNode Parent { get; set; }    //Родительский узел
    public float F { get { return G + H; } }    //Стоимость узла    F = G + H
    public float G { get; set; }        //Стоимость передвижения из стартового узла
    public float H { get; set; }        //Эвристическая стоимость передвижения в конечный узел
    public List<ObjectNode> incidentNodes = new List<ObjectNode>();


    public ObjectNode () { }

    public ObjectNode(float x, float y) : base(x, y) { }

    public ObjectNode(Vector2 vector2)
    {
        this.x = vector2.x;
        this.y = vector2.y;
    }

    public ObjectNode(Vector3 vector3)
    {
        this.x = vector3.x;
        this.y = vector3.z;
    }

    public void Update(Vector3 vector3)
    {
        this.x = vector3.x;
        this.y = vector3.z;
    }



    public Vector2 ToVector2()
    {
        return (new Vector2(x, y));
    }

    public Vector3 ToVector3()
    {
        return (new Vector3(x, 0, y));
    }
}




//====================================================EDGE============================================================


/// <summary> Интерфейс ребра </summary>
interface IEdge
{
    Node2 Vertex1 { get; }
    Node2 Vertex2 { get; }
}

/// <summary> Класс, описывающий РЕБРО графа в 2D декартовой системе координат. </summary>
public class Edge2 : IEdge
{
    protected Node2 vertex1, vertex2;
    public virtual Node2 Vertex1 { get { return vertex1; } set { vertex1 = value; } }       //Первая вершина ребра
    public virtual Node2 Vertex2 { get { return vertex2; } set { vertex2 = value; } }       //Вторая вершина ребра



    public Edge2 () { }

    public Edge2 (Node2 vertex1, Node2 vertex2)     //Инициализация по инцидентным узлам
    {
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
    }
}



/// <summary> Класс ребра, принадлежащего игровому полю или объекту </summary>
public class ObjectEdge : Edge2, IUnityTransform    
{
    public GameObject AttachedObject { get; set; }      //Игровой объект, которому принадлежит ребро
    private Straight2 straight = new Straight2();
    public Straight2 Straight { get { return straight; } }       //урвнение прямой, соответствующее вектору ребра
    public float Length { get { return ToVector2().magnitude; } }       //Длина ребра   

    protected new ObjectNode vertex1, vertex2;
    public new ObjectNode Vertex1       //Переопределение обновляет уравнение прямой
    {
        get
        {
            return vertex1;
        }

        set
        {
            vertex1 = value;
            straight.Update(vertex1, vertex2);
        }
    }       

    public new ObjectNode Vertex2       //Переопределение обновляет уравнение прямой
    {
        get
        {
            return vertex2;
        }

        set
        {
            vertex2 = value;
            straight.Update(vertex1, vertex2);
        }
    }       



   // public ObjectEdge () { }

    public ObjectEdge(ObjectNode vertex1, ObjectNode vertex2)      //Инициализация по инцидентным узлам
    {
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        straight.Update(vertex1, vertex2);

        if (vertex1.AttachedObject == vertex2.AttachedObject)
        {
            AttachedObject = vertex1.AttachedObject;
        }
        else
        {
            AttachedObject = null;
        }
    } 



    public Vector2 ToVector2 ()     //конвертация в тип Vector2
    {
        float newX, newY;

        newX = Vertex2.x - Vertex1.x;
        newY = Vertex2.y - Vertex1.y;
        return new Vector2(newX, newY);
    }

    public Vector3 ToVector3 ()     //конвертация в тип Vector3
    {
        Vector2 vector2 = ToVector2();
        return new Vector3(vector2.x, 0, vector2.y);
    }



    /// <summary> Этот метод определяет, пересекаются 2 ребра или нет. </summary>
    public static bool Cross (ObjectEdge vector1, ObjectEdge vector2)
    {
        bool result = false;
        ObjectEdge v1 = vector1;
        ObjectEdge v2 = vector2;
        Vector2 point = Straight2.Intersection(v1.Straight, v2.Straight);
        if (point.x != Mathf.Infinity || point.y != Mathf.Infinity)
        {
            if (MyMath.InRange(v1.Vertex1.x, v1.Vertex2.x, point.x) &&
                MyMath.InRange(v1.Vertex1.y, v1.Vertex2.y, point.y) &&
                MyMath.InRange(v2.Vertex1.x, v2.Vertex2.x, point.x) &&
                MyMath.InRange(v2.Vertex1.y, v2.Vertex2.y, point.y))
            {
                result = true;
            }
        }      
        return result;
    }



    /// <summary> Этот метод определяет, совпадают два ребра или нет. </summary>
    public static bool Equals(ObjectEdge edge1, ObjectEdge edge2)
    {
        ObjectEdge e1 = edge1;
        ObjectEdge e2 = edge2;
        bool result = false;

        if ((e1.Vertex1 == e2.Vertex1 && e1.Vertex2 == e2.Vertex2) || (e1.Vertex1 == e2.Vertex2 && e1.Vertex2 == e2.Vertex1))
        {
            result = true;
        }
        return result;
    }
}




//================================================STRAIGHT=====================================================================


/// <summary> Интерфейс прямой линии </summary>
interface IStraight
{
    float A { get; }
    float B { get; }
    float C { get; }
}

/// <summary> Класс, описывающий ПРЯМУЮ ЛИНИЮ в 2D декартовой системе координат. </summary>
public class Straight2 : IStraight
{
    public float A { get; private set; }
    public float B { get; private set; }
    public float C { get; private set; }      //коэффициенты уравнения прямой Ax + Bx + C = 0

    public Straight2() { }

    public Straight2(float x1, float y1, float x2, float y2)        //инициализация по координатам 2-х точек
    {
        CoefficientInit(x1, y1, x2, y2);
    }

    public Straight2(Node2 vertex1, Node2 vertex2)     //инициализация по узлам
    {
        CoefficientInit(vertex1.x, vertex1.y, vertex2.x, vertex2.y);
    }



    /// <summary> Пересчет коэффициентов по новым узлам </summary>
    public void Update (Node2 vertex1, Node2 vertex2)      
    {
        CoefficientInit(vertex1.x, vertex1.y, vertex2.x, vertex2.y);
    }



    private void CoefficientInit(float x1, float y1, float x2, float y2)
    {
            A = y2 - y1;
            B = x1 - x2;
            C = y1 * x2 - y2 * x1;
    }



    /// <summary> Этот метод находит точку ПЕРЕСЕЧЕНИЯ прямых. Если прямые коллинеарны, возвращает (infinity, infinity) </summary>
    public static Vector2 Intersection(Straight2 line1, Straight2 line2)
    
    {
        float A1 = line1.A;
        float B1 = line1.B;
        float C1 = line1.C;
        float A2 = line2.A;
        float B2 = line2.B;
        float C2 = line2.C;
       // Debug.Log("A1: " + A1 + " B1: " + B1);
       // Debug.Log("A2: " + A2 + " B2: " + B2);
        Vector2 point = new Vector2();

        if (B1 * A2 != B2 * A1)
        {   
            point.x = (C2 * B1 - C1 * B2) / (A1 * B2 - A2 * B1);
            point.y = (C2 * A1 - C1 * A2) / (B1 * A2 - B2 * A1);
        }
        else
        {
            point.x = Mathf.Infinity;
            point.y = Mathf.Infinity;
        }       
        return point;
    }
}




//=====================================MyMATH=====================================================


/// <summary> Библиотека различных функций </summary>
public class MyMath 
{
    public MyMath() { }

    /// <summary> Этот метод проверяет, входит ли число в область (a, b) </summary>
    public static bool InRange(float a, float b, float number)
    {
        bool result = false;
        float n = number;

        if (((b > a) && (n > a && n < b)) || ((b < a) && (n < a && n > b)) || (a == b && a == n && b == n))
        {
            result = true;
        }
        return result;               
    }
}

