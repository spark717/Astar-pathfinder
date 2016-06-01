using System.Collections;
using System.Collections.Generic;
using UnityEngine;





/// <summary> Алгоритм поиска А*</summary>
public class Astar
{
    public List<ObjectNode> open = new List<ObjectNode>();     //Список открытых нодов      !!!ПОМЕНЯТЬ НА ПРИВАТ ПОСЛЕ ДЕБАГА!!!
    public List<ObjectNode> close = new List<ObjectNode>();        //Список закрытых нодов      !!!ПОМЕНЯТЬ НА ПРИВАТ ПОСЛЕ ДЕБАГА!!!
    public List<ObjectNode> path = new List<ObjectNode>();
    private ObjectNode start, finish;



    public Astar()
    {
        
    }


    
    /// <summary> Запуск алгоритма </summary>
    public void FindPath(ObjectNode start, ObjectNode finish)
    {
        this.start = start;
        this.finish = finish;
        open.Clear();
        close.Clear();
        path.Clear();


        ObjectNode workNode;      //Обрабатываемый нод
        open.Add(start);         //Добавляем стартовый нод в открытый список
        start.G = 0;
        start.H = ManhattanH(start, finish);

        while (!open.Contains(finish))     //Повторять до тех пор, пока финишный нод не попадет в открытый список
        {
            workNode = SortingOpenBy("F");     //Выбираем нод из открытого списка с минимальным F
            open.Remove(workNode);       //Удаляем рабочий нод из открытого списка
            close.Add(workNode);         //Добавляем рабочий нод в закрытый список
            for (int i = 0; i < workNode.incidentNodes.Count; i++)      //Ходим по инцидентным нодам рабочего нода
            {
                ObjectNode target = workNode.incidentNodes[i];      //Нод, который будет добавлен в открытый список
                if (!close.Contains(target))
                {
                    float newG = workNode.G + Vector2.Distance(workNode.ToVector2(), target.ToVector2());

                    if (!open.Contains(target))     //Если нода нет в открытом списке, добавляем
                    {
                        target.Parent = workNode;     //Устанавливаем для нода G, H и родительский нод
                        target.G = newG;
                        target.H = ManhattanH(target, finish);
                        open.Add(target);
                    }
                    else if (open.Contains(target) && target.G > newG)      //Если нод есть в открытом списке, то сравниваем G
                    {
                        target.Parent = workNode;
                        target.G = newG;
                    }
                }
            }
       }
       
        path.Add(finish);     //Добавляем в наш путь финишный нод
        while (!path.Contains(start))        //Поочереди добавляем в путь родительский нод предыдущего нода, пока не добавим стартовый нод
        {
            path.Add(path[path.Count - 1].Parent);
        }

    }


    
    

    
    /// <summary> Метод вычисляет H методом Манхеттена </summary>
    public float ManhattanH (ObjectNode target, ObjectNode finish)
    {
        return (Mathf.Abs(finish.x - target.x) + Mathf.Abs(finish.y - target.y));
    }



    /// <summary> Метод сортирует открытый список по значению F или G, в порядке возрастания </summary>
    private ObjectNode SortingOpenBy(string key)      //Возвращает нод с минимальным F или G
    {
        int minPos = 0;

        for (int i = 1; i < open.Count; i++)
        {
            if (((key == "F") && (open[i].F < open[minPos].F)) ||
                ((key == "G") && (open[i].G < open[minPos].G)))
            {
                minPos = i;
            }
        }
        return open[minPos];
    }





//------------------------------------------DEBUG BLOCK-----------------------------------------------------
    

    void MYTEST3()
    {
        for (int i = 0; i < open.Count; i++)
        {
            Debug.Log("X: " + open[i].x + " Y: " + open[i].y);
        }
        Debug.Log("------------------------------------");
    }
}
//------------------------------------------------------------------------------------------------------------
