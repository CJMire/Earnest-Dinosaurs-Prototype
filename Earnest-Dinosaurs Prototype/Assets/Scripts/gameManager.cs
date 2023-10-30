using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;

    float timeScaleOriginal;
    public bool isPaused;

    //Awake runs before Start() will, letting us instantiate this object
    void Awake()
    {
        instance = this;
        timeScaleOriginal = Time.timeScale;
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
    }

    //Sets the game's time rate to zero to freeze it and frees the cursor
    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //Returns the game time to it's original, locks the cursor, and removes the active menu
    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOriginal;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        menuActive.SetActive(false);
        menuActive = null;
    }
}
