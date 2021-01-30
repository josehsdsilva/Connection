using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int MaxConnections;
    public int currentConnections;
    public bool[] connections;
    public GameObject[] childs;

    public void ChildSetActive(int dir, bool active)
    {
        childs[dir].SetActive(active);
        connections[dir] = active;
        if (active) currentConnections++;
        else currentConnections--;
    }
}
