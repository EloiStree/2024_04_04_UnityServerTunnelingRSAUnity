using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using static RSATunneling;

public class ConnectToServerTunnelingRsaMono : MonoBehaviour
{
    [Header("Set RSA")]
    public string m_serverUri = "ws://81.240.94.97:3615";
    public RsaKeyPair m_rsaIdentity;

    public WebsocketConnectionRsaTunneling m_tunnel;
    public RSATunneling.TraffictInEvent m_trafficEvent;


    [ContextMenu("Generate Rsa Key Pair")]
    public void GenerateRsaKey1024() { 
        RSA rsa = RSA.Create(1024);
        m_rsaIdentity.m_hiddenPrivateKey.m_privateKey = rsa.ToXmlString(true);
        m_rsaIdentity.m_publicKey = rsa.ToXmlString(false);

    }

    [Header("Auto Start (temporary)")]
    public bool m_autoStart = true;
    public bool m_autoReconnect = true;   
    public float m_reconnectDelay = 5;


    public void SetPublicKeyXml(string xmlKey)
    {
        m_rsaIdentity.m_publicKey = xmlKey;
    }
    public void SetPrivateKeyXml(string xmlKey)
    {
        m_rsaIdentity.m_hiddenPrivateKey.m_privateKey = xmlKey;
    }

    public void SetWebsocketUrlWS(string url) {
        m_serverUri = url;
    }


    private void OnDestroy()
    {
        m_tunnel.CloseTunnel();
    }
    void Start()
    {
        StartClient();

    }

     void StartClient()
    {
        if (m_autoStart)
        {
            TryToLaunchOrRelaunchClient();
        }
        if(m_autoReconnect)
            InvokeRepeating("TryToLaunchOrRelaunchClient", m_reconnectDelay, m_reconnectDelay);
    }

    public void TryToLaunchOrRelaunchClient() {

        if(m_tunnel== null)

            LaunchNewConnection();

        else if (!m_tunnel.IsStillRunning())
        {
            m_tunnel.CloseTunnel();
            LaunchNewConnection();
        }
    }



    
    public RSATunneling.DebugRunningState m_runningState;
    private void Update()    
    {
        m_runningState = m_tunnel.m_runningState;
        m_tunnel.UpdateRunningStateInfo();

    }
    [ContextMenu("Launch relaunch or Connect")]
    public void LaunchRelaunchOrConnect()
    {
        bool isGood = m_tunnel.HasStarted() && m_tunnel.IsStillRunning() ;
        if (!isGood)
        {
            CloseAndRelanchConnection();
        }
    }
    [ContextMenu("Close and connect")]
    public void CloseAndRelanchConnection()
    {
        CloseCurrentConnection();
        LaunchNewConnection();
    }

    [ContextMenu("Close")]
    public void CloseCurrentConnection()
    {
        if(m_tunnel!=null)
            m_tunnel.CloseTunnel();
    }

    private void LaunchNewConnection()
    {
        WebsocketConnectionRsaTunneling c = new WebsocketConnectionRsaTunneling();
        c.SetConnectionInfo(m_serverUri, m_rsaIdentity);
        HookTunnelEventToMonoScript(c);
        c.StartConnection();
        m_tunnel = c;
    }


    private void HookTunnelEventToMonoScript(WebsocketConnectionRsaTunneling c)
    {
        c.m_trafficEvent.m_onThreadMessagePushedBinary = OnMessagePushedBinary;
        c.m_trafficEvent.m_onThreadMessagePushedText = OnMessagePushedText;
        c.m_trafficEvent.m_onThreadMessageReceivedBinary = OnMessageReceivedBinary;
        c.m_trafficEvent.m_onThreadMessageReceivedText = OnMessageReceivedText;
        c.m_trafficEvent.m_onConnectionSignedAndValidated = OnSignedAndValidated;
        c.m_trafficEvent.m_onWebsocketConnected = OnConnectionEstablished;
        c.m_trafficEvent.m_onConnectionLost = OnConnectionLost;
        c.m_trafficEvent.m_onIndexLockChanged = OnIndexChanged;
    }

    private void OnIndexChanged(int index)
    {
        if (m_trafficEvent.m_onIndexLockChanged != null)
            m_trafficEvent.m_onIndexLockChanged(index);
    }

    private void OnSignedAndValidated()
    {
        if (m_trafficEvent.m_onConnectionSignedAndValidated != null)
            m_trafficEvent.m_onConnectionSignedAndValidated();
    }

