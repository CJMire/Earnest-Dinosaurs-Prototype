using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneChange : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && SceneManager.GetActiveScene().name  == "Level 1")
        {
            SceneManager.LoadScene("Level 2");
        }
        else if(other.CompareTag("Player") && SceneManager.GetActiveScene().name == "Level 2")
        {
            SceneManager.LoadScene("Level 3");
        }
    }


}
