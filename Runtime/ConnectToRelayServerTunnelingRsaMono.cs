using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{

    public string m_serverUri = "ws://81.240.94.97:3615";
    [TextArea(0, 2)]
    public string m_privateKey = "<RSAKeyValue><Modulus>vP7yDAkjkLrO7zqlaOlVpi3h7knD2xU4voEj3w9aJ9Pm/J0WADOOpnGcBc25VI7yuZuJZjsLuK9dz6aFVQR2+ZpT7H1aD/7qgXG10eIrOSu41ZIpcO26VDFcfsX1as7kmAQmLqFFTzcL2Yzv5Vz3982QeFy5Sx4MIRa26fbrKOE=</Modulus><Exponent>AQAB</Exponent><P>x5+b84t6DU7dmRnZbg6nK5eLyGseIyDVodarQ8f7C4kCTfgYG7WW89X1cU//jMsj3mjQntOjJF2BkhtX/HWO0w==</P><Q>8l77YEBBJiLo6yuFDZLWRyjYJsEvuE3/MQvSwXtY2Hb7BM+ynhIcncs6jGmUuSSNoXhQ877CeD2sOJbGV+Ng+w==</Q><DP>J98nZRO8wx+3fzb8iNEAbuKMFvHeSSHrybF478bny7wH687b8dzpU7aumX1jC5ofhfLliHO5KDBNCwPPJSvN5Q==</DP><DQ>OzKVxUmMYAswxpfHlKwjqBfCy5xt0l9CkDEqFdXRunU9FEzCfLdBxAyqTTdQevQBn8mqRA54ozO1B9FTuo2v1w==</DQ><InverseQ>K+5TNsF1zM4SeFX8Pd7OcsB3yYP0VkCCawyeQxjm3GQbQd805JnqCoaAnAiuM5N49jonQXuJMjYqgxT0JWh2VA==</InverseQ><D>oJ3J9pCNuSIJWyXsDQy/zUqRB4GJAVc3si7t3VOeutpLI8QcPm+Se8FxZz0+k64oebTFQCxN+daPUzmhdm8k6+OqoYV/gHCrWbEQMAKkavT3rxtlJbkWkFgqNxmMQA2/2feC0ESbavtZemBLOP7p+VVr/cYu6DzpUNr5+FVhD0E=</D></RSAKeyValue>";
    [TextArea(0, 10)]
    public string m_publicKey = "<RSAKeyValue><Modulus>vP7yDAkjkLrO7zqlaOlVpi3h7knD2xU4voEj3w9aJ9Pm/J0WADOOpnGcBc25VI7yuZuJZjsLuK9dz6aFVQR2+ZpT7H1aD/7qgXG10eIrOSu41ZIpcO26VDFcfsX1as7kmAQmLqFFTzcL2Yzv5Vz3982QeFy5Sx4MIRa26fbrKOE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    

    public delegate void OnMessageReceivedText(string message);
    public delegate void OnMessageReceivedBinary(byte[] message);


    public OnMessageReceivedText    m_onThreadMessageReceivedText;
    public OnMessageReceivedBinary  m_onThreadMessageReceivedBinary;

    public Queue<string> m_toSendToTheServerUTF8 = new Queue<string>();
    public Queue<byte[]> m_toSendToTheServerBytes = new Queue<byte[]>();
    public ClientWebSocket m_connectionEstablished;
    public bool m_isConnectionValidated;
    public string m_lastPushedMessage = "";
    public byte[] m_lastPushedMessageAsByte;

    public string m_messageToSignedReceived = "";
    public bool m_connectionEstablishedAndVerified = false;
    public string m_lastMessageReceived = "";
    public byte[] m_lastMessageReceivedAsByte;
    public byte[] buffer = new byte[600];

    public string m_lastReceivedMessageTextDate = "";
    public string m_lastReceivedMessageBinaryDate = "";
    public string m_lastPushMessageTextDate = "";
    public string m_lastPushMessageBinaryDate = "";


    public bool m_autoStart = true;
    public bool m_autoReconnect = true;

    private void ResetAllValue()
    {
       
        m_toSendToTheServerUTF8.Clear();
        m_toSendToTheServerBytes.Clear();
        m_connectionEstablished=null;
        m_isConnectionValidated=false;
        m_lastPushedMessage = "";
        m_lastPushedMessageAsByte= new byte[0];

        m_messageToSignedReceived = "";
        m_connectionEstablishedAndVerified = false;
        m_lastMessageReceived = "";
        m_lastMessageReceivedAsByte = new byte[0];
        buffer = new byte[600];

}

    public void SetPublicKeyXml(string xmlKey)
    {
        m_publicKey = xmlKey;
    }
    public void SetPrivateKeyXml(string xmlKey)
    {
        m_privateKey = xmlKey;
    }

    public void SetWebsocketUrlWS(string url) {
        m_serverUri = url;
    }
    private void OnDestroy()
    {
       
        if (m_connectionEstablished != null)
        {
            try {
                if (m_previousSocketStop != null)
                {
                    m_previousSocketStop.m_stopLoop = true;
                }
                if(m_connectionEstablished!=null)
                    m_connectionEstablished.Abort();
                if(m_connectionEstablished !=null)
                    m_connectionEstablished.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace +" "+e.Message) ;
            }
        }
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
    }

    public void TryToLaunchOrRelaunchClient() {

        CheckConnectionState();
        if (m_autoReconnect)
            InvokeRepeating("CheckConnectionState", 0, 5);
    }


    Task m_running;
    public void CheckConnectionState()
    {
        if (m_connectionEstablished == null)
        {
            if (m_previousSocketStop != null)
            {
                m_previousSocketStop.m_stopLoop = true;
            }
            m_previousSocketStop = new StopLoop();
            m_running = Task.Run(() => ConnectAndRun(m_previousSocketStop));
        }

    }
    [ContextMenu("Close and connect")]
    public void CloseAndRelanchConnection()
    {
        if (m_previousSocketStop!=null) {
            m_previousSocketStop.m_stopLoop = true;
        }
       
        if (m_connectionEstablished == null)
        {
            m_previousSocketStop = new StopLoop();
            m_running = Task.Run(() => ConnectAndRun(m_previousSocketStop));
        }
       

    }


    public static byte[] SignData(byte[] data, RSAParameters privateKey)
    {

        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(privateKey);
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
    public static bool VerifySignature(byte[] data, byte[] signature, RSAParameters publicKey)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    public class StopLoop {
        public bool m_stopLoop = false;
    }
    ClientWebSocket m_websocket;
    StopLoop m_previousSocketStop;
    public async Task ConnectAndRun(StopLoop stop )
    {
        Debug.Log("Connect and run");
        ResetAllValue();
        using ( m_websocket = new ClientWebSocket())
        {
            
            m_isConnectionValidated = false;
            m_connectionEstablished = m_websocket;
            try
            {
                m_messageToSignedReceived = "";
                m_connectionEstablishedAndVerified = false;
                Console.WriteLine($"Connecting to server: {m_serverUri}");
                await m_websocket.ConnectAsync(new Uri(m_serverUri), CancellationToken.None);

                Task.Run(() => ReceiveMessages(m_websocket));





                while (m_websocket.State == WebSocketState.Open)
                {
                    if (stop.m_stopLoop)
                        throw new Exception("Force close");

                    if (!m_connectionEstablishedAndVerified)
                    {
                        byte[] b = Encoding.UTF8.GetBytes("Hello "+m_publicKey);
                        await m_websocket.SendAsync(new ArraySegment<byte>(b), WebSocketMessageType.Text, true, CancellationToken.None);

                        while (m_messageToSignedReceived.Length == 0)
                        {
                            await Task.Delay(20);
                        }



                        RSA rsa = RSA.Create();

                        rsa.KeySize = 1024;
                        rsa.FromXmlString(m_privateKey);
                        RSAParameters privateKey = rsa.ExportParameters(true);
                        RSAParameters publicKey = rsa.ExportParameters(false);

                        string messageToSigne = m_messageToSignedReceived;
                        byte[] data = Encoding.UTF8.GetBytes(messageToSigne);
                        byte[] signature = SignData(data, privateKey);
                        var signatureBase64 = Convert.ToBase64String(signature);
                        string sent ="SIGNED:"+  signatureBase64;
                        byte[] signatureBytes = Encoding.UTF8.GetBytes(sent);
                        Console.WriteLine($"Sent message to server: {sent}");
                        Console.WriteLine($"Sent message to server: {messageToSigne}");
                        await m_websocket.SendAsync(new ArraySegment<byte>(signatureBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        // create a signe message
                        byte[] bb = Encoding.UTF8.GetBytes(messageToSigne);
                        await m_websocket.SendAsync(new ArraySegment<byte>(bb), WebSocketMessageType.Text, true, CancellationToken.None);

                        while (!m_connectionEstablishedAndVerified && m_websocket.State == WebSocketState.Open)
                        {
                            if (stop.m_stopLoop)
                                throw new Exception("Force clsoe");
                            await Task.Delay(20);
                        }
                        m_isConnectionValidated = true;
                    }



                    while (m_toSendToTheServerUTF8.Count > 0)
                    {
                        m_lastPushMessageTextDate = DateTime.UtcNow.ToString();
                        Console.WriteLine($"Sending message to server: {m_toSendToTheServerUTF8.Peek()}");
                            m_lastPushedMessage= m_toSendToTheServerUTF8.Peek();
                        byte[] messageBytes = Encoding.UTF8.GetBytes(m_toSendToTheServerUTF8.Dequeue());
                        await m_websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    while (m_toSendToTheServerBytes.Count > 0)
                    {
                        m_lastPushMessageBinaryDate = DateTime.UtcNow.ToString();
                        Console.WriteLine($"Sending message to server: {m_toSendToTheServerBytes.Peek()}");
                            m_lastMessageReceivedAsByte = m_toSendToTheServerBytes.Peek();
                        byte[] messageBytes = m_toSendToTheServerBytes.Dequeue();
                        await m_websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                    m_isConnectionValidated = false;
                    m_connectionEstablished = null;
                Console.WriteLine($"WebSocket error: {ex.Message}");
                Console.WriteLine("Reconnecting in 5 seconds...");
                await Task.Delay(5000);
            }

        }
        m_isConnectionValidated = false;
        m_connectionEstablished = null;
    }

   

    public void PushMessageText(string textToSend)
    {
        m_toSendToTheServerUTF8.Enqueue(textToSend);
    }
    public void PushMessageBytes(byte[] bytesToSend)
    {
        m_toSendToTheServerBytes.Enqueue(bytesToSend);
    }
    public int m_previousInteger    = 0;

    [ContextMenu("Push random integer")]
    public void PushRandomInteger()
    {
        PushMessageInteger(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    [ContextMenu("Push random integer 0-100")]
    public void PushRandomIntegerFrom0To100()
    {
        PushMessageInteger(UnityEngine.Random.Range(0,100));
    }

    public void PushMessageInteger(int value)
    {
       PushMessageInteger(value, DateTime.UtcNow);
    }
    public void PushMessageInteger(int value, DateTime time)
    {
        if (m_previousInteger == value)
            return;
        byte[] localBytes = new byte[12];
        m_previousInteger = value;
        BitConverter.GetBytes(value).CopyTo(localBytes, 0);
        BitConverter.GetBytes((ulong) (time.Ticks / TimeSpan.TicksPerMillisecond)).CopyTo(localBytes, 4);
        m_toSendToTheServerBytes.Enqueue(localBytes);
    }

    private async Task ReceiveMessages(ClientWebSocket webSocket)
{

    try
    {
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {

                    m_lastReceivedMessageTextDate = DateTime.UtcNow.ToString();
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    m_lastMessageReceived = receivedMessage;
                    if(m_onThreadMessageReceivedText!=null)
                       m_onThreadMessageReceivedText(receivedMessage);
                if (!m_connectionEstablishedAndVerified)
                {
                    if (receivedMessage.Contains("SIGNIN:"))
                    {
                        m_messageToSignedReceived = receivedMessage.Replace("SIGNIN:", "");
                        m_isConnectionValidated = true;
                    }
                    if (receivedMessage.Contains("RSA:Verified"))
                    {
                        m_connectionEstablishedAndVerified = true;
                    }
                }
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    m_lastReceivedMessageBinaryDate = DateTime.UtcNow.ToString();

                    byte[] receivedMessage = new byte[result.Count];
                    Array.Copy(buffer, receivedMessage, result.Count);
                    m_lastMessageReceivedAsByte = receivedMessage;
                    if(m_onThreadMessageReceivedBinary!=null)
                        m_onThreadMessageReceivedBinary(receivedMessage);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WebSocket error: {ex.Message}");

        // Handle reconnection logic
        Console.WriteLine("Reconnecting in 5 seconds...");
        await Task.Delay(5000);
    }
}




}


