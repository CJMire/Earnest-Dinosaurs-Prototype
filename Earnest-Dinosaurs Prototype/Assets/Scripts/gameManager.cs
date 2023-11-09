using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("----- Components -----")]
    [SerializeField] GameObject menuActive;
    private GameObject menuPrev;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuRespawnWarning;
    [SerializeField] GameObject menuRestartWarning;
    [SerializeField] GameObject playerHurtScreen;

    public GameObject player;
    public playerController playerScript;

    [Header("----- HUD Components -----")]
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] TextMeshProUGUI textWaves;
    [SerializeField] TextMeshProUGUI textEnemyCount;
    [SerializeField] TextMeshProUGUI isSpawningText;
    [SerializeField] Image imageHPBar;
    [SerializeField] TextMeshProUGUI textAmmo;
    [SerializeField] GameObject hitMarker;
    [SerializeField] GameObject reloadIcon;
    [SerializeField] Image imageReloadingIcon;
    [SerializeField] float hitMarkerRate;


    [Header("----- Settings -----")]
    [SerializeField] int timePenalty;
    public bool isPaused;
    float timeScaleOriginal;
    Stopwatch stopwatch;
    int waveCurrent;
    int enemyCount;
    public bool stopSpawning;
    public int enemiesAlive;
    private int enemiesPerWave;
    private int currentLevel;
    private int totalPenaltyTime;
    private float fillTime;
    public float timetillSpawn;
    public bool showRespawnWarning = true;

    [Header("----- Spawner Enemies -----")]
    [SerializeField] private GameObject EnemyBase_1;
    [SerializeField] private GameObject EnemyBase_2;
    [SerializeField] private GameObject EnemyBase_3;

    [Header("----- Wave Settings -----")]
    [SerializeField] int waveCount;
    [SerializeField] float spawnSpeed;
    [SerializeField] float gracePeriod;

    [Header("----- Spawn Points -----")]
    [SerializeField] Transform playerSpawnPos;
    [SerializeField] Transform spawnLocation1;
    [SerializeField] Transform spawnLocation2;
    [SerializeField] Transform spawnLocation3;
    [SerializeField] Transform spawnLocation4;


    [Header("----- Enemy Settings -----")]
    [SerializeField] public int totalEnemies;


    //Awake runs before Start() will, letting us instantiate this object
    void Awake()
    {
        instance = this;

        //creates new stopwatch and starts it
        stopwatch = Stopwatch.StartNew();
        timeScaleOriginal = Time.timeScale;

        //Find player from the tag 
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

        //Set current level
        currentLevel = 1;

        //Sets spawn locations, current wave to " 1 ", sets stopSpawning, and updates HUD
        SetSpawnPositions();
        waveCurrent = 1;
        textWaves.text = "Wave:  " + waveCurrent.ToString();
        stopSpawning = false;

        //Sets current amount of enemies to zero and updates HUD
        enemyCount = 0;
        textEnemyCount.text = enemyCount.ToString();

        totalPenaltyTime = 0; //Sets total penalty time
        imageReloadingIcon.fillAmount = 0; //Makes sure the reload icon is 0 and not seen
        fillTime = 0; //Sets fillTime for use in the FillReloadingIcon
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
        if (!isPaused)
        {
            textTimer.text = GiveTime();
        }

        if (stopSpawning == false)
        {
            UpdateEnemiesPerWave();

            //----------------------
            //InvokeRepeating("SpawnWave", gracePeriod, spawnSpeed); // begins to spawn enemies
            //-----------------------

            StartCoroutine(SpawnWave());
            
            //-------------------
            //stopSpawning = true; // makes sure there is only one invoke at a time
            //-------------------
        }

        if (totalEnemies == 0) // once the amount of enemies in a wave has spawned the invoke is cancelled
        {
            isSpawningText.text = string.Empty;
            //---------------------
            //CancelInvoke();
            //--------------------
        }

        //end of wave
        if(enemyCount == 0 && totalEnemies == 0)
        {
            totalEnemies = enemiesPerWave + 3; // increases amount of enemies per wave
            stopSpawning = false; // reactivates the if statement for the inovoke on SpawnWave
        }

        if (playerScript.GetIsReloading())
        {
            FillReloadingIcon();
        }
    }

    #region HUD and Game managing methods
    //Sets the game's time rate to zero to freeze it and frees the cursor
    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //stops stopwatch
        stopwatch.Stop();
    }

    //Returns the game time to it's original, locks the cursor, and removes the active menu
    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOriginal;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        menuActive.SetActive(false);
        menuActive = null;

        //resumes stopwatch
        stopwatch.Start();
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
            statePause();
            menuActive = menuLose;
            menuActive.SetActive(true);
    }

    public IEnumerator playerHurtFlash()
    {
        playerHurtScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        playerHurtScreen.SetActive(false);
    }

    //For use in the UpdateEnemyCount for now
    public void OnWin()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void updateHUD()
    {
        //Since ammoCount is accessed multiple times, made a local variable
        int ammoCount = playerScript.getPlayerCurrentAmmo();

        //updates the fill amount of health bar
        imageHPBar.fillAmount = (float)playerScript.getPlayerCurrentHP() / playerScript.getPlayerMaxHP();

        //updates the current ammo in magazine for use
        textAmmo.text = ammoCount + " / " + playerScript.getPlayerMaxAmmo();

        //checks if the reload icon needs to be on or not
        if (ammoCount == 0)
        {
            StartCoroutine(ReloadFlash());
            //-----------------
            //InvokeRepeating("ReloadIconOn", 1f, 1f);
            //InvokeRepeating("ReloadIconOff", 1.5f, 1.5f);
            //-----------------
        }

        //--------------------
        //else if (ammoCount > 0)
        //{
        //    reloadIcon.SetActive(false);
        //    CancelInvoke();
        //}
        //---------------------
    }

    //used in Update() to display give total time in a string format 
    public string GiveTime()
    {
        //takes the total milliseconds that have passed and converts them into usable ints for min. & sec.
        int seconds = (int)(stopwatch.ElapsedMilliseconds / 1000 % 60);
        int minutes = (int)stopwatch.ElapsedMilliseconds / 60000;

        //if ANY penalty time needs to be added, this if check will do so and convert accordingly
        if (totalPenaltyTime > 0)
        {
            seconds += totalPenaltyTime;
            while (seconds >= 60)
            {
                seconds -= 60;
                minutes++;
            }
        }

        //formats for timer text
        return string.Format("{00:00}:{01:00}", minutes, seconds);
    }

    //for use in the buttonHandler script
    public void UpdateTotalTime()
    {
        totalPenaltyTime += timePenalty;
    }
    #endregion
    #region Wave Spawner methods

    public void UpdateWave()
    {
        //increase wave number & update HUD
        waveCurrent++;
        textWaves.text = "Wave:  " + waveCurrent.ToString();
    }

    public void updateEnemyCount(int amount)
    {
        enemyCount += amount;
        textEnemyCount.text = enemyCount.ToString();
        //Calls when 3nd wave is completed for now
        if(enemyCount <= 0 && waveCurrent == 3)
        {
            OnWin();
        }
        if (enemyCount <= 0 && totalEnemies == 0)
            UpdateWave();
    }

    // begins to spawn wave of enemies
    //Was SpawnWave() but is now SpawnEnemy
    public void SpawnEnemy()
    {
        int random = Random.Range(0, 4); // random number is generated as to which spawn will happen
        GameObject enemy = GiveEnemy();
        if (random == 1)
        {
            Instantiate(enemy, spawnLocation1);
        }
        else if (random == 2)
        {
            Instantiate(enemy, spawnLocation2);
        }
        else if (random == 3)
        {
            Instantiate(enemy, spawnLocation3);
        }
        else
        {
            Instantiate(enemy, spawnLocation4);
        }
        totalEnemies--;
    }

    // updates a int to have a placeholder for amount of enemies in a wave to then update it in the next wave
    public void UpdateEnemiesPerWave()
    {
        enemiesPerWave = totalEnemies;
    }
    
    //Gives random enemy to use for spawning
    GameObject GiveEnemy()
    {
        GameObject enemy;
        int random = Random.Range(0, 3); // random number is generated as to which enemy will spawn
        if (random == 0)
        {
            enemy = EnemyBase_1;
        }
        else if (random == 1)
        {
            enemy = EnemyBase_2;
        }
        else
        {
            enemy = EnemyBase_3;
        }
        return enemy;
    }

    //checks if the SpawnWave is being invoked and updates the HUD as so
    //void IsSpawning()
    //{
    //    if (IsInvoking("SpawnWave"))
    //    {
    //        isSpawningText.text = "Enemies Spawning...";
    //    }
    //    else
    //    {
    //        isSpawningText.text = "";
    //    }
    //}

    IEnumerator SpawnWave()
    {
        //if its the start of the wave, the if-check makes the following happen once
        //sets stopSpawning to true so Update() calls this once
        //updates HUD to notify player that the wave is spawning
        //and the IEnumerator waits until the grace period is done
        if (totalEnemies == enemiesPerWave)
        {
            isSpawningText.text = "Enemies Spawning...";
            stopSpawning = true;
            yield return new WaitForSeconds(gracePeriod);
        }
            

        //spawns an enemy
        SpawnEnemy();

        //if there are more enemies to spawn, it will go back to the beginning after waiting the spawnSpeed duration
        if(totalEnemies > 0)
        {
            yield return new WaitForSeconds(spawnSpeed);
            StartCoroutine(SpawnWave());
        }
    }
    #endregion
    #region Reload methods
    //-----------------------------------
    //public void ReloadIconOn()
    //{
    //    reloadIcon.SetActive(true);
    //    Invoke("ReloadIconOff", 0.5f);
    //}

    //public void ReloadIconOff()
    //{
    //    reloadIcon.SetActive(false);
    //}
    //-------------------------------------

    public IEnumerator Reload()
    {
        reloadIcon.SetActive(false);
        playerScript.SetIsReloading(true);
        yield return new WaitForSeconds(playerScript.GetReloadTime());
        //made a method in player script to set ammoCount, set isReloading to false, and update HUD
        playerScript.ReloadSuccess();
        fillTime = 0;
        imageReloadingIcon.fillAmount = 0;
    }

    IEnumerator ReloadFlash()
    {
        reloadIcon.SetActive(true);
        yield return new WaitForSeconds(1f);
        reloadIcon.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        if (playerScript.getPlayerCurrentAmmo() == 0 && !playerScript.GetIsReloading())
            StartCoroutine(ReloadFlash());
    }

    //Inspired by code snippet found @https://forum.unity.com/threads/help-with-getting-slider-to-fill-over-time.871177/
    void FillReloadingIcon()
    {
        imageReloadingIcon.fillAmount = Mathf.Lerp(0,1f,fillTime);
        fillTime += playerScript.GetReloadTime() * Time.deltaTime;
    }
    #endregion
    #region Getters and Setters
    public GameObject GetHitMarker()
    {
        return hitMarker;
    }

    public float getHitMarkerRate()
    {
        return hitMarkerRate;
    }

    public void SetSpawnPositions()
    {
        playerSpawnPos = GameObject.FindWithTag("PlayerSpawn" + currentLevel.ToString()).transform;
        spawnLocation1 = GameObject.FindWithTag("SpawnPoint1." + currentLevel.ToString()).transform;
        spawnLocation2 = GameObject.FindWithTag("SpawnPoint2." + currentLevel.ToString()).transform;
        spawnLocation3 = GameObject.FindWithTag("SpawnPoint3." + currentLevel.ToString()).transform;
        spawnLocation4 = GameObject.FindWithTag("SpawnPoint4." + currentLevel.ToString()).transform;
    }

    public Transform GetSpawnPos()
    {
        return playerSpawnPos;
    }

    public GameObject GetReloadIcon()
    {
        return reloadIcon;
    }

    public GameObject GetRespawnWarning()
    {
        return menuRespawnWarning;
    }

    public GameObject GetRestartWarning()
    {
        return menuRestartWarning;
    }

    public void SetPrevMenu(GameObject menu)
    {
        menuPrev = menu;
    }

    public GameObject GetPrevMenu()
    {
        return menuPrev;
    }

    public void SetActiveMenu(GameObject menu)
    {
        menuActive = menu;
    }

    public GameObject GetActiveMenu()
    {
        return menuActive;
    }
    #endregion
}
