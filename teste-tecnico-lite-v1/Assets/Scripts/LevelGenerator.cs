using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public LevelData level;

    public bool GenerateLevel;
    [Range(2, 4)]
    public int width;
    [Range(2, 6)]
    public int height;

    private void Start()
    {
        if (GenerateLevel)
        {
            level.width = width;
            level.height = height;

            bool[] auxConnections = new bool[4];

            NodeData aux;
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    aux = new NodeData();
                    level.data.Add(aux);
                    // Width Restrictions
                    if (w == 0)
                    {
                        auxConnections[3] = false;
                    }
                    else
                    {
                        auxConnections[3] = level.data[w - 1 + h * width].connections[1];
                    }

                    if (w == level.width - 1)
                    {
                        auxConnections[1] = false;
                    }
                    else
                    {
                        auxConnections[1] = Random.Range(0, 2) == 1;
                    }

                    // Height Restrictions
                    if (h == 0)
                    {
                        auxConnections[2] = false;
                    }
                    else
                    {
                        auxConnections[2] = level.data[w + (h - 1) * width].connections[0];
                    }
                    if (h == level.height - 1)
                    {
                        auxConnections[0] = false;
                    }
                    else
                    {
                        auxConnections[0] = Random.Range(0, 2) == 1;
                    }

                    int connectionCount = 0;
                    // Piece Type
                    for (int i = 0; i < 4; i++)
                    {
                        if (auxConnections[i])
                        {
                            connectionCount++;
                        }
                        
                    }
                    level.data[w + h * width].MaxConnections = connectionCount;
                    level.data[w + h * width].connections = auxConnections;
                    level.data[w + h * width].w = w;
                    level.data[w + h * width].h = h;
                }
            }
        }

    }
}
