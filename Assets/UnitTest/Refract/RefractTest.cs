using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RefractTest : MonoBehaviour
{
    [Range(1.0f, 2.0f)]
    public float RefractIndex = 1.0f;
    
    public Transform StartObj;
    public Transform TargetObj;
    
    private const float REFRACT_INDEX_AIR = 1;
    private const float MAX_RAY_DISTANCE = 10;
    
    private Line m_incidentRay;
    private Line m_refractedRay;
    private Line m_outRay;
    private Line m_inNormalRay;
    private Line m_exitNormalRay;

    private Collider m_collider;
    private Transform m_curTargetObj;
    private Vector3 m_randomVec;
    
    private void OnEnable()
    {
        if (TargetObj != null)
        {
            m_collider = TargetObj.GetComponent<Collider>();   
        }
        
        CheckTarget(TargetObj);
    }

    public struct Line
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 direction;
        public float length;

        public Line(Vector3 startPoint, Vector3 endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.direction = Vector3.Normalize(endPoint - startPoint);
            this.length = Vector3.Distance(startPoint, endPoint);
        }

        public Line(Vector3 startPoint, Vector3 direction, float length)
        {
            this.startPoint = startPoint;
            this.direction = Vector3.Normalize(direction);
            this.endPoint = startPoint + this.direction * length;
            this.length = length;
        }

        public void DrawGizmos(Color gizmosColor)
        {
            Color color = Gizmos.color;
            Gizmos.color = gizmosColor;
            Gizmos.DrawLine(startPoint, endPoint);
            Gizmos.color = color;
        }
    }

    private void Update()
    {
        CheckTarget(TargetObj);
        SimulateGlass1(StartObj, TargetObj);
        // SimulateGlass2(StartObj, TargetObj);
    }

    private void CheckTarget(Transform target)
    {
        if (m_curTargetObj != target)
        {
            m_curTargetObj = target;
            m_randomVec = Random.insideUnitSphere * 0.2f;
        }
    }

    // // 模拟片状玻璃 //
    // private void SimulateGlassSlice()
    // {
    //     // Incident Ray //
    //     Vector3 startPoint = StartPoint.position;
    //     Vector3 direction = -m_viewDir;
    //     float length = Vector3.Distance(startPoint, SliceGlass.position);
    //     m_incidentRay = new Line(startPoint, direction, length);    
    //     
    //     // Refracted Ray //
    //     startPoint = m_incidentRay.endPoint;
    //     direction = CalculateRefractVector(m_viewDir, SliceGlass.up, REFRACT_INDEX_AIR, RefractIndex);
    //     length = 1;
    //     m_refractedRay = new Line(startPoint, direction, length);
    // }

    // 模拟块状玻璃 //
    private void SimulateGlass1(Transform startPoint, Transform target)
    {
        if (startPoint == null || target == null)
        {
            return;
        }
        
        // Incident Ray //
        Vector3 inPos = GetIntersectPos(startPoint.position,  target.position - (startPoint.position + m_randomVec), out Vector3 inNormal);
        m_incidentRay = new Line(startPoint.position, inPos);    
        
        // Refracted Ray //
        Vector3 refractDir = CalculateRefractVector(-m_incidentRay.direction, inNormal, REFRACT_INDEX_AIR, RefractIndex);
        Vector3 exitPos = GetIntersectPos(m_incidentRay.endPoint + refractDir * MAX_RAY_DISTANCE, -refractDir, out Vector3 exitNormal);
        m_refractedRay = new Line(m_incidentRay.endPoint, exitPos);
        
        // Out Ray //
        Vector3 outDir = CalculateRefractVector(-m_refractedRay.direction, -exitNormal, RefractIndex, REFRACT_INDEX_AIR);
        m_outRay = new Line(m_refractedRay.endPoint, outDir, 1);
        
        // Normal Ray //
        m_inNormalRay = new Line(inPos, inNormal, 0.5f);
        m_exitNormalRay = new Line(exitPos, exitNormal, 0.5f);
    }

    // // 获取在玻璃中前进的距离 //
    // private float GetTravelDistanceInGlass(Vector3 startPoint, Vector3 direction)
    // {
    //     if (m_boxCollider != null)
    //     {
    //         // 构造一个反向的Ray //
    //         RaycastHit hitInfo;
    //         if (m_boxCollider.Raycast(new Ray(startPoint + direction * MAX_RAY_DISTANCE, -direction), out hitInfo, Mathf.Infinity))
    //         {
    //             return MAX_RAY_DISTANCE - hitInfo.distance;
    //         }
    //     }
    //
    //     return 0;
    // }

    // 获取Cube Glass 的入射点坐标 //
    private Vector3 GetCubeIncidentPos(in Vector3 origin, in Vector3 direction, out Vector3 outNormal)
    {
        outNormal = Vector3.up;
        Ray ray = new Ray(origin, direction);
        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, Mathf.Infinity))
        {
            outNormal = hitinfo.normal;
            return hitinfo.point;
        }
        return Vector3.zero;
    }

    // 模拟球形玻璃 //
    private void SimulateGlass2(Transform startPoint, Transform target)
    {
        if (startPoint == null || target == null)
        {
            return;
        }

        // Incident Ray //
        Vector3 inPos = GetIntersectPos(startPoint.position,   target.position - (startPoint.position + m_randomVec), out Vector3 inNormal);
        m_incidentRay = new Line(startPoint.position, inPos);    
        
        // Refracted Ray //
        Vector3 refractDir = CalculateRefractVector(-m_incidentRay.direction, inNormal, REFRACT_INDEX_AIR, RefractIndex);
        Vector3 exitPos = GetIntersectPos(m_incidentRay.endPoint + refractDir * MAX_RAY_DISTANCE, -refractDir, out Vector3 exitNormal);
        m_refractedRay = new Line(m_incidentRay.endPoint, exitPos);
        
        // Out Ray //   
        Vector3 outDir = CalculateRefractVector(-m_refractedRay.direction, -exitNormal, RefractIndex, REFRACT_INDEX_AIR);
        m_outRay = new Line(m_refractedRay.endPoint, outDir, 1);
        
        // Normal Ray //
        m_inNormalRay = new Line(inPos, inNormal, 0.5f);
        m_exitNormalRay = new Line(exitPos, exitNormal, 0.5f);
    }
    
    // 获取相交点坐标 //
    private Vector3 GetIntersectPos(in Vector3 origin, in Vector3 direction, out Vector3 outNormal)
    {
        outNormal = Vector3.up;
        Ray ray = new Ray(origin, direction);
        RaycastHit hitinfo;
        // if (Physics.Raycast(ray, out hitinfo, Mathf.Infinity))
        if (m_collider.Raycast(ray, out hitinfo, Mathf.Infinity))
        {
            outNormal = hitinfo.normal;
            return hitinfo.point;
        }
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        m_incidentRay.DrawGizmos(Color.white);
        m_refractedRay.DrawGizmos(Color.green);
        m_outRay.DrawGizmos(Color.red);
        
        m_inNormalRay.DrawGizmos(Color.blue);
        m_exitNormalRay.DrawGizmos(Color.blue);
    }

    // 计算折射方向，没使用内置的 refract 方法, 从A介质进入B介质 //
    // inDir 和 normalWS 需要保证是归一化过的 //
    private Vector3 CalculateRefractVector(Vector3 inDir, Vector3 normalWS, float refractIndexA, float refractIndexB)
    {
        float cosThetaA = Mathf.Clamp01(Vector3.Dot(inDir, normalWS));
        float sinThetaA = Mathf.Sqrt(1 - cosThetaA * cosThetaA);
        float sinThetaB = Mathf.Clamp01(refractIndexA * sinThetaA / refractIndexB);
        float cosThetaB = MathF.Sqrt(1 - sinThetaB * sinThetaB);

        Vector3 viewProjectedDir = -inDir - Vector3.Dot(inDir, normalWS) * (-normalWS);
        viewProjectedDir = Vector3.Normalize(viewProjectedDir);
        Vector3 refractDir = -normalWS * cosThetaB + viewProjectedDir * sinThetaB;
        return refractDir;
    }

    private void OnDisable()
    {
        m_collider = null;
        m_curTargetObj = null;
        m_randomVec = Vector3.zero;
    }
}
