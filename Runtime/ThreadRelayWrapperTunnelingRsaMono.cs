using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// This class need to be a bit clean;
/// </summary>
public class ThreadRelayWrapperTunnelingRsaMono : MonoBehaviour
{


    public ConnectToServerTunnelingRsaMono m_toAffect;
    public bool m_useUdpListener=true;
    public bool m_useUdpBroadcaster=true;
    public bool m_useUnityEvent = true;
    public UnityEvent<byte[]> m_onReceivedUnityThread;
    public UnityEvent<byte[]> m_onSentUnityThread;



    [Header("Port and Target")]
    public int m_listenPort = 3617;
    public List<string> m_targetAddresses = new List<string>();
    public List<int> m_targetPorts = new List<int>();
    public float m_startWaitToSetTargets = 0.1f;



    [Header("Other")]

    public KeepAlive m_keepAlive;
    [System.Serializable]
    public class KeepAlive
    {

        public bool m_keepAlive = true;
        public long m_listenerTick = 0;
        public long m_pusherTick = 0;
    }



   


     List<IPEndPoint> m_endPoints = new List<IPEndPoint>();
     UdpClient c = new UdpClient();

     Queue<byte[]> m_unityThreadReceived = new();
     Queue<byte[]> m_unityThreadToSend = new();


    public void Update()
    {
        if (m_useUnityEvent) { 
            while (m_unityThreadReceived.Count > 0)
            {
                m_onReceivedUnityThread.Invoke(m_unityThreadReceived.Dequeue());
            }
            while (m_unityThreadToSend.Count > 0)
            {
                m_onSentUnityThread.Invoke(m_unityThreadToSend.Dequeue());
            }
        }
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(m_startWaitToSetTargets);
        c = new UdpClient();
        m_endPoints = new List<IPEndPoint>();
        foreach (var item in m_targetAddresses)
        {
            if (item == null) continue;
            if (item.IndexOf(':') <= 7) continue;
            string[] tokens = item.Split(":");
            if (tokens.Length != 2)
                continue;
            if (int.TryParse(tokens[1], out int port))
            {

                m_endPoints.Add(new IPEndPoint(IPAddress.Parse(tokens[0].Trim()), port));
            }
        }
        foreach (var p in m_targetPorts)
        {
            m_endPoints.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), p));
        }

        m_keepAlive = new KeepAlive() { m_keepAlive = true };


        if (m_useUdpBroadcaster) { 
            m_toAffect.m_trafficEvent.m_onThreadMessageReceivedBinary += ((b) =>
            {
                foreach (var item in m_endPoints)
                {
                    if (item != null && c != null && b != null)
                        c.Send(b, b.Length, item);
                }
                m_keepAlive.m_pusherTick = DateTime.Now.Ticks;
                if (m_useUnityEvent) { 
                
                    if (b != null && b.Length > 0)
                    {
                        byte[] b2 = new byte[b.Length];
                        b.CopyTo(b2, 0);
                        m_unityThreadReceived.Enqueue(b2);
                    }
                }
            });
        }

        if (m_useUdpListener) { 
            m_listener = new Thread(() => ListenToPort(m_listenPort, m_keepAlive, PushBytesToTunnel));
            m_listener.Priority = System.Threading.ThreadPriority.AboveNormal;
            m_listener.Start();
        }


    }
    private Thread m_listener = null;


    public void PushBytesToTunnel(byte[] b)
    {

        m_toAffect.PushMessageBytes(b);
        if (m_useUnityEvent)
        {

            if (b != null && b.Length > 0)
            {
                byte[] b2 = new byte[b.Length];
                b.CopyTo(b2, 0);
                m_unityThreadToSend.Enqueue(b2);
            }
        }
    }


    private void OnDisable()
    {
        m_keepAlive.m_keepAlive = false;
    }
    void OnApplicationQuit()
    {

        m_keepAlive.m_keepAlive = false;
    }

    private void OnDestroy()
    {
        m_keepAlive.m_keepAlive = false;
    }




    public void SetPortListener(string port) { int.TryParse(port, out m_listenPort); }
    public void SetPortListener(int port) { m_listenPort = port; }

    public void SetTargetsFromText(string text)
    {
        string[] tokens = text.Split(new char[] { '\n', ',', '|' });
        SetTargetsFromStringArray(tokens);
    }
  
    public void SetUniqueTarget(string target)
    {
        SetTargetsFromStringArray(new string[] { target });
    }
    public void SetUniqueTarget(string ivp4, int port)
    {
       
        SetTargetsFromStringArray(new string[] { ivp4 + ":" + port });
    }

    public void SetTargetsFromStringArray(IEnumerable<string> targets)
    {
        SetTargetsFromStringArray(targets.ToArray());
    }


    public void SetTargetsFromStringArray(string[] targets)
    {
        m_targetAddresses.Clear();
        m_targetPorts.Clear();
        foreach (var item in targets)
        {
            string t = item.Trim();
            if (t != null && t.Length > 0)
            {

                if (int.TryParse(t, out int port))
                {
                    if (port > 0 && port < 65535)
                        m_targetPorts.Add(port);
                }
                else
                {

                    if (t.IndexOf(":") > 7)
                    {
                        m_targetAddresses.Add(t);
                    }
                }
            }

        }
    }



    public static void ListenToPort(int port, KeepAlive keepAlive, Action<byte[]> received)
    {
        keepAlive.m_listenerTick = DateTime.Now.Ticks;
        UdpClient c = new UdpClient();
        IPEndPoint e = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);

        using (UdpClient udpClient = new UdpClient(port))
        {
            // Set the client to listen to any IP address on the specified port
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);


            while (keepAlive.m_keepAlive)
            {
                // Receive a UDP packet asynchronously
                UdpReceiveResult receiveResult = udpClient.ReceiveAsync().Result;

                // Convert the received data into a string
                byte[] b = receiveResult.Buffer;
                if (b != null && b.Length > 0 && received != null)
                {
                    received(b);
                }

                keepAlive.m_listenerTick = DateTime.Now.Ticks;

            }
        }


    }
}
