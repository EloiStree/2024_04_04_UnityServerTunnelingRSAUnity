using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConnectToRelayIntegerMono : MonoBehaviour
{

    public int m_relay;
    public UnityEvent<int> m_onRelay;

    public void Push(int value) {
        m_onRelay.Invoke(value);
    }
}
