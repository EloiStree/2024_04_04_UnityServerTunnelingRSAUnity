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



    [ContextMenu("Push Random Value Integer LE to all")]
    public void PushRandomIIDToAll()
    {
        for (int i = 0; i < m_tunnels.Count; i++)
        {
            byte[] iBytes = BitConverter.GetBytes(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            m_tunnels[i].EnqueueBinaryMessages(iBytes);
        }
    }
    [ContextMenu("Push Random Value Integer LE to one")]
    public void PushRandomIIDToOne()
    {
        int index = UnityEngine.Random.Range(0, m_tunnels.Count);
        byte[] iBytes = BitConverter.GetBytes(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        m_tunnels[index].EnqueueBinaryMessages(iBytes);
    }

    [ContextMenu("Join current game IID")]
    public void JoinCurrentGame()
    {
        NotifyWantToBePlayerAll();
        ConfirmWantToBePlayerAll();
    }

    [ContextMenu("Want to be player 123456789")]
    public void NotifyWantToBePlayerAll()
    {

        for(int i = 0; i < m_tunnels.Count; i++)
        {
            m_tunnels[i].EnqueueInteger(123456789);
        }
    }
    [ContextMenu("Want to be player confirmed 987654321")]
    public void ConfirmWantToBePlayerAll()
    {

        for (int i = 0; i < m_tunnels.Count; i++)
        {
            m_tunnels[i].EnqueueInteger(987654321);
        }

    }


   

    public bool m_createNewRsaAtStart = false;
    public bool m_autoJoinCurrentGame = true;
    public void Start()
    {
        if(m_createNewRsaAtStart)
            GenerateRandomPrivateKey();
        m_tunnels = new List<WebsocketConnectionRsaTunneling>();
        for (int i = 0; i < m_privateKeys.Length; i++)
        {
            WebsocketConnectionRsaTunneling tunnel = new WebsocketConnectionRsaTunneling();
            tunnel.SetConnectionInfo(m_serverUri, m_privateKeys[i]);
            m_tunnels.Add(tunnel);
            tunnel.StartConnection();
        }
       
        if (m_autoJoinCurrentGame)
        {
            NotifyWantToBePlayerAll();
            ConfirmWantToBePlayerAll();
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
