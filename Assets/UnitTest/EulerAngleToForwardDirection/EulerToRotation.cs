
using UnityEngine;

[ExecuteInEditMode]
public class EulerToRotation : MonoBehaviour
{
    public Renderer render;
    private Vector3 m_eulerAngle;
    private Material m_material;
    
    private void OnEnable()
    {
        if (render != null)
        {
            m_material = render.material;   
        }
    }
    
    private void Update()
    {
        // if (m_eulerAngle != render.transform.eulerAngles)
        // {
            m_eulerAngle = render.transform.eulerAngles;
            Matrix4x4 transformMatrix = CalculateTransformMatrix(m_eulerAngle);
            // 修改1: 使用 Vector3.forward, 不是 render.transform.forward //
            Vector4 rotatedForward = transformMatrix.MultiplyPoint3x4(Vector3.forward);
            m_material.SetVector("_CalculatedForward", rotatedForward);
            m_material.SetVector("_TransformForward", render.transform.forward);
        // }
    }

    private Matrix4x4 CalculateTransformMatrix(Vector3 eulerAngle)
    {
        // 修改2: 使用弧度, 不是角度 //
        float cosThetaX = Mathf.Cos(eulerAngle.x * Mathf.Deg2Rad);
        float sinThetaX = Mathf.Sin(eulerAngle.x * Mathf.Deg2Rad);
        float cosThetaY = Mathf.Cos(eulerAngle.y * Mathf.Deg2Rad);
        float sinThetaY = Mathf.Sin(eulerAngle.y * Mathf.Deg2Rad);
        float cosThetaZ = Mathf.Cos(eulerAngle.z * Mathf.Deg2Rad);
        float sinThetaZ = Mathf.Sin(eulerAngle.z * Mathf.Deg2Rad);
        
        // 修改3: Matrix4x4 构造函数的参数是列向量, 使用SetRow方法明确设置行向量 //
        Matrix4x4 rotateAroundX = Matrix4x4.identity;
        rotateAroundX.SetRow(1, new Vector4(0, cosThetaX, -sinThetaX, 0));
        rotateAroundX.SetRow(2, new Vector4(0, sinThetaX, cosThetaX, 0));
        
        // 修改4: cos sin 设置的位置不对 //
        Matrix4x4 rotateAroundY = Matrix4x4.identity;
        rotateAroundY.SetRow(0, new Vector4(cosThetaY, 0, sinThetaY, 0));
        rotateAroundY.SetRow(2, new Vector4(-sinThetaY, 0, cosThetaY, 0));

        Matrix4x4 rotateAroundZ = Matrix4x4.identity;
        rotateAroundZ.SetRow(0, new Vector4(cosThetaZ, -sinThetaZ, 0, 0));
        rotateAroundZ.SetRow(1, new Vector4(sinThetaZ, cosThetaZ, 0, 0));

        // 顺序: Z -> X -> Y //
        return rotateAroundY * rotateAroundX * rotateAroundZ;
    }
}
