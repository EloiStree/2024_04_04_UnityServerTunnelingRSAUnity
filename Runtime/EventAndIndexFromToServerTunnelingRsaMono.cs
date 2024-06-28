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

    private void Awake()
    {
        m_source.m_trafficEvent.m_onConnectionSignedAndValidated += m_onHandshakeVerified.Invoke;
        m_source.m_trafficEvent.m_onWebsocketConnected += m_onDisconnected.Invoke;
        m_source.m_trafficEvent.m_onConnectionLost += m_onConnected.Invoke;
        m_source.m_trafficEvent.m_onIndexLockChanged += LockerChanged;

    }

    private void LockerChanged(int obj)
    {
        m_onIndexLockChanged.Invoke(obj);
        m_onIndexLockChangedAndIsValide.Invoke(obj >int.MinValue);
        m_publicKeyValidated.Invoke(m_source.m_tunnel.m_useKeyPair.m_publicKey.Trim().Replace("\n", "").Replace("  ", " "));
    }
}
