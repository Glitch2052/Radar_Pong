using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AdManager adManager;
    private IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        
        BindObjects();
        //Show Logo
        yield return InitializeObjects();
        // BeginGame();
    }

    private void BindObjects()
    {
        
    }

    private IEnumerator InitializeObjects()
    {
        // Wait Till Initialization Of Objects
        // like ads handler or analytics services
        StartCoroutine(adManager.Init());
        yield return gameManager.Init();
    }
    
    // private void BeginGame()
    // {
    //     JSONNode node = new JSONObject();
    //     node.SetNextSceneType(SceneType.HomeScene);
    //     gameManager.LoadScene(StringID.HomeScene,node);
    // }
    //
    // private async UniTask LoadAddressableLocations()
    // {
    //     List<string> labels = new List<string>();
    //     foreach (ThemeName themeName in Enum.GetValues(typeof(ThemeName)))
    //     {
    //         if(themeName == ThemeName.Custom) continue;
    //         labels.Add(themeName.ToString());
    //     }
    //     labels.Add("Scenes");
    //     labels.Add("AudioClips");
    //     labels.Add("NormalMaps");
    //     await AssetLoader.Instance.LoadResourceLocations(labels);
    // }
}
