using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("----- Components -----")]
    [SerializeField] AudioSource aud;

    [Header("----- Menu Components -----")]
    [SerializeField] GameObject menuActive;
    private GameObject menuPrev;
    public GameObject buttonPrev;

    [Header("----- Game Menu Components -----")]
    [SerializeField] GameObject menuPause;
    public GameObject menuPauseButton;
    [SerializeField] GameObject menuWin;
    public GameObject menuWinButton;
    [SerializeField] GameObject menuLose;
    public GameObject menuLoseButton;
    [SerializeField] GameObject menuRespawnWarning;
    public GameObject menuRespawnWarningButton;
    [SerializeField] GameObject menuRestartWarning;
    public GameObject menuRestartWarningButton;

    [Header("----- Main Menu Components -----")]
    [SerializeField] GameObject menuMask;
    [SerializeField] GameObject menuMain;
    public GameObject menuMainButton;
    [SerializeField] GameObject menuOptions;
    public GameObject menuOptionsButton;
    [SerializeField] GameObject menuGuide;
    public GameObject menuGuideButton;
    [SerializeField] GameObject menuCredits;
    public GameObject menuCreditsButton;
    [SerializeField] GameObject menuShop;
    public GameObject menuShopButton;

    [Header("----- Game Components -----")]
    public GameObject portal;
    public GameObject player;
    public playerController playerScript;
    public GameObject dmgPickup;
    public damagePickup damagePickupScript;

    [Header("----- HUD Components -----")]
    [SerializeField] TextMeshProUGUI textTimer;
    [SerializeField] TextMeshProUGUI textWaves;
    [SerializeField] TextMeshProUGUI textEnemyLeft;
    [SerializeField] TextMeshProUGUI textEnemyCount;
    [SerializeField] TextMeshProUGUI isSpawningText;
    [SerializeField] Image imageHPBar;
    [SerializeField] Image imageBossHPBar;
    [SerializeField] TextMeshProUGUI textAmmo;
    [SerializeField] GameObject playerHurtScreen;
    [SerializeField] GameObject playerLowHealthScreen;
    [SerializeField] GameObject hitMarker;
    [SerializeField] GameObject reloadIcon;
    [SerializeField] GameObject dmgUpIcon;
    [SerializeField] GameObject invincibilityIcon;
    [SerializeField] GameObject speedUpIcon;
    [SerializeField] GameObject bullet1;
    [SerializeField] Image imageReloadingIcon;
    [SerializeField] float hitMarkerRate;


    [Header("----- Settings -----")]
    GameObject loadingScreenObj;
    bool isOnMainMenu;
    [SerializeField] int timePenalty;
    private bool isPaused;
    float timeScaleOriginal;
    Stopwatch stopwatch;
    public int waveCurrent;
    public int enemyCount;
    private bool stopSpawning;
    public int enemiesPerWave;
    private int currentLevel;
    private int totalPenaltyTime;
    private float fillTime;
    private bool showRespawnWarning = true;
    private bool isLowHealth = false;
    private bool playingHealthFlash = false;
    private bool bossIsDead = false;
    private bool bossIsSpawned = false;
    private bool portalSpawned = false;

    [Header("----- Spawner Enemies -----")]
    [SerializeField] GameObject[] enemyLineup;
    //[SerializeField] GameObject EnemyBase_1;
    //[SerializeField] GameObject EnemyBase_2;
    //[SerializeField] GameObject EnemyBase_3;
    //[SerializeField] GameObject EnemyBase_4;
    //[SerializeField] GameObject EnemyBase_5;
    private int barrierChancePercentage;

    [Header("----- Boss Enemies -----")]
    [SerializeField] GameObject MechBoss;
    [SerializeField] GameObject SummonerBoss;
    [SerializeField] GameObject FinalBoss;
    [SerializeField] GameObject bossHealthBar;
    [SerializeField] TextMeshProUGUI bossName;
    [SerializeField] GameObject currentBoss;

    [Header("----- Wave Settings -----")]
    [Range(1,5)] [SerializeField] int levelCompletion; //how many waves must be completed inorder to progress to next level
    [Range(1, 2)][SerializeField] int totalLevels;
    [SerializeField] float spawnSpeed;
    [SerializeField] float gracePeriod;
    [SerializeField] public int totalEnemies;
    [Range(1, 10)][SerializeField] public int newWaveIncrease; // how many more enemies will there be in new wave

    [Header("----- Spawn Points -----")]
    List<GameObject> playerSpawnLocations = new List<GameObject>();
    public List<Transform> enemySpawnLocations = new List<Transform>();

    //Awake runs before Start() will, letting us instantiate this object
    void Awake()
    {
        //This is to avoid obvious errors with the system thinking health is 0. This will likely only happen on first run of install
        if (PlayerPrefs.GetInt("playerMaxHP") <= 0) FactoryReset();

        Time.timeScale = 1.0f;
        instance = this;

        if(SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            isOnMainMenu = true;
            SetActiveMenu(menuMain);
            FactoryReset();
        }
        else
        {
            //creates new stopwatch and starts it
            stopwatch = Stopwatch.StartNew();
            timeScaleOriginal = Time.timeScale;

            //Find player from the tag 
            player = GameObject.FindWithTag("Player");
            playerScript = player.GetComponent<playerController>();

            //Set current level
            currentLevel = PlayerPrefs.GetInt("level");

            //Sets spawn locations, current wave to " 1 ", sets stopSpawning, and updates wave text HUD
            //Since level 3 works differently, the spawning logic is offloaded to there (See elevatorFoorOne.cs)
            if(SceneManager.GetActiveScene().name != "Level 3") SetSpawnPositions();
            waveCurrent = 1;
            textWaves.text = "Wave:  " + waveCurrent.ToString();
            stopSpawning = false;

            //Sets current amount of enemies to zero and updates HUD
            enemyCount = 0;
            barrierChancePercentage = 50;
            textEnemyCount.text = enemyCount.ToString();

            totalPenaltyTime = 0; //Sets total penalty time
            imageReloadingIcon.fillAmount = 0; //Makes sure the reload icon is 0 and not seen
            fillTime = 0; //Sets fillTime for use in the FillReloadingIcon
            playerLowHealthScreen.SetActive(false); //makes sure the low health screen is off
        }
    }

    void Update()
    {
        if(loadingScreenObj == null) loadingScreenObj = GameObject.FindWithTag("Loading Screen");
        if (!isOnMainMenu)
        {
            //Pressing the ESC key calls the pause function if the menu is available and the pause menu has a refrence
            if (Input.GetButtonDown("Cancel") && menuActive == null && menuPause != null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
                selectButton(menuPauseButton);
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

        if(currentBoss == null && bossIsSpawned)
        {
            bossIsDead = true;
        }

        if (SceneManager.GetActiveScene().name == "Level 3" && bossIsDead)
        {
            updateEnemyCount(enemyCount);
        }
            if (bossIsDead && !portalSpawned)
        {
            textWaves.gameObject.SetActive(true);
            textEnemyLeft.gameObject.SetActive(true);
            textEnemyCount.gameObject.SetActive(true);
            isSpawningText.gameObject.SetActive(true);
            bossHealthBar.gameObject.SetActive(false);
            portalSpawn();
        }
        }
    }

    public void switchSceneAsync(string sceneName)
    {
        if (loadingScreenObj != null)
        {
            loadingScreenObj.gameObject.GetComponent<loadingScreen>().LoadScene(sceneName);
        }
    }

    //These are all the player pref keys as of now
    //"playerHP" - int
    //"playerMaxHP" - int
    //"level" - int
    //"gun1" - int
    //"gun2" - int
    //"gun3" - int
    //"SFXVolume" - float
    //"MusicVolume" - float

    //This method is for starting new runs
    public void ResetGameManagerValues()
    {
        PlayerPrefs.SetInt("level", 1);
        PlayerPrefs.SetInt("playerHP", PlayerPrefs.GetInt("playerMaxHP"));
        //The 1 means the player has it, and the 0 means they don't
        PlayerPrefs.SetInt("gun1", 1);
        PlayerPrefs.SetInt("gun2", 0);
        PlayerPrefs.SetInt("gun3", 0);
    }

    //This method is to stage for changing levels (This should be used at the END of a level)
    public void SavePlayerData()
    {
        //This will increment the level
        PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level")+1);
        //Saves the player health
        PlayerPrefs.SetInt("playerHP", playerScript.getPlayerCurrentHP());
        //These ifs will give the player the weapon upon level completion, and will take effect on the next level
        //There is not a Level 3 case because that is the last level and there will be no reward
        if (SceneManager.GetActiveScene().name == "Level 1") PlayerPrefs.SetInt("gun2", 1);
        else if (SceneManager.GetActiveScene().name == "Level 2") PlayerPrefs.SetInt("gun3", 1);
    }

    //This is used to set the playerPrefs back to a fresh install, or to a new save
    public static void FactoryReset()
    {
        //Game state
        PlayerPrefs.SetInt("level", 1);
        //Player stats
        PlayerPrefs.SetInt("playerHP", 15);
        PlayerPrefs.SetInt("playerMaxHP", 15);
        PlayerPrefs.SetInt("gun1", 1);
        PlayerPrefs.SetInt("gun2", 0);
        PlayerPrefs.SetInt("gun3", 0);
        //Boss tokens
            //PlayerPrefs.SetInt("BossTokens",0.5f);
        //Purchased upgrades
        //Options prefs
        PlayerPrefs.SetFloat("SFXVolume",0.5f);
        PlayerPrefs.SetFloat("MusicVolume", 0.5f);
    }

    #region Main Menu Management
    public void switchMenu(GameObject requested)
    {
        StartCoroutine(transition(requested));
    }

    IEnumerator transition(GameObject requested)
    {
        if (isOnMainMenu)
        {
            menuMask.SetActive(true); //This is to show the mask, otherwise it would block button functionality

            StartCoroutine(playTransition());
            yield return new WaitForSeconds(0.5f);

            menuActive.SetActive(false);
            menuPrev = menuActive;
            menuActive = requested;
            menuActive.SetActive(true);

            StartCoroutine(playTransition());
            yield return new WaitForSeconds(0.5f);
            menuMask.SetActive(false);
        }
    }

    //Toggles the main menu mask
    IEnumerator playTransition()
    {
        Color clr = menuMask.GetComponentInChildren<Image>().color;
        if (clr != null)
        {
            //Take it down
            while (clr.a < 1.0f)
            {
                clr.a += 0.1f;
                menuMask.GetComponentInChildren<Image>().color = clr;
                yield return new WaitForSeconds(0.05f);
            }
            //This is to avoid float errors
            clr.a = 1;
            menuMask.GetComponentInChildren<Image>().color = clr;

            //Put it up
            while (clr.a > 0.0f)
            {
                clr.a -= 0.1f;
                menuMask.GetComponentInChildren<Image>().color = clr;
                yield return new WaitForSeconds(0.05f);
            }
            //This is to avoid float errors
            clr.a = 0;
            menuMask.GetComponentInChildren<Image>().color = clr;
        }
        else UnityEngine.Debug.Log("No color component found!");
        yield return null;
    }

    public void selectButton(GameObject button)
    {
        buttonPrev = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    #endregion
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
        selectButton(menuLoseButton);
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
        selectButton(menuWinButton);
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

    //will turn on the low health screen
    public void OnLowHealth(bool lowHP)
    {
        if (lowHP && !playingHealthFlash)
        {
            isLowHealth = lowHP;
            playerLowHealthScreen.SetActive(lowHP); // turns it on

            //checks if the last time it was used, it was set back to prefered alpha level, if not this will
            if (playerLowHealthScreen.GetComponent<Image>().color.a != .39f)
            {
                //Code snippet found @https://stackoverflow.com/questions/48259663/unity-set-alpha-via-code
                Color newC = playerLowHealthScreen.GetComponent <Image>().color;
                newC.a = .39f;
                playerLowHealthScreen.GetComponent<Image>().color = newC;
            }

            StartCoroutine(HeartBeat());
            playingHealthFlash = true;
            
        }
        else
        {
            isLowHealth = lowHP;
            playerLowHealthScreen.SetActive(lowHP);
        }
        
    }

    IEnumerator HeartBeat()
    {
        yield return new WaitForSeconds(1.2f);
        if (isLowHealth)
        {
            Color newC = playerLowHealthScreen.GetComponent<Image>().color;
            newC.a = .58f;
            playerLowHealthScreen.GetComponent<Image>().color = newC;
            yield return new WaitForSeconds(.1f);
            newC.a = .39f;
            playerLowHealthScreen.GetComponent<Image>().color = newC;
            yield return new WaitForSeconds(.1f);
            newC.a = .58f;
            playerLowHealthScreen.GetComponent<Image>().color = newC;
            yield return new WaitForSeconds(.1f);
            newC.a = .39f;
            playerLowHealthScreen.GetComponent<Image>().color = newC;
            if (isLowHealth)
            {
                StartCoroutine(HeartBeat());
            }
        }
        else
        {
            playingHealthFlash = false;
            playerLowHealthScreen.SetActive(false);
            yield break;
        }
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
                if(SceneManager.GetActiveScene().name == "Level 3" && waveCurrent == 3 && bossIsDead)
                {
                    OnWin();
                    return;
                }

                //Spawned boss
                else
                {
                    bossSpawn();
                }
            }
            else
            {
                //Continue to next wave
                totalEnemies = enemiesPerWave + newWaveIncrease; // increases amount of enemies per wave
                stopSpawning = false; // reactivates the if statement for the inovoke on SpawnWave

                //increase wave number & update HUD
                waveCurrent++;
                textWaves.text = "Wave:  " + waveCurrent.ToString();
            }
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
        //GameObject enemy;
        int random = Random.Range(0, enemyLineup.Length); // random number is generated as to which enemy will spawn

        return enemyLineup[random];
        /*
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
        else if(random == 3)
        {
            enemy = EnemyBase_4;
        }
        else
        {
            enemy = EnemyBase_5;
        }
        return enemy;
        */
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

        //Add variable name to access the SetBarrierHP function 
        GameObject enemyClone;
        enemyClone = Instantiate(GiveEnemy(), location.position, location.rotation);

        if (enemyClone.CompareTag("Enemy"))
        {
            //Setting enemy barrier 
            int barrierChance = Random.Range(0, 100);

            //We can change the chance of enemy spawning with barrier rate by adjusting barrierChancePercentage
            //currently set it to 50% chance of spawning with barrier.
            if (barrierChance < barrierChancePercentage)
            {
                enemyClone.GetComponent<enemyAI>().SetBarrierHP(3);
            }

            else
            {
                enemyClone.GetComponent<enemyAI>().SetBarrierHP(0);
            }
        }

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

    public void OnLevelSwitch()
    {
        currentLevel++;
        SetSpawnPositions();
        playerScript.spawnPlayer();
    }

    private void portalSpawn()
    {
        portalSpawned = true;
        Instantiate(portal, new Vector3(0, 1.5f, 0), Quaternion.identity);
    }

    private void bossSpawn()
    {
        if(!bossIsDead && waveCurrent == 3 && enemyCount == 0 && !bossIsSpawned)
        {
            bossIsSpawned = true;
            textWaves.gameObject.SetActive(false);
            textEnemyLeft.gameObject.SetActive(false);
            textEnemyCount.gameObject.SetActive(false);
            isSpawningText.gameObject.SetActive(false);

            if (SceneManager.GetActiveScene().name == "Level 1")
            {
                currentBoss = Instantiate(MechBoss, new Vector3(0, 200.0f, 0), Quaternion.identity);
                bossName.text = "Sergeant Kuller";
                bossHealthBar.gameObject.SetActive(true);
            }

            else if(SceneManager.GetActiveScene().name == "Level 2")
            {
                currentBoss = Instantiate(SummonerBoss, new Vector3(0, 1.5f, 0), Quaternion.identity);
                bossName.text = "Mastermind";
                bossHealthBar.gameObject.SetActive(true);
            }

            else if(SceneManager.GetActiveScene().name == "Level 3")
            {
                currentBoss = Instantiate(FinalBoss, new Vector3(0, 68f, 0), Quaternion.identity);
                bossName.text = "Master-Prime";
                bossHealthBar.gameObject.SetActive(true);
            }
            
            else
            {
                //Do nothing 
            }
        }
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

    #region Main Menus
    public void SetMainMenu(GameObject menu)
    {
        menuMain = menu;
    }

    public GameObject GetMainMenu()
    {
        return menuMain;
    }

    public void SetMainOptions(GameObject menu)
    {
        menuOptions = menu;
    }

    public GameObject GetMainOptions()
    {
        return menuOptions;
    }

    public void SetMainGuide(GameObject menu)
    {
        menuGuide = menu;
    }

    public GameObject GetMainGuide()
    {
        return menuGuide;
    }

    public void SetMainCredits(GameObject menu)
    {
        menuCredits = menu;
    }

    public GameObject GetMainCredits()
    {
        return menuCredits;
    }

    public void SetMainShop(GameObject menu)
    {
        menuShop = menu;
    }

    public GameObject GetMainShop()
    {
        return menuShop;
    }

    public void SetMainMask(GameObject menu)
    {
        menuMask = menu;
    }

    public GameObject GetMainMask()
    {
        return menuMask;
    }
    #endregion

    public void SetBossHealth(int currentHP, int maxHP)
    {
        imageBossHPBar.fillAmount = (float)currentHP / maxHP;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }

    public int GetEnemyCount()
    {
        return enemyCount;
    }
    #endregion
}
