using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FirstBytesHistoryDebugMono : MonoBehaviour
{

    [TextArea(2,4)]
    public string m_debugText;
    public int m_maxLenght = 100;
    private StringBuilder sb= new StringBuilder();
    public void PushBytesIn(byte[] bytes)
    {
        if (bytes==null || bytes.Length==0)
        {
            return;
        }

        sb.Insert(0,' ');
        sb.Insert(0,bytes[0]);
        while (sb.Length > m_maxLenght)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        


    }

    public void Update()
    {
        m_debugText = sb.ToString();
    }
    
}
