using System;
using UnityEngine;
using UnityEngine.Events;

public class IID16BytesToUnityEventMono : MonoBehaviour
{
    public byte[] m_binary;

    public int m_intIndex = 0;
    public int m_intValue = 0;
    public ulong m_utcDateMilliseconds = 0;
    public ulong m_receivedTime = 0;
    public ulong m_sendToReceivedLag = 0;

    public UnityEvent<int> m_onValueFound;
    public UnityEvent<int,int> m_onIndexValueFound;
    public UnityEvent<int,int,ulong> m_onIndexValueDateFound;
    public UnityEvent<int,int,ulong,ulong> m_onIndexValueDateFoundWithReceivedTime;

    public delegate void OnIndexIntegerDateFound(int index, int value, ulong utcDateMilliseconds, ulong receivedTime);
    public OnIndexIntegerDateFound m_onIndexIntegerDateFound;

    public void PushBytes(byte[] receivedBytes)
    {
        if (receivedBytes!=null && receivedBytes.Length == 16)
        {
            m_binary = receivedBytes;
            int index = BitConverter.ToInt32(receivedBytes, 0);
            int value = BitConverter.ToInt32(receivedBytes, 4);
            ulong date = BitConverter.ToUInt64(receivedBytes, 8);
            ulong receivedTime = (ulong)DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            m_intIndex = index;
            m_intValue = value;
            m_utcDateMilliseconds = date;
            m_receivedTime = receivedTime;
            m_sendToReceivedLag = m_receivedTime - m_utcDateMilliseconds;


            m_onValueFound.Invoke(value);
            m_onIndexValueFound.Invoke(index, value);
            m_onIndexValueDateFound.Invoke(index,value, date);
            m_onIndexValueDateFoundWithReceivedTime.Invoke(index, value, date, receivedTime);

            if (m_onIndexIntegerDateFound != null)
            {
                m_onIndexIntegerDateFound(index, value, date, receivedTime);
            }
        }
    }
}