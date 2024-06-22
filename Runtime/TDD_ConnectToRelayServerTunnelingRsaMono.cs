using System;
using System.Collections;
using UnityEngine;

public class TDD_ConnectToRelayServerTunnelingRsaMono : MonoBehaviour
{
    public ConnectToServerTunnelingRsaMono m_connection;

    public string m_time = "";

    public int [] m_intValue = new int[] { 0,42,69,2501,314,31418};
    public ulong m_utcDateMilliseconds = 0;
    public float m_timeWaiting=0.5f;
    void Start()
    {
        StartCoroutine(ConnectAndRun());
    }

    public int m_currentValue = 0;
    public int m_previousValue = 0;
    IEnumerator ConnectAndRun()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            yield return new WaitForSeconds(m_timeWaiting);
            m_time = System.DateTime.UtcNow.ToString();
            if (m_connection.m_tunnel.IsConnectedAndHandShakeVerified()) {

                //m_connection.PushMessageText("Time Client: " + System.DateTime.UtcNow.ToString());
                yield return new WaitForSeconds(m_timeWaiting);
                //generate 32 bytes of random data
                byte[] randomData = new byte[12];
                
                m_utcDateMilliseconds = (ulong)DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                m_previousValue = m_currentValue;
                m_currentValue = GetRandomInteger();
                if (m_currentValue == m_previousValue)
                    m_currentValue += 1;
                BitConverter.GetBytes(m_currentValue).CopyTo(randomData, 0);
                BitConverter.GetBytes(m_utcDateMilliseconds).CopyTo(randomData, 4);

                m_connection.PushMessageBytes(randomData);
                yield return new WaitForSeconds(m_timeWaiting);

            }
        }
        
    }

    private int GetRandomInteger()
    {
        if(m_intValue.Length > 0)
            return m_intValue[UnityEngine.Random.Range(0, m_intValue.Length)];
        return 0;
    }
}
