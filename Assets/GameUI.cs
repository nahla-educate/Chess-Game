using TMPro;
using UnityEngine;

using Unity.Collections;
using System;
using Unity.Networking.Transport;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}

public class GameUI : MonoBehaviour
{

    public static GameUI Instance { set; get; }
    [SerializeField] private Animator menuAnimator;

    public Server server;
    public Client client;

    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;


   // Start is called before the first frame update
   private void Awake()
    {
        Instance = this;
        RegisterAvents();


    }
    //cameras
    public void ChangeCamera(CameraAngle index)
    {
        for(int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)index].SetActive(true);
    }

    //buttons
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        //SetLocalGame?.Invoke(true);
        // Check if it's a local game or not
        if (SetLocalGame != null)
        {
            SetLocalGame.Invoke(true);
        }
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }
    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
        Debug.Log("Online");
    }
    public void OnOnlineHostButton()
    {
       
        if (SetLocalGame != null)
        {
            SetLocalGame.Invoke(false);
        }
        server.Init(8007);
        client.Init("127.0.0.1", 8007); 
        menuAnimator.SetTrigger("HostMenu");
    }
    public void OnOnlineConnectButton()
    {
        if (SetLocalGame != null)
        {
            SetLocalGame.Invoke(false);
        }
        client.Init(addressInput.text, 8007);
    }
    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
        Debug.Log("back");
    }
    public void OnHostBackButton()
    {
       
        Debug.Log("back");
        server.Shutdown();
        client.Shutdown(); 
        menuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimator.SetTrigger("StartMenu");
       
    }

    #region
    private void RegisterAvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }



    private void UnregisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage obj)
    {
        menuAnimator.SetTrigger("InGameMenu");
    }
    #endregion
}