    private void OnConnectionEstablished()
    {
        if (m_trafficEvent.m_onWebsocketConnected != null)
            m_trafficEvent.m_onWebsocketConnected();
    }

    private void OnConnectionLost()
    {
        if (m_trafficEvent.m_onConnectionLost != null)
            m_trafficEvent.m_onConnectionLost();
    }

    private void OnMessageReceivedText(string message)
    {
        if (m_trafficEvent.m_onThreadMessageReceivedText != null)
            m_trafficEvent.m_onThreadMessageReceivedText(message);
    }

    private void OnMessageReceivedBinary(byte[] message)
    {
        if (m_trafficEvent.m_onThreadMessageReceivedBinary != null)
            m_trafficEvent.m_onThreadMessageReceivedBinary(message);
    }

    private void OnMessagePushedText(string message)
    {
            if (m_trafficEvent.m_onThreadMessagePushedText != null)
                m_trafficEvent.m_onThreadMessagePushedText(message);

    }

    private void OnMessagePushedBinary(byte[] message)
    {
            if (m_trafficEvent.m_onThreadMessagePushedBinary != null)
                m_trafficEvent.m_onThreadMessagePushedBinary(message);
    }


    [ContextMenu("Push Hello World")]
    public void PushHelloWorldMessage() => PushMessageText("Hello world");
    [ContextMenu("Push GUID")]
    public void PushGuid() => PushMessageText(Guid.NewGuid().ToString());

    [ContextMenu("Push Date UTC ")]
    public void PushDateTimeNowUTC() => PushMessageText(DateTime.UtcNow.ToString());


    [ContextMenu("Push Random Bytes 0 to 20")]
    public void PushRandomBytesTo20() => PushMessageBytes(GetRandomBytes(0, 20));
    [ContextMenu("Push Random Bytes 0 to 1000")]
    public void PushRandomBytesTo1023() => PushMessageBytes(GetRandomBytes(0, 1000));
    [ContextMenu("Push Random Bytes 1050 to 65000")]
    public void PushRandomBytesTo65500() => PushMessageBytes(GetRandomBytes(1050, 65000));

    private byte[] GetRandomBytes(int min, int max)
    {
        byte[] b = new byte[UnityEngine.Random.Range(min, max)];
        for (int i = 0; i < b.Length; i++)
        {
            b[i] = (byte)UnityEngine.Random.Range(0, 255);
        }
        return b;
    }

    public void PushMessageText(string textToSend)
    {
        m_tunnel.EnqueueTextMessages(textToSend);
    }
    public void PushMessageBytes(byte[] bytesToSend)
    {
        m_tunnel.EnqueueBinaryMessages(bytesToSend);
    }
    public int m_previousInteger    = 0;





    [ContextMenu("Push random integer LE")]
    public void PushRandomInteger4Bytes()
    {
        PushMessageInteger4BytesLE(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    [ContextMenu("Push random integer LE 0-100")]
    public void PushRandomInteger4BytesFrom0To100()
    {
        PushMessageInteger4BytesLE(UnityEngine.Random.Range(0, 100));
    }

    public void PushMessageInteger4BytesLE(int value)
    {      
        m_previousInteger = value;
        m_tunnel.EnqueueBinaryMessages(BitConverter.GetBytes(value));
    }


    [ContextMenu("Push random integer IID")]
    public void PushRandomIntegerIID()
    {
        PushMessageIntegerIID(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    [ContextMenu("Push random integer IID 0-100")]
    public void PushRandomIntegerIIDFrom0To100()
    {
        PushMessageIntegerIID(UnityEngine.Random.Range(0,100));
    }

    public void PushMessageIntegerIID(int value)
    {
        PushMessageIntegerIID(value, DateTime.UtcNow);
    }
    public void PushMessageIntegerIID(int value, DateTime time)
    {
        byte[] localBytes = new byte[12];
        m_previousInteger = value;
        BitConverter.GetBytes(value).CopyTo(localBytes, 0);
        BitConverter.GetBytes((ulong) (time.Ticks / TimeSpan.TicksPerMillisecond)).CopyTo(localBytes, 4);
        m_tunnel.EnqueueBinaryMessages(localBytes);
    }

    public void PushClampedBytesAsIID(byte[] bytes)
    {
        m_tunnel.PushClampedBytesAsIID(bytes);
    }
}
