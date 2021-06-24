using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level"),]
public class LevelData : ScriptableObject
{
    [Range(2, 4)]
    public int width;
    [Range(1, 6)]
    public int height;
    public int[] data;

    public void GenerateLevel()
    {
        bool[] auxConnections;
        Node[,] aux = new Node[width, height];
        data = new int[width * height];

        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                aux[w, h] = new Node();
                auxConnections = new bool[4];

                // Width Restrictions
                if (w == 0)
                {
                    auxConnections[3] = false;
                }
                else
                {
                    auxConnections[3] = aux[w - 1, h].connections[1];
                }

                if (w == width - 1)
                {
                    auxConnections[1] = false;
                }
                else if (w == 0)
                {
                    auxConnections[1] = true;
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
                    auxConnections[2] = aux[w, h - 1].connections[0];
                }
                if (h == height - 1)
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

                aux[w, h].connections = auxConnections;
                data[w + h * width] = connectionCount;
            }
        }
    }

}
