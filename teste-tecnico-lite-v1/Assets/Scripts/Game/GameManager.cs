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

    // ### Game Variables
    Gamestate gamestate;
    Node[,] level;
    int width;
    int height;
    int winValue;
    int currentValue;
    int currentLevel = 0;
    int score;
    int levelCount;

    LevelData[] levels;
    // ### Auxiliar variables
    float counter;

    // Objects to set in inspector
    [SerializeField] GameObject[] NodePrefabs;
    [SerializeField] Text scoreText, levelText;
    [SerializeField] AudioManager audioManager;

    // Touch variables
    private Vector3 mouseStartPosition, mouseEndPosition;

    // Start is called before the first frame update
    void Start()
    {
        levels = Resources.LoadAll<LevelData>("Levels");
        levelCount = levels.Length;
        audioManager.PlayOnLoop("Background");

        gamestate = Gamestate.gameStart;
        score = 0;

        SetLevel(GetLevel());
        GetLevelInfo();
        CalculateWinValue();

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
                    TryToLinkNodes();
                }
            }
        }

        if (gamestate == Gamestate.levelComplete)
        {
            counter += Time.deltaTime;
            if (counter > 3)
            {
                counter = 0;
                NextLevel();
            }
        }
    }

    #region Instantiate from level data
    // Retrieve level data from ScriptableObject and Instantiate level objects
    void GetLevelInfo()
    {
        width = levels[currentLevel].width;
        height = levels[currentLevel].height;
        level = new Node[width, height];

        GameObject go;
        for (int h = 0; h < levels[currentLevel].height; h++)
        {
            for (int w = 0; w < levels[currentLevel].width; w++)
            {
                if (levels[currentLevel].data[w + h * width] == 0)
                {
                    go = Instantiate(NodePrefabs[0], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                else if (levels[currentLevel].data[w + h * width] == 1)
                {
                    go = Instantiate(NodePrefabs[1], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                else
                {
                    go = Instantiate(NodePrefabs[2], new Vector3(w, 0, h), Quaternion.identity, this.transform);
                }
                go.GetComponent<Node>().MaxConnections = levels[currentLevel].data[w + h * width];
                go.GetComponent<Node>().UpdateTextMesh();
                level[w, h] = go.GetComponent<Node>();
            }
        }
    }
    #endregion

    #region Play Management

    // Make a play if possible
    void TryToLinkNodes()
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
                int differenceStart = -QuickSweep(startNodeX, startNodeZ);
                int differenceEnd = -QuickSweep(endNodeX, endNodeZ);

                if (level[startNodeX, startNodeZ].GetComponent<Node>().connections[startDir] && level[endNodeX,endNodeZ].GetComponent<Node>().connections[endDir])
                {
                    // Update Start Node
                    level[startNodeX, startNodeZ].GetComponent<Node>().ChildSetActive(startDir, false);

                    // Update End Node
                    level[endNodeX, endNodeZ].GetComponent<Node>().ChildSetActive(endDir, false);
                    score -= 15;
                    audioManager.Play("Lose");
                }
                else
                {
                    if (level[startNodeX,startNodeZ].GetComponent<Node>().currentConnections < level[startNodeX,startNodeZ].GetComponent<Node>().MaxConnections &&
                    level[endNodeX,endNodeZ].GetComponent<Node>().currentConnections < level[endNodeX,endNodeZ].GetComponent<Node>().MaxConnections)
                    {
                        // Update Start Node
                        level[startNodeX,startNodeZ].GetComponent<Node>().ChildSetActive(startDir, true);

                        // Update End Node
                        level[endNodeX,endNodeZ].GetComponent<Node>().ChildSetActive(endDir, true);
                    }
                    score += 10;
                    audioManager.Play("Win");
                }

                // Calculate new currentValue
                differenceStart += QuickSweep(startNodeX, startNodeZ);
                differenceEnd += QuickSweep(endNodeX, endNodeZ);
                currentValue += differenceStart + differenceEnd;

                if (currentValue == winValue)
                {
                    if (score / 10 == winValue / 2)
                    {
                        score *= 2;
                    }
                    audioManager.Stop("Win");
                    audioManager.Play("Completed");
                    LevelComplete();
                }
                scoreText.text = "Score: " + score;
            }
        }
    }

    // Compare if you connected more nodes
    public int QuickSweep(int w, int h)
    {
        return level[w,h].currentConnections;
    }

    #endregion

    #region Level Management
    void SetLevel(int _current_level)
    {
        currentLevel = _current_level;
        levelText.text = "Level " + (currentLevel + 1);
    }

    void SetLevelCompleted(int level, int completed)
    {
        string aux = "level" + (level + 1);
        PlayerPrefs.SetInt(aux, completed);
    }

    public void ResetCompletedLevels()
    {
        for (int i = 1; i <= levelCount; i++)
        {
            string aux = "level" + i;
            PlayerPrefs.SetInt(aux, 0);
        }
    }

    int GetLevel()
    {
        return PlayerPrefs.GetInt("currentLevel");
    }
    #endregion

    #region Win
    // Get Level Win Value
    void CalculateWinValue()
    {
        winValue = 0;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                winValue += levels[currentLevel].data[w + h * width];
            }
        }
    }

    // Animate on Win
    void LevelComplete()
    {
        gamestate = Gamestate.levelComplete;

        foreach (var p in level)
        {
            p.SetOnLevelFinished();
        }

        SetLevelCompleted(currentLevel, 1);
    }

    // Go to level selection
    void NextLevel()
    {
        gamestate = Gamestate.nextLevel;
        SceneManager.LoadScene("LevelSelection");
    }
    #endregion

}
