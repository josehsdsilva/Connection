using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public int level;
    [SerializeField] Text text;
    private void Start()
    {
        gameObject.name = level.ToString();
        text.text = level.ToString();

        if (PlayerPrefs.GetInt("level" + level) == 1)
        {
            gameObject.GetComponent<Image>().color = Color.green;
        }
        else if (level > 1 && PlayerPrefs.GetInt("level" + (level - 1)) == 0)
        {
            gameObject.GetComponent<Image>().color = Color.grey;
        }
    }

    public void SelectLevel()
    {
        if (level == 1 || PlayerPrefs.GetInt("level" + (level - 1)) == 1)
        {
            Debug.Log(level);
            Debug.Log(PlayerPrefs.GetInt("level" + (level - 1)));
            PlayerPrefs.SetInt("currentLevel", level - 1);
            SceneManager.LoadScene("Game");
        }
    }
}
