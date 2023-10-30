using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void Resume()
    {
        //GameManager.instance.ResumeGame();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //GameManager.instance.ResumeGame();
    }

    public void Submit()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }
}
