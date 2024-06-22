using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RSATunneling;

public class ConnectArrayToServerTunnelingRsaMono : MonoBehaviour
{


    public string m_serverUri = "ws://81.240.94.97:4501";
    public string[] m_privateKeys= new string[12];

    public List<WebsocketConnectionRsaTunneling> m_tunnels;


    public IEnumerator Start()
    {
        m_tunnels = new List<WebsocketConnectionRsaTunneling>();
        for (int i = 0; i < m_privateKeys.Length; i++)
        {
            WebsocketConnectionRsaTunneling tunnel = new WebsocketConnectionRsaTunneling();
            tunnel.SetConnectionInfo(m_serverUri, m_privateKeys[i]);
            m_tunnels.Add(tunnel);
            tunnel.StartConnection();
        }

        while(true) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1);
            for (int i = 0; i < m_tunnels.Count; i++)
            {
                byte[] iBytes = BitConverter.GetBytes(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
                m_tunnels[i].EnqueueBinaryMessages(iBytes);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < m_tunnels.Count; i++)
        {
            m_tunnels[i].UpdateRunningStateInfo();
        }
    }
    [ContextMenu("Generate random keys")]
    public void GenerateRandomPrivateKey() { 
    
        for(int i = 0; i < m_privateKeys.Length; i++) {
            m_privateKeys[i] = RSATunnelingUtility.GenerateRandomPrivateKeyXml();
        }
    }
}
