using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SentToConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{

    public ConnectToServerTunnelingRsaMono m_connection;

    public Queue<string> m_sentToServerUTF8 = new Queue<string>();
    public Queue<byte[]> m_sentToServerBytes = new Queue<byte[]>();

    public UnityEvent<string> m_onSentMessageUTF8 = new UnityEvent<string>();
    public UnityEvent<byte[]> m_onSentMessageBytes = new UnityEvent<byte[]>();
    public void Start()
    {
        if (m_connection != null)
        {
            m_connection.m_trafficEvent.m_onThreadMessagePushedText += OnMessageReceived;
            m_connection.m_trafficEvent.m_onThreadMessagePushedBinary += OnMessageReceived;
        }
    }

    private void OnMessageReceived(string message)
    {
        m_sentToServerUTF8.Enqueue(message);
    }

    private void OnMessageReceived(byte[] message)
    {
        m_sentToServerBytes.Enqueue(message);
    }
    public bool m_catchExceptions = false;
    void Update()
    {
        while (m_sentToServerBytes.Count > 0)
        {
            if (m_catchExceptions)
            {
                try
                {
                    byte[] v = m_sentToServerBytes.Dequeue();
                    if (v != null && v.Length > 0)
                    {
                        m_onSentMessageBytes.Invoke(v);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                byte[] v = m_sentToServerBytes.Dequeue();
                if (v != null && v.Length > 0)
                {
                    m_onSentMessageBytes.Invoke(v);
                }
            }
        }
        while (m_sentToServerUTF8.Count > 0)
        {
            if (m_catchExceptions)
            {
                try
                {
                    string v = m_sentToServerUTF8.Dequeue();
                    if (v != null && v.Length > 0)
                    {
                        m_onSentMessageUTF8.Invoke(v);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                string v = m_sentToServerUTF8.Dequeue();
                if (v != null && v.Length > 0)
                {
                    m_onSentMessageUTF8.Invoke(v);
                }
            }
        }
    }
}