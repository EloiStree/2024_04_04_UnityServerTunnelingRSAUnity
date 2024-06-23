using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FirstBytesFilteringMono : MonoBehaviour
{

    public ushort m_maxBytesLengthGlobal = 1024;

    public BytesFilterRange[] m_filters;
    
    [System.Serializable]
    public class BytesFilterRange { 
    
        public byte m_startByte;
        public ushort m_minLength=4;
        public ushort m_maxLength= 1024;
        public byte[] m_lastReceived;
        public UnityEvent<byte[]> m_onBytesReceived;
    }


    public void PushBytesArrayIn(byte[] bytes)
    {
        if(bytes.Length > m_maxBytesLengthGlobal) return;

        for (int i = 0; i < m_filters.Length; i++)
        {
            if (bytes.Length>0 &&  bytes[0] != m_filters[i].m_startByte) continue;
            if (bytes.Length < m_filters[i].m_minLength) continue;
            if (bytes.Length > m_filters[i].m_maxLength) continue;
            m_filters[i].m_lastReceived = bytes;
            m_filters[i].m_onBytesReceived.Invoke(bytes);
        }
    }
}
