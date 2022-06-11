using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class SH : MonoBehaviour
{
    public Cubemap skyCubemap;
    public int SHOrder;
    public float lerpValue;

    public Material rebuildMaterial;
    public Mesh sphereMesh;
    
    private CommandBuffer m_cmd;
    private Camera m_camera;
    private RenderTexture m_rebuiltRT;
    private Vector4[] m_shCoefficients;
    private Cubemap m_rebuiltCubemap;
    
    private void OnEnable()
    {
        m_cmd = new CommandBuffer();
        m_cmd.name = "Rebuild Light using SH Coefficient";
        
        m_camera = Camera.main;
        m_rebuiltRT = new RenderTexture(skyCubemap.width, skyCubemap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        m_rebuiltRT.name = "RebuiltRT";
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Generate SH Coefficient"))
        {
            // m_shCoefficients = CalculateSHCoefficient(skyCubemap, SHOrder, 4096);
            m_shCoefficients = CalculateSHCoefficient2(skyCubemap, SHOrder, 4096);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(m_camera.transform.position, m_camera.transform.rotation, new Vector3(1, 1, -1)).inverse;
            Matrix4x4 projectionMatrix = m_camera.projectionMatrix;
            rebuildMaterial.SetVectorArray("_SHCoefficients", m_shCoefficients);
            rebuildMaterial.SetInt("_SHOrder", SHOrder);

            int rtID = Shader.PropertyToID("RebuildLightRT");
            m_cmd.Clear();
            m_cmd.GetTemporaryRT(rtID, Screen.width, Screen.height, 24);
            m_cmd.SetRenderTarget(new RenderTargetIdentifier(rtID));
            // m_cmd.SetRenderTarget(m_rebuiltRT);
            m_cmd.ClearRenderTarget(true, true, Color.black);
            m_cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            m_cmd.DrawMesh(sphereMesh, Matrix4x4.identity, rebuildMaterial);
            m_cmd.ReleaseTemporaryRT(rtID);
        }

        if (GUI.Button(new Rect(200, 0, 200, 100), "Rebuild"))
        {
            // m_rebuiltCubemap = RebuildCubemap(skyCubemap.width, m_shCoefficients, 1024 * 1024);
            m_rebuiltCubemap = RebuildCubemap2(skyCubemap.width, m_shCoefficients, 1024 * 1024);
            
            // // 调试用 //
            // Cubemap processedCubemap = SampleCubemapAndDoNothing(skyCubemap, 1024 * 1024);
        }

        if (GUI.Button(new Rect(400, 0, 200, 100), "Save Rebuild Result To Png"))
        {
            SaveCubeAsPng(m_rebuiltCubemap);
        }

        lerpValue = GUI.HorizontalSlider(new Rect(0, 200, 200, 50), lerpValue, 0, 1);
        rebuildMaterial.SetFloat("_LerpValue", lerpValue);
    }

    private void Update()
    {
        Graphics.ExecuteCommandBuffer(m_cmd);
    }

    private Cubemap SampleCubemapAndDoNothing(Cubemap cubemap, int sampleCount)
    {
        if (cubemap == null)
        {
            return null;
        }

        Cubemap destCubemap = new Cubemap(cubemap.width, TextureFormat.ARGB32, false);
        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 rndDirection = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(rndDirection);
            Vector2 uv = GetUV(face, rndDirection);
            int coordX = (int) (skyCubemap.width * uv.x);     // [0, width - 1] //
            int coordY = (int) (skyCubemap.height * uv.y);    // [0, height - 1] //
            Color sourceColor = skyCubemap.GetPixel(face, coordX, coordY);
            // 上下翻转 //
            destCubemap.SetPixel(face, coordX, skyCubemap.height - 1 - coordY, sourceColor);
        }
        destCubemap.Apply();
        return destCubemap;
    }
    
    private Vector4[] CalculateSHCoefficient(Cubemap cubemap, int shOrder, int cubemapSampleCount)
    {
        if (cubemap == null)
        {
            return null;
        }

        Vector4[] shCoefficients = new Vector4[shOrder * shOrder];
        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] = Vector4.zero;
        }

        for (int i = 0; i < cubemapSampleCount; i++)
        {
            Vector3 rndDirection = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(rndDirection);
            Vector2 uv = GetUV(face, rndDirection);
            int coordX = (int) (skyCubemap.width * uv.x);     // [0, width - 1] //
            int coordY = (int) (skyCubemap.height * uv.y);    // [0, height - 1] //
            Color sourceColor = skyCubemap.GetPixel(face, coordX, coordY);

            for (int l = 0; l < shOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    int index = l * (l + 1) + m;
                    Vector4 coefficient = shCoefficients[index];
                    float shBasis = SHCoefficient.SHBasis(l, m, rndDirection);
                    coefficient.x += sourceColor.r * shBasis;
                    coefficient.y += sourceColor.g * shBasis;
                    coefficient.z += sourceColor.b * shBasis;
                    shCoefficients[index] = coefficient;
                }
            }
        }

        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] *= 4 * Mathf.PI / cubemapSampleCount;
        }

        return shCoefficients;
    }
    
    private Vector4[] CalculateSHCoefficient2(Cubemap cubemap, int shOrder, int cubemapSampleCount)
    {
        if (cubemap == null)
        {
            return null;
        }

        Vector4[] shCoefficients = new Vector4[shOrder * shOrder];
        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] = Vector4.zero;
        }

        for (int i = 0; i < cubemapSampleCount; i++)
        {
            Vector3 pointOnUnitSphere = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(pointOnUnitSphere);
            Vector2 uv = GetUV(face, pointOnUnitSphere);
            int coordX = (int) (skyCubemap.width * uv.x);     // [0, width - 1] //
            int coordY = (int) (skyCubemap.height * uv.y);    // [0, height - 1] //
            Color sourceColor = skyCubemap.GetPixel(face, coordX, coordY);
            
            for (int l = 0; l < shOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    int index = l * (l + 1) + m;
                    Vector4 coefficient = shCoefficients[index];
                    float theta = 0;
                    float phi = 0;  
                    CartesianToSphere(pointOnUnitSphere, ref theta, ref phi);
                    float shBasis = SHCoefficient2.Y(l, m, theta, phi);
                    coefficient.x += sourceColor.r * shBasis;
                    coefficient.y += sourceColor.g * shBasis;
                    coefficient.z += sourceColor.b * shBasis;
                    shCoefficients[index] = coefficient;
                }
            }
        }

        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] *= 4 * Mathf.PI / cubemapSampleCount;
        }

        return shCoefficients;
    }
    
    // 创建cubemap副本 //
    private Cubemap CreateCubemapCopy(Cubemap cubemap)
    {
        if (cubemap == null)
        {
            return null;
        }

        int width = cubemap.width;
        int height = cubemap.height;
        // int halfWidth = width / 2;
        int halfHeight = height / 2;
        Cubemap destCubemap = new Cubemap(width, TextureFormat.ARGB32, false);
        for (int i = 0; i < 6; i++)
        {
            CubemapFace face = (CubemapFace) i;
            Color[] colors = cubemap.GetPixels(face);
            
            // for (int row = 0; row < height; row++)        // 左右翻转 //
            // {
            //     for (int column = 0; column < halfWidth; column++)
            //     {
            //         int curIndex = row * width + column;
            //         int flippedColumn = width - 1 - column;
            //         int flippedIndex = row * width + flippedColumn;
            //         Swap(ref colors, curIndex, flippedIndex);
            //     }
            // }
            
            for (int row = 0; row < halfHeight; row++)        // 上下翻转 //
            {
                for (int column = 0; column < width; column++)
                {
                    int curIndex = row * width + column;
                    int flippedRow = height - 1 - row;
                    int flippedIndex = flippedRow * width + column;
                    Swap(ref colors, curIndex, flippedIndex);
                }
            }
            destCubemap.SetPixels(colors, face);
        }

        return destCubemap;
    }

    // 保存cubemap到磁盘 //
    private void SaveCubeAsPng(Cubemap cubemap)
    {
        if (cubemap == null)
        {
            return;
        }

        Texture2D tex2D = ConvertCubemapToTexture2D(cubemap);

#if UNITY_EDITOR
        // string sampledCubemapPath = "Assets/UnitTest/SH/Sampled.asset";
        // UnityEditor.AssetDatabase.CreateAsset(cubemap, sampledCubemapPath);
        // UnityEditor.AssetDatabase.ImportAsset(sampledCubemapPath);
        // UnityEditor.AssetDatabase.Refresh();

        string sampledPNGPath = "Assets/UnitTest/SH/SampledPNG.png";
        byte[] pngBytes = tex2D.EncodeToPNG();
        File.WriteAllBytes(sampledPNGPath, pngBytes);
        UnityEditor.AssetDatabase.ImportAsset(sampledPNGPath);
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void Swap(ref Color[] colors, int index1, int index2)
    {
        Color tempColor = colors[index1];
        colors[index1] = colors[index2];
        colors[index2] = tempColor;
    }

    private CubemapFace GetCubemapFace(Vector3 direction)
    {
        CubemapFace face = CubemapFace.PositiveX;
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        float absZ = Mathf.Abs(direction.z);
        if (absX > absY && absX > absZ)
        {
            if (direction.x >= 0)
            {
                face = CubemapFace.PositiveX;
            }
            else
            {
                face = CubemapFace.NegativeX;
            }
        }
        else if (absY > absX && absY > absZ)
        {
            if (direction.y >= 0)
            {
                face = CubemapFace.PositiveY;
            }
            else
            {
                face = CubemapFace.NegativeY;
            }            
        }
        else if (absZ > absX && absZ > absY)
        {
            if (direction.z >= 0)
            {
                face = CubemapFace.PositiveZ;
            }
            else
            {
                face = CubemapFace.NegativeZ;
            }
        }
        return face;
    }

    private Vector2 GetUV(CubemapFace face, Vector3 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        float absZ = Mathf.Abs(direction.z);
        float maxAbs = Mathf.Max(absX, Mathf.Max(absY, Mathf.Max(absZ, 0.0001f)));
        Vector3 unitDirection = new Vector3(direction.x/maxAbs, direction.y/maxAbs, direction.z/maxAbs);
        Vector2 uv = Vector2.zero;
        switch (face)
        {
            case CubemapFace.NegativeX:
                uv = new Vector2(unitDirection.z, unitDirection.y);
                break;
            case CubemapFace.PositiveX:
                uv = new Vector2(-unitDirection.z, unitDirection.y); 
                break;
            case CubemapFace.NegativeY:
                uv = new Vector2(unitDirection.x, unitDirection.z); 
                break;
            case CubemapFace.PositiveY:
                uv = new Vector2(unitDirection.x, -unitDirection.z); 
                break;
            case CubemapFace.NegativeZ:
                uv = new Vector2(-unitDirection.x, unitDirection.y); 
                break;
            case CubemapFace.PositiveZ:
                uv = new Vector2(unitDirection.x, unitDirection.y); 
                break;
        }
        uv = uv * new Vector2(0.5f, 0.5f) + new Vector2(0.5f, 0.5f);  // [-1, 1] => [0, 1] //
        return uv;
    }

    private Texture2D ConvertCubemapToTexture2D(Cubemap cubemap)
    {
        if (cubemap == null)
        {
            return null;
        }

        Texture2D tex = new Texture2D(cubemap.width * 4, cubemap.height * 3, TextureFormat.ARGB32, false, false);
        Color[] blackPixels = new Color[cubemap.width * cubemap.height];
        for (int i = 0; i < blackPixels.Length; i++)
        {
            blackPixels[i] = Color.black;
        }

        for (int i = 0; i < 12; i++)
        {
            CubemapFace face = GetCubemapFace(i);
            Vector2Int startCoord = GetStartCoord(i, cubemap.width, cubemap.height);
            if (face == CubemapFace.Unknown)
            {
                tex.SetPixels(startCoord.x, startCoord.y, cubemap.width, cubemap.height, blackPixels);
            }
            else
            {
                Color[] pixels = cubemap.GetPixels(face);
                tex.SetPixels(startCoord.x, startCoord.y, cubemap.width, cubemap.height, pixels);
            }
        }
        return tex;
    }

    private Vector2Int GetStartCoord(int index, int width, int height)
    {
        Vector2Int startCoord = Vector2Int.zero;
        startCoord.x = (index % 4) * width;
        startCoord.y = (index / 4) * height;
        return startCoord;
    }

    private CubemapFace GetCubemapFace(int index)
    {
        CubemapFace face = CubemapFace.Unknown;
        switch (index)
        {
            case 1:
                face = CubemapFace.NegativeY;
                break;
            case 4:
                face = CubemapFace.NegativeX;
                break;
            case 5:
                face = CubemapFace.PositiveZ;
                break;
            case 6:
                face = CubemapFace.PositiveX;
                break;
            case 7:
                face = CubemapFace.NegativeZ;
                break;
            case 9:
                face = CubemapFace.PositiveY;
                break;
        }
        return face;
    }

    private Cubemap RebuildCubemap(int width,Vector4[] shCoefficients, int rebuildSampleCount)
    {
        Cubemap rebuildCubemap = new Cubemap(width, TextureFormat.ARGB32, false);
        
        for (int i = 0; i < rebuildSampleCount; i++)
        {
            Vector3 pointOnUnitSphere = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(pointOnUnitSphere);
            Vector2 uv = GetUV(face, pointOnUnitSphere);
            int coordX = (int) (width * uv.x);     // [0, width - 1] //
            int coordY = (int) (width * uv.y);     // [0, height - 1] //
            Color rebuiltColor = Color.black;

            for (int l = 0; l < SHOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    int index = l * (l + 1) + m;
                    Vector4 coefficient = shCoefficients[index];
                    float shBasis = SHCoefficient.SHBasis(l, m, pointOnUnitSphere);
                    rebuiltColor.r += coefficient.x * shBasis;
                    rebuiltColor.g += coefficient.y * shBasis;
                    rebuiltColor.b += coefficient.z * shBasis;
                }
            }
            
            // 上下翻转 //
            // rebuildCubemap.SetPixel(face, coordX, width - 1 - coordY, rebuiltColor);
            rebuildCubemap.SetPixel(face, coordX, coordY, rebuiltColor);
        }
        return rebuildCubemap;
    }
    
    private Cubemap RebuildCubemap2(int width, Vector4[] shCoefficients, int rebuildSampleCount)
    {
        Cubemap rebuildCubemap = new Cubemap(width, TextureFormat.ARGB32, false);
        
        for (int i = 0; i < rebuildSampleCount; i++)
        {
            Vector3 pointOnUnitSphere = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(pointOnUnitSphere);
            Vector2 uv = GetUV(face, pointOnUnitSphere);
            int coordX = (int) (width * uv.x);     // [0, width - 1] //
            int coordY = (int) (width * uv.y);     // [0, height - 1] //
            Color rebuiltColor = Color.black;

            for (int l = 0; l < SHOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    int index = l * (l + 1) + m;
                    Vector4 coefficient = shCoefficients[index];
                    float theta = 0;
                    float phi = 0;  
                    CartesianToSphere(pointOnUnitSphere, ref theta, ref phi);
                    float shBasis = SHCoefficient2.Y(l, m, theta, phi);
                    rebuiltColor.r += coefficient.x * shBasis;
                    rebuiltColor.g += coefficient.y * shBasis;
                    rebuiltColor.b += coefficient.z * shBasis;
                }
            }
            
            // 上下翻转 //
            // rebuildCubemap.SetPixel(face, coordX, width - 1 - coordY, rebuiltColor);
            rebuildCubemap.SetPixel(face, coordX, coordY, rebuiltColor);
        }
        return rebuildCubemap;
    }

    private void SaveRTAsPng(RenderTexture rt, string name)
    {
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, true);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] pngBytes = texture.EncodeToPNG();
        string assetPath = "Assets/UnitTest/SH/" + name + ".png";
        File.WriteAllBytes(assetPath, pngBytes);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(assetPath);
        UnityEditor.AssetDatabase.Refresh();
#endif
        RenderTexture.active = activeRT;
    }

    private void CartesianToSphere(Vector3 pointOnSphere, ref float theta, ref float phi)
    {
        // 第一版 //
        theta = Mathf.Acos(pointOnSphere.z);                  // [0, pi] //
        phi = Mathf.Atan(pointOnSphere.y/pointOnSphere.x);    // [-2/pi, 2/pi]
        
        if (pointOnSphere.x < 0)
        {
            phi += Mathf.PI;
        }
        else if (pointOnSphere.y < 0)
        {
            phi += 2 * Mathf.PI;
        }
    }
}
