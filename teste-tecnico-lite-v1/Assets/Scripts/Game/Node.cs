using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    // Set on Inspector
    public GameObject[] childs;
    public TextMesh textMesh;
    public Material win;

    public int currentConnections;
    public int MaxConnections;
    public bool[] connections;

    public void ChildSetActive(int dir, bool active)
    {
        childs[dir].SetActive(active);
        connections[dir] = active;
        if (active) currentConnections++;
        else currentConnections--;
        UpdateTextMesh();
    }

    public void UpdateTextMesh()
    {
        if (MaxConnections == 0) textMesh.gameObject.SetActive(false);
        textMesh.text = (MaxConnections - currentConnections).ToString();
    }

    public void SetOnLevelFinished()
    {
        if(MaxConnections != 0)
        childs[4].GetComponent<MeshRenderer>().material = win;
        textMesh.gameObject.SetActive(false);
    }

}
