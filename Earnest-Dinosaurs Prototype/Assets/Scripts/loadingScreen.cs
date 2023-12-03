using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//CODE ADAPTED FROM SPEED TUTOR: https://www.youtube.com/watch?v=NyFYNsC3H8k

//This code is for use with loading screens
public class loadingScreen : MonoBehaviour
{
    [SerializeField] GameObject loadingScreenObj;
    [SerializeField] Slider loadingSlider;

    public void LoadScene(string sceneName, bool waitToLoad = false)
    {
        loadingScreenObj.SetActive(true);
        StartCoroutine(LoadSceneAsync(sceneName, waitToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneName, bool waitToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOperation.isDone)
        {
            float progressPercent = Mathf.Clamp01(loadOperation.progress/0.9f);
            loadingSlider.value = progressPercent;
            yield return null;
        }
        loadingScreenObj.SetActive(false);
    }
}
