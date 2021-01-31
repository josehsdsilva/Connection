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
    public NodeData nodeData = new NodeData();
    public int MaxConnections;
    public bool[] connections;
    bool random = false;

    public void ChildSetActive(int dir, bool active, bool _random)
    {
        random = _random;
        childs[dir].SetActive(active);
        if (!random) connections[dir] = active;
        else nodeData.connections[dir] = active;
        if (active) currentConnections++;
        else currentConnections--;
        UpdateTextMesh();
    }

    public void UpdateTextMesh()
    {
        if(!random)
        {
            if (MaxConnections == 0) textMesh.gameObject.SetActive(false);
            textMesh.text = (MaxConnections - currentConnections).ToString();
        }
        else
        {
            if (nodeData.MaxConnections == 0) textMesh.gameObject.SetActive(false);
            textMesh.text = (nodeData.MaxConnections - currentConnections).ToString();
        }
    }

    public void SetOnLevelFinished()
    {
        if(MaxConnections != 0)
        childs[4].GetComponent<MeshRenderer>().material = win;
        textMesh.gameObject.SetActive(false);
    }

}
