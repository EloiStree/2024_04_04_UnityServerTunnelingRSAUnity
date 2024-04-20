using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReceivedFromConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{

    public ConnectToRelayServerTunnelingRsaMono m_connection;

    public Queue<string> m_receivedFromServerUTF8 = new Queue<string>();
    public Queue<byte[]> m_receivedFromServerBytes = new Queue<byte[]>();

    public UnityEvent<string> m_onReceivedMessageUTF8 = new UnityEvent<string>();
    public UnityEvent<byte[]> m_onReceivedMessageBytes = new UnityEvent<byte[]>();
    public void Start()
    {
        if(m_connection != null)
        {
            m_connection.m_onThreadMessageReceivedBinary = OnMessageReceived;
            m_connection.m_onThreadMessageReceivedText = OnMessageReceived;
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
                    m_onReceivedMessageBytes.Invoke(m_receivedFromServerBytes.Dequeue());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                m_onReceivedMessageBytes.Invoke(m_receivedFromServerBytes.Dequeue());
            }
        }
        while (m_receivedFromServerUTF8.Count > 0)
        {
            if (m_catchExceptions)
            {
                try
                {
                    m_onReceivedMessageUTF8.Invoke(m_receivedFromServerUTF8.Dequeue());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                m_onReceivedMessageUTF8.Invoke(m_receivedFromServerUTF8.Dequeue());
            }
        }
    }
}
