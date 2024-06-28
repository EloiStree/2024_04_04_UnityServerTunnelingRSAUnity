using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReceivedFromConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{

    public ConnectToServerTunnelingRsaMono m_connection;

    public Queue<string> m_receivedFromServerUTF8 = new Queue<string>();
    public Queue<byte[]> m_receivedFromServerBytes = new Queue<byte[]>();

    public UnityEvent<string> m_onReceivedMessageUTF8 = new UnityEvent<string>();
    public UnityEvent<byte[]> m_onReceivedMessageBytes = new UnityEvent<byte[]>();
    public void Start()
    {
        if (m_connection != null)
        {
            m_connection.m_trafficEvent.m_onThreadMessageReceivedBinary += OnMessageReceived;
            m_connection.m_trafficEvent.m_onThreadMessageReceivedText += OnMessageReceived;
        }
    }

    private void OnMessageReceived(string message)
    {
        m_receivedFromServerUTF8.Enqueue(message);
    }

    private void OnMessageReceived(byte[] message)
    {
        m_receivedFromServerBytes.Enqueue(message);
    }
    public bool m_catchExceptions = false;
    void Update()
    {
        while (m_receivedFromServerBytes.Count > 0)
        {
            if (m_catchExceptions)
            {
                try
                {
                    byte[] v = m_receivedFromServerBytes.Dequeue();
                    if (v != null && v.Length > 0)
                    {
                        m_onReceivedMessageBytes.Invoke(v);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                byte[] v = m_receivedFromServerBytes.Dequeue();
                if (v != null && v.Length > 0)
                {
                    m_onReceivedMessageBytes.Invoke(v);
                }
            }
        }
        while (m_receivedFromServerUTF8.Count > 0)
        {
            if (m_catchExceptions)
            {
                try
                {
                    string v = m_receivedFromServerUTF8.Dequeue();
                    if (v != null && v.Length > 0)
                    {
                        m_onReceivedMessageUTF8.Invoke(v);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                string v = m_receivedFromServerUTF8.Dequeue();
                if (v != null && v.Length > 0)
                {
                    m_onReceivedMessageUTF8.Invoke(v);
                }
            }
        }
    }
}
