using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class UnfoldMeshUV : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    private CommandBuffer m_cmd;
    private Camera m_camera;
    
    private void OnEnable()
    {
        m_cmd = new CommandBuffer();
        m_cmd.name = "Unfold Mesh UV";
        m_camera = Camera.main;
    }

    private void Update()
    {
        m_cmd.Clear();
        
        int rtID = Shader.PropertyToID("_UnfoldUVRT");
        m_cmd.GetTemporaryRT(rtID, Screen.width, Screen.height, 16);
        m_cmd.SetRenderTarget(new RenderTargetIdentifier(rtID));
        m_cmd.ClearRenderTarget(true, true, Color.black);
        
        Matrix4x4 projectMatrix = m_camera.projectionMatrix;

        Matrix4x4 trsMatrix = Matrix4x4.TRS(m_camera.transform.position, m_camera.transform.rotation, new Vector3(1, 1, -1));
        Matrix4x4 viewMatrix = trsMatrix.inverse;

        // Matrix4x4 lookMatrix = Matrix4x4.LookAt(m_camera.transform.position, render.transform.position, Vector3.up);
        // Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, quaternion.identity, new Vector3(1, 1, -1));
        // Matrix4x4 viewMatrix = scaleMatrix * lookMatrix.inverse;
        
        m_cmd.SetViewProjectionMatrices(viewMatrix, projectMatrix);
        m_cmd.DrawMesh(mesh, Matrix4x4.identity, material);
        m_cmd.ReleaseTemporaryRT(rtID);
        
        Graphics.ExecuteCommandBuffer(m_cmd);
    }
}
