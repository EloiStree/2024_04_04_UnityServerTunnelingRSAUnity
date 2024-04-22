using System;
using System.Collections;
using UnityEngine;

public class TDD_ReceivedFromConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{
    public string m_text = "";
    public byte [] m_binary ;

    public int m_intIndex = 0;
    public int m_intValue = 0;
    public ulong m_utcDateMilliseconds = 0;
    public ulong m_receivedTime = 0;
    public ulong m_sendToReceivedLag = 0;


    
    public delegate void OnIndexIntegerDateFound(int index, int value, ulong utcDateMilliseconds, ulong receivedTime);
    public OnIndexIntegerDateFound m_onIndexIntegerDateFound;

    public void PushText(string receivedText)
    {
        m_text = receivedText;
    }
    public void PushBytes(byte[] receivedBytes)
    {
        m_binary = receivedBytes;
        if(m_binary.Length == 16)
        {
            int index = BitConverter.ToInt32(receivedBytes, 0); 
            int value = BitConverter.ToInt32(receivedBytes, 4);
            ulong date = BitConverter.ToUInt64(receivedBytes,8); 
            ulong receivedTime = (ulong) DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond; 

            m_intIndex = index;
            m_intValue = value;
            m_utcDateMilliseconds = date;
            m_receivedTime = receivedTime;
            m_sendToReceivedLag = m_receivedTime - m_utcDateMilliseconds;
            
            if(m_onIndexIntegerDateFound != null)
            {
                m_onIndexIntegerDateFound(index, value, date, receivedTime);
            }
        }
    }
}
