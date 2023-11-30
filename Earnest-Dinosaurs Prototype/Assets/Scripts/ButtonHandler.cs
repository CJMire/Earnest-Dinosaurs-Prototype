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

    public void RestartButton()
    {
        gameManager.instance.GetActiveMenu().SetActive(false);
        gameManager.instance.SetPrevMenu(gameManager.instance.GetActiveMenu());
        gameManager.instance.SetActiveMenu(gameManager.instance.GetRestartWarning());
        gameManager.instance.GetActiveMenu().SetActive(true);
    }

    public void Restart()
    {
        //on press, reloads the game, restarts the timer, and then continues the timer
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

    public void RespawnButton()
    {
        if (gameManager.instance.GetShowRespawnWarning())
        {
            gameManager.instance.GetActiveMenu().SetActive(false);
            gameManager.instance.SetPrevMenu(gameManager.instance.GetActiveMenu());
            gameManager.instance.SetActiveMenu(gameManager.instance.GetRespawnWarning());
            gameManager.instance.GetActiveMenu().SetActive(true);
        }
        else
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        //Adds penalty time to timer
        gameManager.instance.UpdateTotalTime();

        //respawns player and resumes game
        gameManager.instance.playerScript.spawnPlayer(true);
        gameManager.instance.stateUnpause();
    }

    public void ToggleShowRespawn()
    {
        //toggles showRespawnWarning in gameManager
        gameManager.instance.SetShowRespawnWarning(!gameManager.instance.GetShowRespawnWarning());
    }

    public void Back()
    {
        gameManager.instance.GetActiveMenu().SetActive(false);
        gameManager.instance.SetActiveMenu(null);
        gameManager.instance.SetActiveMenu(gameManager.instance.GetPrevMenu());
        gameManager.instance.SetPrevMenu(null);
        gameManager.instance.GetActiveMenu().SetActive(true);
    }
}
