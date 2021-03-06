﻿using System.Collections;
using Assets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MessageRouter : MonoBehaviour, IMessageRouter
{
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject gameActivity;

    public void OnMessage(string message)
    {
        Debug.Log("MessageRouter OnMessage: " + message);
        Message messageType = JsonUtility.FromJson<Message>(message);
        if (Constants.IS_ANDROID_INITIATED == false && messageType.type != 0)
            return;
        switch (messageType.type)
        {
            case 0: // init from android
                Debug.Log("MessageRouter 0 - init from android");
                GameInitMessage gameInitMessage = JsonUtility.FromJson<GameInitMessage>(message);
                Constants.USER_ID = gameInitMessage.user_id;
                Constants.GAME_ID = gameInitMessage.game_id;
                Constants.LEFT_PLAYER_ID = gameInitMessage.left_player_id;
                Constants.IS_ANDROID_INITIATED = true;
                Constants.BASE_URL = gameInitMessage.base_url;
                Debug.Log("MessageRouter Init from android Constants.USER_ID " + Constants.USER_ID);
                Debug.Log("MessageRouter Init from android Constants.GAME_ID " + Constants.GAME_ID);
                Debug.Log("MessageRouter Init from android Constants.LEFT_PLAYER_ID " + Constants.LEFT_PLAYER_ID);
                Debug.Log("MessageRouter Init from android Constants.IS_ANDROID_INITIATED " +
                          Constants.IS_ANDROID_INITIATED);
                Debug.Log("MessageRouter Init from android Constants.BASE_URL " + Constants.BASE_URL);

                if (gameInitMessage.user_id == -1)
                {
                    // if we are watching
                    Screen.orientation = ScreenOrientation.AutoRotation;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                    Debug.Log("MessageRouter We are watching");
                }
                else
                {
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    Screen.autorotateToPortrait = true;
                    Debug.Log("MessageRouter We are playing");
                }
                break;
            case 1: // screen orientation change request
                Debug.Log("MessageRouter 1 - screen orientation change request");
                ScreenOrientationMessage orientationMessage = JsonUtility.FromJson<ScreenOrientationMessage>(message);
                switch (orientationMessage.orientation)
                {
                    case 0: // Landscape
                        Screen.orientation = ScreenOrientation.Landscape;
                        Debug.Log("MessageRouter screen orientation Landscape");
                        Screen.autorotateToLandscapeLeft = true;
                        Screen.autorotateToLandscapeRight = true;
                        Screen.autorotateToPortrait = false;
                        break;
                    case 1: // Portrait
                        Screen.orientation = ScreenOrientation.Portrait;
                        Debug.Log("MessageRouter screen orientation Portrait");
                        Screen.autorotateToLandscapeLeft = false;
                        Screen.autorotateToLandscapeRight = false;
                        Screen.autorotateToPortrait = true;
                        Screen.autorotateToPortraitUpsideDown = true;
                        break;
                    case 2: //autorotate
                        Debug.Log("MessageRouter screen orientation autorotation");
                        Screen.orientation = ScreenOrientation.AutoRotation;
                        Screen.autorotateToLandscapeLeft = true;
                        Screen.autorotateToLandscapeRight = true;
                        Screen.autorotateToPortrait = true;
                        Screen.autorotateToPortraitUpsideDown = true;
                        break;
                }
                break;
            case 2: //switch scene
                SwitchSceneMessage sceneMessage = JsonUtility.FromJson<SwitchSceneMessage>(message);
                Debug.Log("MessageRouter switch scene: " + sceneMessage.scene);
                SwitchScene(sceneMessage);
                break;
        }
    }

    private void SwitchScene(SwitchSceneMessage sceneMessage)
    {
        switch (sceneMessage.scene)
        {
            case "7": //spin the disks
                SceneManager.LoadScene("SpinTheDisk");
                break;
            case "10": //rock scissors papper
                SceneManager.LoadScene("RockSpock");
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameActivity != null)
        {
            Debug.Log("MessageRouter onUnityBackPressed");
            gameActivity.Call("onUnityBackPressed");
        }
    }

    void Start()
    {
        Screen.fullScreen = false; // work with plugin to show status bar
        ObtainActivity();
    }

    void OnDestroy()
    {
        Debug.Log("MessageRouter OnDestroy");
        ReleaseActivity();
    }

    public void SendAndroidMessage(UnityMessage message)
    {
        if (gameActivity != null)
        {
            Debug.Log("MessageRouter onUnityMessage: " + JsonUtility.ToJson(message));
            gameActivity.Call("onUnityMessage", JsonUtility.ToJson(message));
        }
    }

    public void ObtainActivity()
    {
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        gameActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        Debug.Log("MessageRouter onUnityReady");
        gameActivity.Call("onUnityReady");
    }

    public void ReleaseActivity()
    {
        gameActivity = null;
        unityPlayer = null;
    }
}