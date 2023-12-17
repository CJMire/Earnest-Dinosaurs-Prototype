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
        gameManager.instance.selectButton(gameManager.instance.menuRestartWarningButton);
    }

    public void Restart()
    {
        //on press, reloads the game, restarts the timer, and then continues the timer
        gameManager.instance.stateUnpause();
        SceneManager.LoadScene("Level 1");
        gameManager.instance.playerScript.spawnPlayer(true);
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
        gameManager.instance.switchSceneAsync("MainMenuScene");
    }

    public void RespawnButton()
    {
        if (PlayerPrefs.GetInt("ShowRespawnWarning", 1) == 1)
        {
            gameManager.instance.GetActiveMenu().SetActive(false);
            gameManager.instance.SetPrevMenu(gameManager.instance.GetActiveMenu());
            gameManager.instance.SetActiveMenu(gameManager.instance.GetRespawnWarning());
            gameManager.instance.GetActiveMenu().SetActive(true);
            gameManager.instance.selectButton(gameManager.instance.menuRespawnWarningButton);
        }
        else
        {
            Respawn();
        }

        /*
        if (gameManager.instance.GetShowRespawnWarning())
        {
            gameManager.instance.GetActiveMenu().SetActive(false);
            gameManager.instance.SetPrevMenu(gameManager.instance.GetActiveMenu());
            gameManager.instance.SetActiveMenu(gameManager.instance.GetRespawnWarning());
            gameManager.instance.GetActiveMenu().SetActive(true);
            gameManager.instance.selectButton(gameManager.instance.menuRespawnWarningButton);
        }
        else
        {
            Respawn();
        }
        */
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
        PlayerPrefs.SetInt("ShowRespawnWarning", 0);

        /*
        //toggles showRespawnWarning in gameManager
        gameManager.instance.SetShowRespawnWarning(!gameManager.instance.GetShowRespawnWarning());
        */
    }

    public void Back()
    {
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            gameManager.instance.switchMenu(gameManager.instance.GetPrevMenu());
            gameManager.instance.selectButton(gameManager.instance.buttonPrev);
        }
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
        gameManager.instance.ResetGameManagerValues();
        gameManager.instance.switchSceneAsync("Level 1");
    }

    public void MainOptions()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainOptions());
        gameManager.instance.selectButton(gameManager.instance.menuOptionsButton);
    }

    public void MainGuide()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainGuide());
        gameManager.instance.selectButton(gameManager.instance.menuGuideButton);
    }

    public void MainCredits()
    {
        gameManager.instance.switchMenu(gameManager.instance.GetMainCredits());
        gameManager.instance.selectButton(gameManager.instance.menuCreditsButton);
    }

    public void MainShop()
    {
        gameManager.instance.ShowBossTokens();
        gameManager.instance.switchMenu(gameManager.instance.GetMainShop());
        gameManager.instance.selectButton(gameManager.instance.menuShopButton);
    }

    public void MainExit()
    {
        Application.Quit();
    }
    #endregion
}
