using System;
using System.Net.WebSockets;
using UnityEngine;
using static RSATunneling.WebSocketConnectionState;

public partial class RSATunneling
{
    [System.Serializable]
    public class WebsocketConnectionRsaTunneling
    {
        public RsaKeyPair m_useKeyPair= new ();
        public WebSocketConnectionState m_connection=new();
        public HandshakeConnectionState m_handshake=new ();

        [Header("Debuggers")]
        public DebugTime m_timeDebug = new();
        public DebugByteCount m_byteCountDebug = new();
        public DebugRunningState m_runningState = new();

        public TraffictInEvent m_trafficEvent = new();
        public TrafficOutQueue m_pushInTunnel = new();


         ~WebsocketConnectionRsaTunneling()
        {
            CloseTunnel();
        }

        public bool HasServerAnsweredToHelloHandshake()
        {
            return
                m_handshake!=null 
                && m_handshake.m_sentHello!=null
                && m_handshake.m_sentHello.Length>0 
                && m_handshake.m_signInMessage!=null 
                && m_handshake.m_signInMessage.Length>0;       
        }

        public void CloseTunnel()
        {
            m_connection.KillIt();
        }

        public bool IsStillRunning()
        {
            return m_connection.IsStillRunning();
        }

        public void UpdateRunningStateInfo()
        {
            m_runningState.isRunningHandleTask = m_connection.m_runningThread != null && !m_connection.m_runningThread.IsCompleted;
            m_runningState.isRunningListener = m_connection.m_runningListener != null && !m_connection.m_runningListener.IsCompleted;
            m_runningState.isWebsocketConnected = m_connection.m_websocket != null;
            if (m_connection.m_websocket != null)
                m_runningState.m_websocketState = m_connection.m_websocket.State;
            else m_runningState.m_websocketState = WebSocketState.None;
            m_runningState.isStillRunning = m_connection.IsStillRunning();
        }

        public void SetConnectionInfo(string serverURI, string publicKeyXml, string privateKeyXml)
        {
            m_useKeyPair.m_publicKey = publicKeyXml;
            m_useKeyPair.m_hiddenPrivateKey.m_privateKey = privateKeyXml;
            SetConnectionInfo(serverURI, m_useKeyPair);
        }
        public void SetConnectionInfo(string serverURI, string privateKeyXml)
        {
            string publicKey = RSATunnelingUtility.GetPublicKeyFromPrivateKey(privateKeyXml);
            SetConnectionInfo(serverURI, publicKey, privateKeyXml);
            
        }
        public void SetConnectionInfo(string serverURI, RsaKeyPair rsaIdentity)
        {
            m_useKeyPair = rsaIdentity;
            m_connection.m_serverUri= serverURI; 
        }

        public void StartConnection()
        {
            if(!HasStarted())
                RSATunnelingTaskRunUtility.StartRunnningTunnel(this);

        }
       

        public bool HasStarted()
        {
            LaunchState state = m_connection.m_launchState;
            return state != LaunchState.WaitingToBeLaunched;
        }
        public bool IsReadyToBeUsed() {

            LaunchState state = m_connection.m_launchState;
            return state == LaunchState.ReadyToBeUsed;
        }

        public void EnqueueTextMessages(string textToSend)
        {
            m_pushInTunnel.m_toSendToTheServerUTF8.Enqueue(textToSend);
            
        }

        public void EnqueueBinaryMessages(byte[] bytesToSend)
        {
            m_pushInTunnel.m_toSendToTheServerBytes.Enqueue(bytesToSend);
        }

        public string GetPublicXmlKey()
        {
            return m_useKeyPair.m_publicKey;
        }

        public string GetPrivateXmlKey()
        {
            return m_useKeyPair.m_hiddenPrivateKey.m_privateKey; 

        }

        public bool IsInMustBeKillMode()
        {
            return m_connection.m_killSwitch.m_needToBeKilled;
        }

        public bool IsConnectedAndHandShakeVerified()
        {
            return m_handshake.m_connectionEstablishedAndVerified;
        }

        public void NotifyAsTunnelEnded()
        {
            if (m_trafficEvent.m_onConnectionLost != null)
            m_trafficEvent.m_onConnectionLost.Invoke();
        }

        public void NotifyAsConnectedAndVerified()
        {
            if (m_trafficEvent.m_onConnectionSignedAndValidated != null)
            m_trafficEvent.m_onConnectionSignedAndValidated.Invoke();
        }

        public void NotifyAsWebsocketInOpenState()
        {
            if (m_trafficEvent.m_onWebsocketConnected != null)
            m_trafficEvent.m_onWebsocketConnected.Invoke();
        }

        public void EnqueueInteger(int integerValue)
        {
            byte[] iBytes = BitConverter.GetBytes(integerValue);
            EnqueueBinaryMessages(iBytes);
        }

        public void PushClampedBytesAsIID(byte[] bytes)
        {
            if (bytes == null) return;

            if (bytes.Length == 4 || bytes.Length == 12 || bytes.Length == 16) { 
                EnqueueBinaryMessages(bytes);
            }
            else if (bytes.Length > 16)
            {
                byte[] b = new byte[16];
                Array.Copy(bytes, 0, b, 0, 16);
                EnqueueBinaryMessages(b);
            }
        }
    }
}
