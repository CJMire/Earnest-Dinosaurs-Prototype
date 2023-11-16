using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading;
using UnityEditor.SearchService;

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
    public GameObject dmgPickup;
    public damagePickup damagePickupScript;

    [Header("----- HUD Components -----")]
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] TextMeshProUGUI textWaves;
    [SerializeField] TextMeshProUGUI textEnemyCount;
    [SerializeField] TextMeshProUGUI isSpawningText;
    [SerializeField] Image imageHPBar;
    [SerializeField] TextMeshProUGUI textAmmo;
    [SerializeField] GameObject hitMarker;
    [SerializeField] GameObject reloadIcon;
    [SerializeField] GameObject dmgUpIcon;
    [SerializeField] GameObject invincibilityIcon;
    [SerializeField] GameObject speedUpIcon;
    [SerializeField] GameObject bullet1;
    [SerializeField] Image imageReloadingIcon;
    [SerializeField] float hitMarkerRate;


    [Header("----- Settings -----")]
    [SerializeField] int timePenalty;
    private bool isPaused;
    float timeScaleOriginal;
    Stopwatch stopwatch;
    int waveCurrent;
    int enemyCount;
    private bool stopSpawning;
    private int enemiesPerWave;
    private int currentLevel;
    private int totalPenaltyTime;
    private float fillTime;
    private bool showRespawnWarning = true;

    [Header("----- Spawner Enemies -----")]
    [SerializeField] GameObject EnemyBase_1;
    [SerializeField] GameObject EnemyBase_2;
    [SerializeField] GameObject EnemyBase_3;
    [SerializeField] GameObject EnemyBase_4;

    [Header("----- Wave Settings -----")]
    [Range(1,5)] [SerializeField] int levelCompletion; //how many waves must be completed inorder to progress to next level
    [Range(1, 2)][SerializeField] int totalLevels;
    [SerializeField] float spawnSpeed;
    [SerializeField] float gracePeriod;
    [SerializeField] int totalEnemies;
    [Range(1, 10)][SerializeField] int newWaveIncrease; // how many more enemies will there be in new wave
    [SerializeField] List<gunStats> levelCompleteRewards = new List<gunStats>();

    [Header("----- Spawn Points -----")]
    List<GameObject> playerSpawnLocations = new List<GameObject>();
    List<Transform> enemySpawnLocations = new List<Transform>();

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

        //Sets spawn locations, current wave to " 1 ", sets stopSpawning, and updates wave text HUD
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

            StartCoroutine(SpawnWave());
        }

        if (playerScript.GetIsReloading())
        {
            FillReloadingIcon();
        }

        if(playerScript.shootDamage >= 20)
        {
            dmgIconOn();
        }
        else if(playerScript.shootDamage < 20)
        {
            dmgIconOff();
        }

        if (playerScript.playerSpeed >= 16)
        {
            speedIconOn();
        }
        else if (playerScript.shootDamage < 16)
        {
            speedIconOff();
        }

        if (bullet1.GetComponent<SphereCollider>().enabled == false)
        {
            invincibilityIconOn();
        }
        else if (bullet1.GetComponent<SphereCollider>().enabled == true)
        {
            invincibilityIconOff();
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

        //makes sure menuPrev is null when used again
        if(menuPrev != null)
        {
            menuPrev = null;
        }

        //resumes stopwatch
        stopwatch.Start();
    }

    //On player death, checks to see if wave ammount is higher than the lowest highscore wave amount
    public void OnDeath()
    {
        //no need to set anything to null or false because it's handled in the buttonHandler script
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
        }
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

    public void dmgIconOn()
    {
        dmgUpIcon.SetActive(true);
    }
    public void dmgIconOff()
    {
        dmgUpIcon.SetActive(false);
    }

    public void speedIconOn()
    {
        speedUpIcon.SetActive(true);
    }
    public void speedIconOff()
    {
        speedUpIcon.SetActive(false);
    }

    public void invincibilityIconOn()
    {
        invincibilityIcon.SetActive(true);
    }
    public void invincibilityIconOff()
    {
        invincibilityIcon.SetActive(false);
    }

    #endregion
    #region Wave Spawner methods

    public void updateEnemyCount(int amount)
    {
        enemyCount += amount;
        textEnemyCount.text = enemyCount.ToString();

        //End of wave
        if(enemyCount <= 0 && totalEnemies == 0)
        {
            //Game complete (completed waves == total waves)
            if (waveCurrent >= levelCompletion * totalLevels)
            {
                OnWin();
                return;
            }
            //Level Complete (completed waves == waves up to this level)
            else if (waveCurrent >= levelCompletion * currentLevel)
            {
                StartCoroutine("OnLevelSwitch");
            }

            //Continue to next wave
            totalEnemies = enemiesPerWave + newWaveIncrease; // increases amount of enemies per wave
            stopSpawning = false; // reactivates the if statement for the inovoke on SpawnWave

            //increase wave number & update HUD
            waveCurrent++;
            textWaves.text = "Wave:  " + waveCurrent.ToString();
        }
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
        int random = Random.Range(0, 4); // random number is generated as to which enemy will spawn
        if (random == 0)
        {
            enemy = EnemyBase_1;
        }
        else if (random == 1)
        {
            enemy = EnemyBase_2;
        }
        else if(random == 2)
        {
            enemy = EnemyBase_3;
        }
        else
        {
            enemy = EnemyBase_4;
        }
        return enemy;
    }

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

        //Since the transform of the location is accessed twice, made a local variable for easier code readability
        Transform location = enemySpawnLocations[Random.Range(0, enemySpawnLocations.Count - 1)]; //gets random location for spawning (instantiate)

        //Gets a randomly chosen enemy to spawn
        //then gets the position and rotation from the randomly selected spawn location assigned above
        Instantiate(GiveEnemy(), location.position, location.rotation);
        totalEnemies--;

        //if there are more enemies to spawn, it will go back to the beginning after waiting the spawnSpeed duration
        if (totalEnemies > 0)
        {
            yield return new WaitForSeconds(spawnSpeed);
            StartCoroutine(SpawnWave());
        }
        else // once the amount of enemies in a wave has spawned the invoke is cancelled
        {
            isSpawningText.text = string.Empty;
        }
    }

    private void OnLevelSwitch()
    {
        if (levelCompleteRewards.Count > 0)
        {
            playerScript.getGunStats(levelCompleteRewards[0]);
            levelCompleteRewards.RemoveAt(0);
        }
        
        currentLevel++;
        SetSpawnPositions();
        playerScript.spawnPlayer();
    }
    #endregion
    #region Reload methods

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
        fillTime += 1 / playerScript.GetReloadTime() * Time.deltaTime;
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
        playerSpawnLocations.Clear();
        enemySpawnLocations.Clear();
        int i = 1;
        while (true)
        {
            GameObject spawnPoint;
            //tries to find EXAMPLE - "PlayerPoint1.1" tag then "PlayerPoint2.1" tag and so on...
            //accidentally discovered "try" BC of error throws when trying to find tags that do not exist
            try { spawnPoint = GameObject.FindWithTag("PlayerSpawn" + i + "." + currentLevel.ToString()); }
            catch { break; }

            //if it finds it, it's added to the list
            playerSpawnLocations.Add(spawnPoint);
            i++;
        }
        i = 1; //resets searching index
        while (true)
        {
            Transform spawnPoint;
            //tries to find EXAMPLE - "SpawnPoint1.1" tag then "SpawnPoint2.1" tag and so on...
            //accidentally discovered "try" BC of error throws when trying to find tags that do not exist
            try { spawnPoint = GameObject.FindWithTag("SpawnPoint" + i + "." + currentLevel.ToString()).transform; }
            catch { break; }

            //if it finds it, it's added to the list
            enemySpawnLocations.Add(spawnPoint);
            i++;
        }
    }

    public Transform GetSpawnPos()
    {
        for (int i = 0; i < playerSpawnLocations.Count; i++)
        {
            if (!playerSpawnLocations[i].GetComponent<playerSpawner>().IsEnemyNear())
            {
                return playerSpawnLocations[i].transform;
            }
        }
        return playerSpawnLocations[0].transform;
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

    public bool GetShowRespawnWarning()
    {
        return showRespawnWarning;
    }

    public void SetShowRespawnWarning(bool show)
    {
        showRespawnWarning = show;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
    #endregion
}
