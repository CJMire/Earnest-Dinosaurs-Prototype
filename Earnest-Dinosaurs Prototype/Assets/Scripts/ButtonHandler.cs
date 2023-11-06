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
        gameManager.instance.stateUnpause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    public void Respawn()
    {
        //Adds penalty time to timer
        gameManager.instance.UpdateTotalTime();
        //respawns player and resumes game
        gameManager.instance.playerScript.spawnPlayer();
        gameManager.instance.stateUnpause();
    }
}
