using UnityEngine;

public class RandomPointOnSphere : MonoBehaviour
{
    private Vector3[] m_points;
    private int m_pointCount;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Generate"))
        { 
            m_points = GeneratePoints(m_pointCount);    
        }
        
        m_pointCount = (int)GUI.HorizontalSlider(new Rect(0, 100, 200, 50), m_pointCount, 0, 10000);
        GUI.Label(new Rect(220, 100, 100, 50), m_pointCount.ToString());
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_points.Length; i++)
        { 
            Gizmos.DrawSphere(m_points[i], 0.005f);   
        }
    }

    private Vector3[] GeneratePoints(int num)
    {
        Vector3[] points = new Vector3[num];
        for (int i = 0; i < num; i++)
        {
            points[i] = Random.onUnitSphere;
        }
        return points;
    }
}
