using UnityEngine;
using Unity.Collections;
using System;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    # region Singleton implementation
    public static Client Instance { set; get; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    //single cnx to server
    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    //methods

    public void Init(String ip, ushort port)
    {
        driver = NetworkDriver.Create();
        //for peaople to connect anyone
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip,port);
        endpoint.Port = port;
        connection = driver.Connect(endpoint);
        Debug.Log("Attemping to connect to server on" + endpoint.Address);

        isActive = true;
        //keep a live msg
        RegisterToEvent();
    }
    public void Shutdown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
        { return; }

        driver.ScheduleUpdate().Complete();
        CheckAlive();
        UpdateMessagePump();

    }
    private void CheckAlive()
    {
        if(!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }


    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("Client connected");

            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                    NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                    Debug.Log("Client got disconnected from server");
                    connection = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown(); //when  we re in a two person game

            }
        }
        
    }

    //server specific 
    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    //Event pqrsing 
    private void RegisterToEvent()
    {
       NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage nm)
    {
        // Send it back, to keep both side alive
        SendToServer(nm);
    }


}
