using UnityEngine;
using UnityEngine.Events;

public class EventAndIndexFromToServerTunnelingRsaMono : MonoBehaviour{

    public ConnectToServerTunnelingRsaMono m_source;
    public UnityEvent m_onConnected;
    public UnityEvent m_onDisconnected;
    public UnityEvent m_onHandshakeVerified;
    public UnityEvent<int> m_onIndexLockChanged;
    public UnityEvent<bool> m_onIndexLockChangedAndIsValide;
    public UnityEvent<string> m_publicKeyValidated;


    private bool m_connected = false;
    private bool m_disconnected = false;
    private bool m_handshakeVerified = false;
    private bool m_indexLockChanged = false;
    private int m_indexLock = 0;
    private string m_publicKey = "";
    private void Awake()
    {
        m_source.m_trafficEvent.m_onConnectionSignedAndValidated += () => { m_handshakeVerified = true; };
        m_source.m_trafficEvent.m_onWebsocketConnected += () => { m_handshakeVerified = true; };
        m_source.m_trafficEvent.m_onConnectionLost += () => { m_handshakeVerified = true; };
        m_source.m_trafficEvent.m_onIndexLockChanged += (t) => { m_indexLock = t; m_indexLockChanged = true; };

    }
    private void Update()
    {
        if (m_connected == true) { m_connected = false; m_onConnected.Invoke(); }
        if (m_disconnected == true) { m_disconnected = false; m_onDisconnected.Invoke(); }
        if (m_handshakeVerified == true) { m_handshakeVerified = false; m_onHandshakeVerified.Invoke(); }
        if (m_indexLockChanged == true) { m_indexLockChanged = false;
            bool isInvalide = m_indexLock == int.MinValue;
            m_onIndexLockChanged.Invoke(m_indexLock);
            m_onIndexLockChangedAndIsValide.Invoke(!isInvalide);
            m_publicKeyValidated.Invoke(m_source.m_tunnel.GetPublicXmlKey());
        
        }
         }

}
