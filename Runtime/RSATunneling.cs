using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using UnityEngine;

public partial class RSATunneling
{
    [System.Serializable]
    public class TrafficOutQueue
    {
        /// QUEUE
        public Queue<string> m_toSendToTheServerUTF8 = new Queue<string>();
        public Queue<byte[]> m_toSendToTheServerBytes = new Queue<byte[]>();
    }
    [System.Serializable]

    public class TraffictInEvent
    {
        public delegate void OnMessageReceivedText(string message);
        public delegate void OnMessageReceivedBinary(byte[] message);
        public OnMessageReceivedText m_onThreadMessageReceivedText = null;
        public OnMessageReceivedBinary m_onThreadMessageReceivedBinary = null;
        public delegate void OnMessagePushedText(string message);
        public delegate void OnMessagePushedBinary(byte[] message);
        public OnMessagePushedText m_onThreadMessagePushedText = null;
        public OnMessagePushedBinary m_onThreadMessagePushedBinary = null;

        public Action m_onWebsocketConnected;
        public Action m_onConnectionSignedAndValidated;
        public Action m_onConnectionLost;
        public Action<int> m_onIndexLockChanged;
    }


    [System.Serializable]
    public class StopLoop
    {
        public bool m_needToBeKilled = false;
    }


    [System.Serializable]
    public class WebSocketConnectionState
    {
        public enum LaunchState { 
           WaitingToBeLaunched, Created, Launched, Creating, TaskCreated, ReadyToBeUsed
        }
        public LaunchState m_launchState = LaunchState.WaitingToBeLaunched;
        public StopLoop m_killSwitch=new StopLoop();
        public Task m_runningThread=null;
        public ClientWebSocket m_websocket=null;
        public Task m_runningListener = null;
        public string m_serverUri="";
        public string m_runningThreadErrorHappened="";
        public string m_runningListenerErrorHappened="";

        public void KillIt()
        {
            if (m_killSwitch != null)
                m_killSwitch.m_needToBeKilled = true;
            if (m_websocket != null)
            {
                m_websocket.Abort();
                m_websocket.Dispose();
            }
            if (m_runningThread != null)
            {
                
                if(m_runningThread.IsCompleted)
                    m_runningThread.Dispose();
            }
            if (m_runningListener != null)
            {
                if (m_runningThread.IsCompleted)
                    m_runningListener.Dispose();
            }
            // Missing some dispose when closed later.
        }

        public bool IsStillRunning()
        {
            return (
                       (m_runningThread != null && !m_runningThread.IsCompleted)
                    && (m_runningListener != null && !m_runningListener.IsCompleted)
                    && m_websocket != null
                    && (
                    m_websocket.State == WebSocketState.Open ||
                    m_websocket.State == WebSocketState.Connecting
                    )
                );
        }

        public void SetLaunchState(LaunchState launched)
        {
            m_launchState = launched; 
        }
    }


    [System.Serializable]
    public class HandshakeConnectionState
    {
        [Tooltip("Say Hello to the server with public key RSA")]
        public string m_sentHello="";
        [Tooltip("Wait the server to send a GUID id to sign with SIGNIN:GUID")]
        public string m_signInMessage = "";
        [Tooltip("The GUID received to signed as bytes and send back")]
        public string m_guidToSigned = "";
        [Tooltip("GUID signed with private key as B64 text value")]
        public string m_guidSignedB64 = "";
        [Tooltip("Signed message ready to be sent back")]
        public string m_signedMessage;
        [Tooltip("Server recevied out signed GUID and validate the connection")]
        public string m_receivedValideHankShake = "";
        [Tooltip("Server identify the connection as integer index (negative: means guest on the server, positive: means a claimed registered index")]
        public string m_receivedIndexLockValidation = "";
        [Tooltip("Is connection with server exists as a websocket?")]
        public bool m_isConnectionValidated = false;
        [Tooltip("Is connected was established and the handshake verified")]
        public bool m_connectionEstablishedAndVerified=false;
        [Tooltip("Is server return us a index integer lock ?")]
        public bool m_receiveGivenIndexLock = false;
        [Tooltip("What is our given index locked by the server for this public rsa key ?")]
        public int m_givenIndexLock=int.MinValue;

        public void ResetToNewHandshake()
        {

            m_sentHello = "";
            m_signInMessage = "";
            m_guidToSigned = "";
            m_guidSignedB64 = "";
            m_signedMessage = "";
            m_receivedValideHankShake = "";
            m_receivedIndexLockValidation = "";
            m_isConnectionValidated = false;
            m_connectionEstablishedAndVerified = false;
            m_receiveGivenIndexLock = false;
            m_givenIndexLock = 0;

        }
    }

    [System.Serializable]
    public class DebugRunningState
    {
        [Header("Running State")]
        public bool isStillRunning = false;
        public bool isRunningHandleTask = false;
        public bool isRunningListener=false;
        public bool isWebsocketConnected = false;
        public WebSocketState m_websocketState = WebSocketState.None;
    }
    [System.Serializable]
    public class DebugByteCount
    {

        [Header("Byte count")]
        public long m_receivedBinaryBytesCount = 0;
        public long m_receivedTextBytesCount = 0;
        public long m_sentBinaryBytesCount = 0;
        public long m_sentTextBytesCount = 0;
        //public long m_datetimeNow = 0;
    }

    [System.Serializable]
    public class DebugTime
    {
        [Header("Time")]
        public string m_lastReceivedMessageTextDate = "";
        public string m_lastReceivedMessageBinaryDate = "";
        public string m_lastPushMessageTextDate = "";
        public string m_lastPushMessageBinaryDate = "";

        public void ResetToNew()
        {
            m_lastReceivedMessageTextDate = "";
            m_lastReceivedMessageBinaryDate = "";
            m_lastPushMessageTextDate = "";
            m_lastPushMessageBinaryDate = "";
        }
    }

    [System.Serializable]
    public class RsaKeyPair
    {
        [TextArea(0, 10)]
        public string m_publicKey="";

        public PrivateKey m_hiddenPrivateKey= new PrivateKey();

        [System.Serializable]
        public class PrivateKey
        {
            [TextArea(0, 2)]
            public string m_privateKey="";
        }
    }
}
