using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;
using System.Linq;

// Provides methods for rendering title UI
public class TitleUIHandler : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Arena");
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
    }
}