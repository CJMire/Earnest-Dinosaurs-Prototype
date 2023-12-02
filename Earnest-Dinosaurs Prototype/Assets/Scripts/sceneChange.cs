using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneChange : MonoBehaviour
{
    public static gameManager instance;
    public int level = 1;
    public int actualWave;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            level++;
            SceneManager.LoadScene(level);
        }
    }


}
