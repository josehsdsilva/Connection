using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Puzzle
    {
        public int winValue;
        public int currentValue;

        public int width;
        public int height;
        public Node[,] level;
    }

    public Puzzle puzzle = new Puzzle();

    // Touch variables
    private Vector3 mouseStartPosition, mouseEndPosition;

    // Start is called before the first frame update
    void Start()
    {
        Vector2 dimensions = CheckDimensions();

        puzzle.width = (int)dimensions.x;
        puzzle.height = (int)dimensions.y;

        puzzle.level = new Node[puzzle.width, puzzle.height];

        foreach (var p in GameObject.FindGameObjectsWithTag("Node"))
        {
            puzzle.level[(int)p.transform.position.x, (int)p.transform.position.z] = p.GetComponent<Node>();
        }

        //foreach (var p in puzzle.level)
        //{
        //    Debug.Log(p.gameObject.name);
        //}

        puzzle.winValue = GetWinValue();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPosition = Input.mousePosition;
                mouseStartPosition = Camera.main.ScreenToWorldPoint(mouseStartPosition);
            }
        }
        if (Input.GetButtonUp("Fire1"))
        {
            if (Input.GetMouseButtonUp(0))
            {
                mouseEndPosition = Input.mousePosition;
                mouseEndPosition = Camera.main.ScreenToWorldPoint(mouseEndPosition);
                TryToLinkNodes();
            }
        }
    }

    Vector2 CheckDimensions()
    {
        Vector2 aux = Vector2.zero;
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Node");

        foreach (var p in pieces)
        {
            if (p.transform.position.x > aux.x) aux.x = p.transform.position.x;
            if (p.transform.position.z > aux.y) aux.y = p.transform.position.z;
        }
        aux += Vector2.one;
        return aux;
    }

    int GetWinValue()
    {
        int winValue = 0;
        foreach (var p in puzzle.level)
        {
            winValue += p.MaxConnections;
        }

        return winValue / 2;
    }

    public int Sweep()
    {
        int value = 0;

        for (int h = 0; h < puzzle.height; h++)
        {
            for (int w = 0; w < puzzle.width; w++)
            {
                // compare top
                if (h != puzzle.height - 1)
                    if (puzzle.level[w, h].connections[0] && puzzle.level[w, h + 1].connections[2])
                        value++;

                // compare right
                if (w != puzzle.width - 1)
                    if (puzzle.level[w, h].connections[1] && puzzle.level[w + 1, h].connections[3])
                        value++;

            }
        }

        return value;
    }

    void TryToLinkNodes()
    {

        // floats
        float xStart = mouseStartPosition.x;
        float zStart = mouseStartPosition.z;
        float xEnd = mouseEndPosition.x;
        float zEnd = mouseEndPosition.z;

        // int
        int startNodeX = (int)(xStart + 0.5f);
        int startNodeZ = (int)(zStart + 0.5f);
        int endNodeX = (int)(xEnd + 0.5f);
        int endNodeZ = (int)(zEnd + 0.5f);

        int dirX = endNodeX - startNodeX;
        int dirZ = endNodeZ - startNodeZ;

        int startDir = -1, endDir = -1;

        if(dirX > -2 && dirX < 2 && dirZ == 0 && startNodeX != endNodeX)
        {
            if (dirX == 1)
            {
                startDir = 1; endDir = 3;
            }
            else
            {
                startDir = 3; endDir = 1;
            }
        }
        else if (dirZ > -2 && dirZ < 2 && dirX == 0 && startNodeZ != endNodeZ)
        {
            if (dirZ == 1)
            {
                startDir = 0; endDir = 2;
            }
            else
            {
                startDir = 2; endDir = 0;
            }
        }

        if (startDir != -1)
        {
            if (startNodeX < puzzle.width && startNodeX >= 0 && startNodeZ < puzzle.height && startNodeZ >= 0 && endNodeX < puzzle.width && endNodeX >= 0 && endNodeZ < puzzle.height && endNodeZ >= 0)
            {

                if (puzzle.level[startNodeX, startNodeZ].connections[startDir] && puzzle.level[endNodeX, endNodeZ].connections[endDir])
                {
                    puzzle.level[startNodeX, startNodeZ].ChildSetActive(startDir, false);
                    puzzle.level[endNodeX, endNodeZ].ChildSetActive(endDir, false);
                }
                else
                {
                    if (puzzle.level[startNodeX, startNodeZ].currentConnections < puzzle.level[startNodeX, startNodeZ].MaxConnections &&
                    puzzle.level[endNodeX, endNodeZ].currentConnections < puzzle.level[endNodeX, endNodeZ].MaxConnections)
                    {
                        puzzle.level[startNodeX, startNodeZ].ChildSetActive(startDir, true);
                        puzzle.level[endNodeX, endNodeZ].ChildSetActive(endDir, true);
                    }
                }
            }
        }
    }
}
