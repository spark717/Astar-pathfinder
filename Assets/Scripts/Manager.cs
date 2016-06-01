using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Manager : MonoBehaviour {

    public Transform start, finish;
    public LineRenderer visiblePath;
    private ObjectNode startNode, finishNode;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);   
    private WallBlockScript wallScript;
    

    private List<ObjectNode> nodes = new List<ObjectNode>();        //Список узлов, принадлежащих игровым объектам
    private List<ObjectEdge> edges = new List<ObjectEdge>();        //Список ребер, принадлежащих игровым объектам
    private List<ObjectEdge> web = new List<ObjectEdge>();      //Список ребер, соединяющих разные статические объекты между собой
    private List<ObjectEdge> startNodeWeb = new List<ObjectEdge>();
    private List<ObjectEdge> finishNodeWeb = new List<ObjectEdge>();
    private Astar astar1 = new Astar();


    void Awake ()
    {
        startNode = new ObjectNode(start.position);
        finishNode = new ObjectNode(finish.position);

        ShowExecTime(ConstructStaticNodesAndEdges, "Время построения нодов: ");
        ShowExecTime(CreateWeb, "Время создания паутины: ");
    }


    void Update ()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SetPositionByMouse(finish);
            finishNode.Update(finish.position);
            ShowExecTime(SetIncNodes, finishNode, "Время установки finishNode: ");
            if (startNode.incidentNodes.Count != 0)
            {
                astar1.FindPath(startNode, finishNode);
                CreateVisiblePath();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            SetPositionByMouse(start);
            startNode.Update(start.position);
            ShowExecTime(SetIncNodes, startNode, "Время установки startNode: ");
            if (finishNode.incidentNodes.Count != 0)
            {
                astar1.FindPath(startNode, finishNode);
                CreateVisiblePath();
            }
        }
    }



    void SetPositionByMouse (Transform target)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            target.position = ray.GetPoint(distance);
        }
    }



    public void FindPathButtonClick ()
    {
              
    }



    void SetIncNodes(ObjectNode target)
    {
        target.incidentNodes.Clear();       
        foreach (ObjectNode node in nodes)
        {
            node.incidentNodes.Remove(target);
            bool cross = false;
            foreach (ObjectEdge edge in edges)
            {           
                if (ObjectEdge.Cross(new ObjectEdge(target, node), edge))
                {
                    cross = true;
                } 
            }   
            
            if (!cross)
            {
                target.incidentNodes.Add(node);
                node.incidentNodes.Add(target);
            }                   
        }
    }



    public void PlaceConstructionButtonClick ()
    {

    }



    /// <summary>
    /// Наполняем списки nodes и edges статичными нодами и ребрами
    /// </summary>
    private void ConstructStaticNodesAndEdges()
    {
        int i = 1;
        GameObject wallObject = GameObject.Find("Object (" + i + ")");
        while (wallObject != null)
        {
            wallScript = wallObject.GetComponent<WallBlockScript>();
            nodes.AddRange(wallScript.nodes);
            edges.AddRange(wallScript.edges);
            i++;
            wallObject = GameObject.Find("Object (" + i + ")");
        }
    }



    /// <summary> Создание системы графов (паутины) </summary>
    private void CreateWeb()
    {
        web.Clear();
        foreach (ObjectNode node1 in nodes)
        {
            foreach (ObjectNode node2 in nodes)
            {
                if (node1.AttachedObject != node2.AttachedObject && !node1.incidentNodes.Contains(node2))
                {
                    ObjectEdge edge12 = new ObjectEdge(node1, node2);
                    bool cross = false;

                    foreach (ObjectEdge edge in edges)
                    {
                        if (ObjectEdge.Cross(edge12, edge))
                        {
                            cross = true; break;
                        }
                    }

                    if (!cross)
                    {
                        web.Add(edge12);
                        node1.incidentNodes.Add(node2);
                        node2.incidentNodes.Add(node1);
                    }
                }
            }           
        }

    }

    private void CreateVisiblePath()
    {
        var path = astar1.path;
        visiblePath.SetVertexCount(path.Count);
        for (int i = 0; i <= path.Count - 1; i++)
        {          
          //visiblePath.SetPosition(i, new Vector3(0,0,0));
            visiblePath.SetPosition(i, path[i].ToVector3());
        }
    }



    private void AddStartAndFinish ()
    {
        nodes.Add(new ObjectNode(start.position));
        nodes.Add(new ObjectNode(finish.position));
    }



    //===============================DEBUG================================================

    void MYTEST1 ()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
           UnityEngine.Debug.Log("X: " + nodes[i].x + " Y: " + nodes[i].y);
        }
    }

    void MYTEST2 ()
    {
        for (int i = 0; i < edges.Count; i++)
        {
            UnityEngine.Debug.Log("V1:(" + edges[i].Vertex1.x + ";" + edges[i].Vertex1.y + ") V2:(" + edges[i].Vertex2.x + ";" + edges[i].Vertex2.y + ")");
        }
    }

    void ShowExecTime(Action func, string message)
    {
        Stopwatch watch = Stopwatch.StartNew();

        func();

        watch.Stop();
        long delta = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log(message + delta + " мс");
    }

    void ShowExecTime(Action<ObjectNode> func, ObjectNode param, string message)
    {
        Stopwatch watch = Stopwatch.StartNew();

        func(param);

        watch.Stop();
        long delta = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log(message + delta + " мс");
    }








    //-----------------------------------GIZMOS------------------------------------------
    void OnDrawGizmos()
    {
        //---------------Ребра статических объектов-------------------------------
        Gizmos.color = Color.black;
        for (int i = 0; i < edges.Count; i++)
        {
            Gizmos.DrawLine(edges[i].Vertex1.ToVector3(), edges[i].Vertex2.ToVector3());
        }

        //---------------Паутина----------------------------------------
        Gizmos.color = Color.white;
        for (int i = 0; i < web.Count; i++)
        {
            //Gizmos.DrawLine(web[i].Vertex1.ToVector3(), web[i].Vertex2.ToVector3());
        }

        //-------------startNode--------------------------------------
        if (finishNode != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < startNode.incidentNodes.Count; i++)
            {
                //Gizmos.DrawLine(startNode.incidentNodes[i].ToVector3(), startNode.ToVector3());
            }
        }

        //-------------finishNode--------------------------------------
        if (finishNode != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < finishNode.incidentNodes.Count; i++)
            {
               // Gizmos.DrawLine(finishNode.incidentNodes[i].ToVector3(), finishNode.ToVector3());
            }
        }


        //--------------Открытый и закрытый список----------------------------
        if (astar1 != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < astar1.open.Count; i++)
            {
               Gizmos.DrawSphere(astar1.open[i].ToVector3(), 1);
            }
            Gizmos.color = Color.red;
            for (int j = 0; j < astar1.close.Count; j++)
            {
               Gizmos.DrawSphere(astar1.close[j].ToVector3(), 1);
            }


            //---------------------------Путь------------------------------------
            Gizmos.color = Color.blue;
            if (astar1.path.Count > 1)
            {
                for (int i = 0; i < astar1.path.Count - 1; i++)
                {
                    Gizmos.DrawLine(astar1.path[i].ToVector3(), astar1.path[i + 1].ToVector3());
                }
            }
        }
    }
}