using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("----- Components -----")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuEntry;
    [SerializeField] GameObject menuEnd;
    public GameObject player;

    [Header("----- HUD Text Components -----")]
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] TextMeshProUGUI textWaves;

    [Header("----- Settings -----")]
    public bool isPaused;
    float timeScaleOriginal;
    Stopwatch stopwatch;
    int waveCurrent;


    //Awake runs before Start() will, letting us instantiate this object
    void Awake()
    {
        instance = this;
        //creates new stopwatch and starts it
        stopwatch = Stopwatch.StartNew();
        timeScaleOriginal = Time.timeScale;
        //Sets current wave to " 1 " and updates HUD
        waveCurrent = 1;
        textWaves.text = "Wave:  " + waveCurrent.ToString();
    }

    void Update()
    {
        //Pressing the ESC key calls the pause function if the menu is available and the pause menu has a refrence
        if (Input.GetButtonDown("Cancel") && menuActive == null && menuPause != null)
        {
            statePause();
            menuActive = menuPause;
            menuActive.SetActive(isPaused);
        }
        //updates the timer everyframe if game is NOT paused
        if (!isPaused) {
        double seconds = ((stopwatch.ElapsedMilliseconds / 1000) % 60);
        double minutes = stopwatch.ElapsedMilliseconds / 60000;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    //Sets the game's time rate to zero to freeze it and frees the cursor
    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //stops stopwatch
        stopwatch.Stop();
    }

    //Returns the game time to it's original, locks the cursor, and removes the active menu
    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOriginal;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        menuActive.SetActive(false);
        menuActive = null;
        //resumes stopwatch
        stopwatch.Start();
    }

    public void UpdateWave()
    {
        //increase wave number & update HUD
        waveCurrent++;
        textWaves.text = "Wave:  " + waveCurrent.ToString();
    }

    //On player death, checks to see if wave ammount is higher than the lowest highscore wave amount
    public void OnDeath()
    {
        //should check if current wave amount is greather than lowest highscore wave amount (blank for now)
        //something like:
        //  if (menuActive == null && player.waveAmount >= highscore[4].waveAmount)
        //  {
        //      menuActive = menuEntry;
        //      menuActive.SetActive(true);
        //    /get input for player name
        //      string name = menuEntry.input
        //      /then insert player name, wave amount, and time into player list
        //      menuActive.SetActive(false);
        //      menuActive = null;
        //  }

        //displays Leaderboard (End Menu)
        //no need to set anything to null or false because the scene will either reset or just be quit out from here
            menuActive = menuEnd;
            menuActive.SetActive(true);
    }
}
