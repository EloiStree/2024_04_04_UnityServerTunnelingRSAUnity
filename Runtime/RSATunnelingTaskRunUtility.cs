using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RSATunneling;
using static RSATunneling.WebSocketConnectionState;

public class RSATunnelingTaskRunUtility {

    public static void StartRunnningTunnel(WebsocketConnectionRsaTunneling tunnel)
    {
        if (tunnel.HasStarted())
            throw new Exception("Already started");
       Task t = Task.Run(() => ConnectAndRun(tunnel));
       tunnel.m_connection.m_runningThread = t;
    }

    public static async Task ConnectAndRun(WebsocketConnectionRsaTunneling tunnel)
    {
        tunnel.m_connection.SetLaunchState(LaunchState.Launched);
        using (ClientWebSocket ws = new ClientWebSocket())
        {
            HandshakeConnectionState handshake = tunnel.m_handshake;
            tunnel.m_connection.m_websocket= ws;
            tunnel.m_handshake.ResetToNewHandshake();
            try
            {
                string serverUri = tunnel.m_connection.m_serverUri;
                await ws.ConnectAsync(new Uri(serverUri), CancellationToken.None);

                tunnel.m_connection.m_runningListener= Task.Run(() => ReceiveMessages(tunnel));
                tunnel.m_connection.SetLaunchState(LaunchState.TaskCreated);
                bool firstOpenState=false;
                while (ws.State == WebSocketState.Open)
                {
                    if (firstOpenState == false)
                    {
                        firstOpenState = true;
                        tunnel.m_connection.SetLaunchState(LaunchState.ReadyToBeUsed);
                        tunnel.NotifyAsWebsocketInOpenState();
                    }
                    if (tunnel.IsInMustBeKillMode())
                        throw new Exception("Force close");

                    if (!handshake.m_connectionEstablishedAndVerified)
                    {
                        if (handshake.m_sentHello.Length == 0)
                        {
                            string helloToSent = "Hello " + tunnel.GetPublicXmlKey();
                            handshake.m_sentHello = helloToSent;
                            byte[] b = Encoding.UTF8.GetBytes(helloToSent);
                            await ws.SendAsync(new ArraySegment<byte>(b), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        
                    }


                    //tunnel.m_byteCountDebug.m_datetimeNow = DateTime.UtcNow.Ticks;

                    Queue<string> queueText= tunnel.m_pushInTunnel.m_toSendToTheServerUTF8;
                    while (queueText.Count > 0)
                    {
                        tunnel.m_timeDebug.m_lastPushMessageTextDate = DateTime.UtcNow.ToString();
                        if (tunnel.m_trafficEvent.m_onThreadMessagePushedText != null)
                            tunnel.m_trafficEvent.m_onThreadMessagePushedText(queueText.Peek());
                        byte[] messageBytes = Encoding.UTF8.GetBytes(queueText.Dequeue());
                        tunnel.m_byteCountDebug.m_sentTextBytesCount += messageBytes.Length;
                        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    Queue<byte[]> queueBytes= tunnel.m_pushInTunnel.m_toSendToTheServerBytes;
                    while (queueBytes.Count > 0)
                    {
                        tunnel.m_timeDebug.m_lastPushMessageBinaryDate = DateTime.UtcNow.ToString();
                        if (tunnel.m_trafficEvent.m_onThreadMessagePushedBinary != null)
                            tunnel.m_trafficEvent.m_onThreadMessagePushedBinary(queueBytes.Peek());
                        byte[] messageBytes = queueBytes.Dequeue();
                        tunnel.m_byteCountDebug.m_sentBinaryBytesCount += messageBytes.Length;
                        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                tunnel.m_connection.m_runningThreadErrorHappened = ex.Message + "\n\n" + ex.StackTrace;
                //Console.WriteLine($"WebSocket error: {ex.Message}");
            }

        }
        tunnel.NotifyAsTunnelEnded();
        
    }



    public static async Task ReceiveMessages(WebsocketConnectionRsaTunneling tunnel )
    {
        ClientWebSocket webSocket = tunnel.m_connection.m_websocket;
        HandshakeConnectionState handshake = tunnel.m_handshake;

        byte[] buffer = new byte[8096];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                if (tunnel.IsInMustBeKillMode())
                    throw new Exception("Force close");
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    tunnel.m_byteCountDebug.m_receivedTextBytesCount += result.Count;
                    tunnel.m_timeDebug.m_lastReceivedMessageTextDate = DateTime.UtcNow.ToString();
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (tunnel.m_trafficEvent.m_onThreadMessageReceivedText != null)
                        tunnel.m_trafficEvent.m_onThreadMessageReceivedText(receivedMessage);

                    if (!handshake.m_connectionEstablishedAndVerified)
                    {
                        if (receivedMessage.StartsWith("SIGNIN:"))
                        {
                            string guid = receivedMessage.Replace("SIGNIN:", "");
                            handshake.m_isConnectionValidated = true;
                            handshake.m_signInMessage = receivedMessage;
                            handshake.m_guidToSigned = guid;

                            {

                                RSA rsa = RSA.Create();

                                rsa.KeySize = 1024;
                                rsa.FromXmlString(tunnel.GetPrivateXmlKey());
                                RSAParameters privateKey = rsa.ExportParameters(true);
                                RSAParameters publicKey = rsa.ExportParameters(false);

                                string messageToSigne = handshake.m_guidToSigned;

                                byte[] data = Encoding.UTF8.GetBytes(messageToSigne);
                                byte[] signature = RSATunnelingUtility.SignData(data, privateKey);
                                var signatureBase64 = Convert.ToBase64String(signature);
                                string sent = "SIGNED:" + signatureBase64;
                                byte[] signatureBytes = Encoding.UTF8.GetBytes(sent);
                                //Console.WriteLine($"Sent message to server: {sent}");
                                //Console.WriteLine($"Sent message to server: {messageToSigne}");

                                handshake.m_signedMessage = sent;
                                handshake.m_guidSignedB64 = signatureBase64;
                                await webSocket.SendAsync(new ArraySegment<byte>(signatureBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                            }

                        }
                        else if (receivedMessage.StartsWith("RSA:Verified"))
                        {
                            handshake.m_receivedValideHankShake = receivedMessage;
                            handshake.m_connectionEstablishedAndVerified = true;
                            tunnel.NotifyAsConnectedAndVerified();
                        }
                        
                    }
                    if (string.IsNullOrWhiteSpace(handshake.m_receivedIndexLockValidation)
                        && receivedMessage.ToLower().Trim().IndexOf("indexlock:") == 0)
                    {

                        string msg = receivedMessage.Substring("indexlock:".Length);
                        handshake.m_receivedIndexLockValidation = msg;
                        if (int.TryParse(msg, out int index))
                        {
                            handshake.m_receiveGivenIndexLock = true;
                            handshake.m_givenIndexLock = index;
                        }
                        else
                        {
                            handshake.m_receiveGivenIndexLock = false;
                            handshake.m_givenIndexLock = int.MinValue;
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    tunnel.m_byteCountDebug.m_receivedBinaryBytesCount += result.Count;
                    tunnel.m_timeDebug.m_lastReceivedMessageBinaryDate = DateTime.UtcNow.ToString();
                    byte[] receivedMessage = new byte[result.Count];
                    Array.Copy(buffer, receivedMessage, result.Count);
                    if (tunnel.m_trafficEvent. m_onThreadMessageReceivedBinary != null)
                        tunnel.m_trafficEvent.m_onThreadMessageReceivedBinary(receivedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            tunnel.m_connection.m_runningListenerErrorHappened = ex.Message + "\n\n" + ex.StackTrace;

        }
    }
}
