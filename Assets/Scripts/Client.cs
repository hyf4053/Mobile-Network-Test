using UnityEngine.Networking;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    private const int USERNUMBER = 1;
    public int PORT;
    int frame;
    private const int WEB_PORT = 26001;
    public string SERVER_IP = "0";
    private const int BYTE_SIZE = 1024;
    private byte reliableChannel;
    private int connectionId;
    private int hostId;
    private byte error;
    public NetworkDiscovery networkDiscovery;
    //Dictionary<string,NetworkBroadcastResult> dic;
    
    public ArrayList STRING_IP;
    public IPAddress ipAddress;
    public int ipCounter;
    
    //private int webHostId;
    private bool isStarted;

    #region MonoBehaviour
    private void Start(){
        DontDestroyOnLoad(gameObject);
        ipAddress = new IPAddress();
        STRING_IP = new ArrayList();
        StartCoroutine(SearchServerIP());
        StartCoroutine(Init());
        ipCounter = 0;
    }
    private void Update(){
        UpdateMessagePump();
        
    }
    #endregion MonoBehaviour
    IEnumerator Init(){
        
        yield return new WaitForSeconds(6);
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc,USERNUMBER);

        //Client only code
        hostId = NetworkTransport.AddHost(topo,0);

#if UNITY_WEBGL && !UNITY_EDITOR
        //Web Client
        connectionId = NetworkTransport.Connect(hostId,ipAddress.ModifyIp((string)STRING_IP[0]),WEB_PORT,0,out error);
#else
        //standalone Client
        connectionId = NetworkTransport.Connect(hostId,ipAddress.ModifyIp((string)STRING_IP[0]),PORT,0,out error);
        Debug.Log(string.Format("Attemping from standalone"));
        //webHostId = NetworkTransport.AddWebsocketHost(topo,WEB_PORT,null);
#endif
        Debug.Log(string.Format("Attemping to connect on {0}...",ipAddress.ModifyIp((string)STRING_IP[0])));

        isStarted = true;
    }
    private void InitAgain(){
       NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc,USERNUMBER);

        //Client only code
        hostId = NetworkTransport.AddHost(topo,0);

#if UNITY_WEBGL && !UNITY_EDITOR
        //Web Client
        connectionId = NetworkTransport.Connect(hostId,SERVER_IP,WEB_PORT,0,out error);
#else
        //standalone Client
        connectionId = NetworkTransport.Connect(hostId,SERVER_IP,PORT,0,out error);
        Debug.Log(string.Format("Attemping from standalone"));
        //webHostId = NetworkTransport.AddWebsocketHost(topo,WEB_PORT,null);
#endif
        Debug.Log(string.Format("Attemping to connect on {0}...",SERVER_IP));

        isStarted = true;
    }

    IEnumerator SearchServerIP(){
        //yield return new WaitForSeconds(5);
        Debug.Log("Searching for server IP");
        networkDiscovery.isClient = true;
        networkDiscovery.isServer = false;
        networkDiscovery.Initialize();
        yield return new WaitForSeconds(5);
        Debug.Log("IP search end");
        if(networkDiscovery.broadcastsReceived.Count>1){
            foreach(string key in networkDiscovery.broadcastsReceived.Keys){STRING_IP.Add(key);}
        }
        Debug.Log(ipAddress.ModifyIp((string)STRING_IP[0]));
    }

    public void Shutdown(){
        isStarted = false;
        NetworkTransport.Shutdown();
    }
    public void UpdateMessagePump(){
        if(!isStarted){
            return;
        }
        int recHostId; // Is this from Web? Or standalone
        int connectionId;//Which user is sending me this?
        int channelId;//Which lane is user sending that message from

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId,out connectionId, out channelId,recBuffer,BYTE_SIZE,out dataSize,out error);
        switch(type){
            case NetworkEventType.Nothing:
            /*
                if(ipCounter<STRING_IP.Count){
                    SERVER_IP = (string)STRING_IP[ipCounter];
                    Debug.Log(SERVER_IP);
                    //Shutdown();
                    connectionId = NetworkTransport.Connect(hostId,SERVER_IP,PORT,0,out error);
                    ipCounter++;
                } */
                break;
            
            case  NetworkEventType.ConnectEvent:
                Debug.Log("We have connected to the server");
                break;

            case  NetworkEventType.DisconnectEvent:
                Debug.Log("We have disconnected to the server");
                break;
            
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);

                OnData(connectionId,channelId,recHostId,msg);
                break;

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Receive Server Broadcast");
               
                break;

        }

    }

#region OnData
    private void OnData(int connectionId, int channelId,int recHostId,NetMsg msg){

        switch(msg.OperationCode){
            case NetOP.None:
                Debug.Log("Unexpected Net Operation code");
                break;
        }
    }   
#endregion

    #region Send
    public void SendServer(NetMsg msg){
        //This is where we hold our data
        byte[] buffer = new byte[BYTE_SIZE];

        //This is where you would crush your data into a byte[]
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms,msg);

        NetworkTransport.Send(hostId,connectionId,reliableChannel,buffer,BYTE_SIZE,out error);
    }
    #endregion
    public void TESTFUNCCREATEINFO(){
        NetCreateMessage ncm = new NetCreateMessage();
        ncm.information = "Data received!";
        SendServer(ncm);
    }

}