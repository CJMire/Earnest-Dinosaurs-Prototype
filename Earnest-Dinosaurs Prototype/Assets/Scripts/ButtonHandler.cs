using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        //on press returns to the main menu
        if (gameManager.instance.player != null) gameManager.instance.playerScript.StopAllCoroutines();
        gameManager.instance.StopAllCoroutines();
        SceneManager.LoadSceneAsync(0);
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
        if (SceneManager.GetActiveScene().name == "MainMenuScene") gameManager.instance.switchMenu(gameManager.instance.GetPrevMenu());
        else
        {
            gameManager.instance.GetActiveMenu().SetActive(false);
            gameManager.instance.SetActiveMenu(null);
            gameManager.instance.SetActiveMenu(gameManager.instance.GetPrevMenu());
            gameManager.instance.SetPrevMenu(null);
            gameManager.instance.GetActiveMenu().SetActive(true);
        }
    }

    #region Main Menu Buttons
    public void MainStart()
    {
        gameManager.instance.switchSceneAsync("Level 1");
    }

    public void MainOptions()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainOptions());
    }

    public void MainGuide()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainGuide());
    }

    public void MainCredits()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainCredits());
    }

    public void MainShop()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainShop());
    }

    public void MainExit()
    {
        Application.Quit();
    }
    #endregion
}
