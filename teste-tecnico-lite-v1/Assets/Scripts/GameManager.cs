using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Gamestates
    enum Gamestate
    {
        gameStart = 0,
        playable,
        levelComplete,
        nextLevel
    }
    #endregion

    Gamestate gamestate;

    [System.Serializable]
    public class Puzzle
    {
        public int winValue;
        public int currentValue;

        public int width;
        public int height;
        public Node[,] level;
    }

    // Objects to set in inspector
    public GameObject[] NodePrefabs;
    public LevelData[] levels;
    public Text scoreText;

    int currentLevel = 0;
    int winValue;
    int currentValue;
    int score;

    bool Random;

    float counter;

    List<List<GameObject>> level = new List<List<GameObject>>();

    // Touch variables
    private Vector3 mouseStartPosition, mouseEndPosition;

    public Puzzle puzzle = new Puzzle();

    // Start is called before the first frame update
    void Start()
    {
        gamestate = Gamestate.gameStart;
        score = 0;

        if (Random)
        {
            GetLevelInfo();

            int winAux = 0;
            for (int h = 0; h < levels[currentLevel].height; h++)
            {
                List<GameObject> aux = new List<GameObject>();
                for (int w = 0; w < levels[currentLevel].width; w++)
                {
                    GameObject go = new GameObject();
                    aux.Add(go);
                }
                level.Add(aux);
            }
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Node"))
            {
                winAux += p.GetComponent<Node>().nodeData.MaxConnections;

                level[p.GetComponent<Node>().nodeData.w][p.GetComponent<Node>().nodeData.h] = p;
            }

            // Calculate Win Value
            winValue = winAux;
        }
        else
        {
            Vector2 dimensions = CheckDimensions();

            puzzle.width = (int)dimensions.x;
            puzzle.height = (int)dimensions.y;

            puzzle.level = new Node[puzzle.width, puzzle.height];

            foreach (var p in GameObject.FindGameObjectsWithTag("Node"))
            {
                puzzle.level[(int)p.transform.position.x, (int)p.transform.position.z] = p.GetComponent<Node>();
                puzzle.level[(int)p.transform.position.x, (int)p.transform.position.z].UpdateTextMesh();
            }

            //foreach (var p in puzzle.level)
            //{
            //    Debug.Log(p.gameObject.name);
            //}

            puzzle.winValue = GetWinValue();
        }

        gamestate = Gamestate.playable;
    }

    // Update is called once per frame
    void Update()
    {
        if (gamestate == Gamestate.playable)
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
                    // Make a play if possible
                    if (Random)
                    {
                        TryToLinkRandomGeneratedNodes();
                    }
                    else
                    {
                        TryToLinkNodes();
                    }
                }
            }
        }

        if(gamestate == Gamestate.levelComplete)
        {
            counter += Time.deltaTime;
            if(counter > 3)
            {
                counter = 0;
                NextLevel();
            }
        }
    }

    // Get Level Data

    void GetLevelInfo()
    {
        // Retrieve level data from ScriptableObject and Instantiate level objects
        GameObject go;
        for (int h = 0; h < levels[currentLevel].height; h++)
        {
            for (int w = 0; w < levels[currentLevel].width; w++)
            {
                //Debug.Log(levels[currentLevel].data[w + h * puzzle.width].MaxConnections);
                if(levels[currentLevel].data[w + h * levels[currentLevel].width].MaxConnections == 0)
                {
                    go = Instantiate(NodePrefabs[0], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                else if (levels[currentLevel].data[w + h * levels[currentLevel].width].MaxConnections == 1)
                {
                     go = Instantiate(NodePrefabs[1], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                else
                {
                    go = Instantiate(NodePrefabs[2], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                go.GetComponent<Node>().nodeData.MaxConnections = levels[currentLevel].data[w + h * levels[currentLevel].width].MaxConnections;
                go.GetComponent<Node>().UpdateTextMesh();
            }
        }
    }

    #region Play

    // Make a play if possible
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

        if (dirX > -2 && dirX < 2 && dirZ == 0 && startNodeX != endNodeX)
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
                // Calculate new currentValue
                int differenceStart = -QuickSweep(startNodeX, startNodeZ);
                int differenceEnd = -QuickSweep(endNodeX, endNodeZ);
                if (puzzle.level[startNodeX, startNodeZ].connections[startDir] && puzzle.level[endNodeX, endNodeZ].connections[endDir])
                {
                    // Update Start Node
                    puzzle.level[startNodeX, startNodeZ].ChildSetActive(startDir, false, false);

                    // Update End Node
                    puzzle.level[endNodeX, endNodeZ].ChildSetActive(endDir, false, false);

                    score -= 15;
                }
                else
                {
                    if (puzzle.level[startNodeX, startNodeZ].currentConnections < puzzle.level[startNodeX, startNodeZ].MaxConnections &&
                    puzzle.level[endNodeX, endNodeZ].currentConnections < puzzle.level[endNodeX, endNodeZ].MaxConnections)
                    {
                        // Update Start Node
                        puzzle.level[startNodeX, startNodeZ].ChildSetActive(startDir, true, false);

                        // Update End Node
                        puzzle.level[endNodeX, endNodeZ].ChildSetActive(endDir, true, false);

                        score += 10;
                    }
                }

                // Calculate new currentValue
                differenceStart += QuickSweep(startNodeX, startNodeZ);
                differenceEnd += QuickSweep(endNodeX, endNodeZ);
                puzzle.currentValue += differenceStart + differenceEnd;

                if (puzzle.currentValue == puzzle.winValue)
                {
                    if(score/10 == puzzle.winValue/2)
                    {
                        score *= 2;
                    }

                    LevelComplete();
                }
                scoreText.text = "Score: " + score;
            }
        }
    }

    // Make a play if possible
    void TryToLinkRandomGeneratedNodes()
    {
        // floats
        float nodeRadius = 0.5f;
        float xStart = mouseStartPosition.x;
        float zStart = mouseStartPosition.z;
        float xEnd = mouseEndPosition.x;
        float zEnd = mouseEndPosition.z;

        // int
        int startNodeX = (int)(xStart + nodeRadius);
        int startNodeZ = (int)(zStart + nodeRadius);
        int endNodeX = (int)(xEnd + nodeRadius);
        int endNodeZ = (int)(zEnd + nodeRadius);

        int dirX = endNodeX - startNodeX;
        int dirZ = endNodeZ - startNodeZ;

        int startDir = -1, endDir = -1;

        if (dirX > -2 && dirX < 2 && dirZ == 0 && startNodeX != endNodeX)
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
            if (startNodeX < levels[currentLevel].width && startNodeX >= 0 && startNodeZ < levels[currentLevel].height && startNodeZ >= 0 && endNodeX < levels[currentLevel].width && endNodeX >= 0 && endNodeZ < levels[currentLevel].height && endNodeZ >= 0)
            {
                // Calculate new currentValue
                int differenceStart = -QuickSweepRandom(startNodeX, startNodeZ);
                int differenceEnd = -QuickSweepRandom(endNodeX, endNodeZ);

                if (level[startNodeX][ startNodeZ].GetComponent<Node>().nodeData.connections[startDir] && level[endNodeX][ endNodeZ].GetComponent<Node>().nodeData.connections[endDir])
                {
                    // Update Start Node
                    level[startNodeX][ startNodeZ].GetComponent<Node>().ChildSetActive(startDir, false, true);

                    // Update End Node
                    level[endNodeX][ endNodeZ].GetComponent<Node>().ChildSetActive(endDir, false, true);
                }
                else
                {
                    if (level[startNodeX][startNodeZ].GetComponent<Node>().currentConnections < level[startNodeX][ startNodeZ].GetComponent<Node>().nodeData.MaxConnections &&
                    level[endNodeX][endNodeZ].GetComponent<Node>().currentConnections < level[endNodeX][endNodeZ].GetComponent<Node>().nodeData.MaxConnections)
                    { 
                        // Update Start Node
                        level[startNodeX][startNodeZ].GetComponent<Node>().ChildSetActive(startDir, true, true);

                        // Update End Node
                        level[endNodeX][endNodeZ].GetComponent<Node>().ChildSetActive(endDir, true, true);
                    }
                }

                // Calculate new currentValue
                differenceStart += QuickSweepRandom(startNodeX, startNodeZ);
                differenceEnd += QuickSweepRandom(endNodeX, endNodeZ);
                currentValue += differenceStart + differenceEnd;

                if (currentValue == winValue)
                {
                    LevelComplete();
                }
            }
        }
    }

    // Compare if you connected more nodes
    public int QuickSweep(int w, int h)
    {
        return puzzle.level[w, h].currentConnections;
    }

    // Compare if you connected more nodes
    public int QuickSweepRandom(int w, int h)
    {
        Debug.Log(level[w][h]);
        return level[w][h].GetComponent<Node>().currentConnections;
    }

    #endregion

    // Get Level Dimensions
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

    #region Win
    // Get Level Win Value
    int GetWinValue()
    {
        int winValue = 0;
        foreach (var p in puzzle.level)
        {
            winValue += p.MaxConnections;
        }

        return winValue;
    }

    void LevelComplete()
    {
        gamestate = Gamestate.levelComplete;

        foreach (var p in puzzle.level)
        {
            p.SetOnLevelFinished();
        }
    }

    void NextLevel()
    {
        gamestate = Gamestate.nextLevel;
        SceneManager.LoadScene("SampleScene");
    }
    #endregion

}