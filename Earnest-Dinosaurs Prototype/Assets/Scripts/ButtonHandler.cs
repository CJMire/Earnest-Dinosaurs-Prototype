using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    public void Resume()
    {
        //on press, resumes the game
        gameManager.instance.stateUnpause();
    }

    public void Restart()
    {
        //on press, reloads the game and continues the time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void EnterName()
    {
        //on press, takes string input from name and enters in leaderboard

    }

    public void Quit()
    {
        //on press, quits game
        Application.Quit();
    }
}
