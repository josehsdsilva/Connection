using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level"),]
public class LevelData : ScriptableObject
{
    public int width = 0, height = 0;
    public List<NodeData> data = new List<NodeData>();
}
