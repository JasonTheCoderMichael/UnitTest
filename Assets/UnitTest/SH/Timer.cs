using System;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Timer : IDisposable
{
    public enum ETimerUnit
    {
        Millisecond = 0,
        Second,
    }

    private string m_tag;
    private Stopwatch m_watch;
    private ETimerUnit m_timerUnit;
    
    public Timer(string tag, ETimerUnit unit)
    {
        m_tag = tag;
        m_timerUnit = unit;
        m_watch = new Stopwatch();
        m_watch.Start();
    }

    public void Dispose()
    {
        float timeElapsed = 0;
        string unit = "";
        if (m_watch != null)
        {
            m_watch.Stop();
            switch (m_timerUnit)
            {
                case ETimerUnit.Millisecond:
                    timeElapsed = m_watch.ElapsedMilliseconds;
                    unit = "ms";
                    break;
                case ETimerUnit.Second:
                    timeElapsed = m_watch.ElapsedMilliseconds / 1000.0f;
                    unit = "s";
                    break;
            }
        }

#if UNITY_EDITOR
        Debug.Log($"{m_tag}, time elapsed : {timeElapsed.ToString("#0.0")} {unit}");
#endif
    }
}
