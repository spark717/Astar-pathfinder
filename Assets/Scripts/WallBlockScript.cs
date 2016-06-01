using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallBlockScript : MonoBehaviour {

    public List<ObjectNode> nodes = new List<ObjectNode>();
    public List<ObjectEdge> edges = new List<ObjectEdge>();

    void Awake ()
    {
        InitNodes();
        InitEdges();
    }



    /// <summary> Метод создает список нодов, принадлежащих объекту </summary>
    void InitNodes()
    {
        Transform nodeT;
        int i = 1;
        nodeT = transform.Find("node (1)");

        while (nodeT != null)
        {            
            nodes.Add(new ObjectNode(nodeT.position));
            nodes[i - 1].AttachedObject = gameObject;
            i++;
            nodeT = transform.Find("node (" + i + ")");           
        }


        nodes[0].incidentNodes.Add(nodes[nodes.Count - 1]);
        nodes[0].incidentNodes.Add(nodes[1]);
        nodes[nodes.Count - 1].incidentNodes.Add(nodes[nodes.Count - 2]);
        nodes[nodes.Count - 1].incidentNodes.Add(nodes[0]);

        for (int j = 1; j < nodes.Count - 1; j++)
        {
            nodes[j].incidentNodes.Add(nodes[j - 1]);
            nodes[j].incidentNodes.Add(nodes[j + 1]);
        }
    }



    /// <summary> Метод создает список ребер </summary>
    void InitEdges()
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            edges.Add(new ObjectEdge(nodes[i], nodes[i + 1]));
        }
        edges.Add(new ObjectEdge(nodes[nodes.Count - 1], nodes[0]));
    }






    void MYTEST ()
    {
        /*for (int i = 0; i < nodes.Count; i++)
        {
            Debug.Log("X: " + nodes[i].x + "Y: " + nodes[i].y);
        }*/
        for (int i = 0; i < edges.Count; i++)
        {
            Debug.Log("V1:(" + edges[i].Vertex1.x + ";" + edges[i].Vertex1.y + ") V2:(" + edges[i].Vertex2.x + ";" + edges[i].Vertex2.y + ")");
        }
    }
}
