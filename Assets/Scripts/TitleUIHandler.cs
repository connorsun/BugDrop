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
    private bool load;
    public void OnStartButtonClicked()
    {
        load = true;
    }

    // IEnumerator LoadScene()
    // {
    //     AsyncOperation op = SceneManager.LoadScene("Arena");
    //     op.allowSceneActivation = true;
    //     while (!op.isDone)
    //     {
    //         yield return null;
    //     }
    // }
    public void Update()
    {
        if (load)
        {
            SceneManager.LoadScene("Arena");
        }
    }
}