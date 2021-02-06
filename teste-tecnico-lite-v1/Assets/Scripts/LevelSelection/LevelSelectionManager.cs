using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] GameObject button;
    private void Start()
    {
        LevelData[] levels = Resources.LoadAll<LevelData>("Levels");
        int levelCount = levels.Length;

        GameObject go;
        for (int i = 0; i < levelCount; i++)
        {
            go = Instantiate(button, this.transform);
            go.GetComponent<ButtonController>().level = i + 1;
        }
    }
}

