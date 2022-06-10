using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class SH : MonoBehaviour
{
    public Cubemap skyCubemap;
    public int sampleCount;
    public int SHDegree;

    public Material rebuildMaterial;
    public Mesh sphereMesh;
    
    private CommandBuffer m_cmd;
    private Camera m_camera;
    
    private void OnEnable()
    {
        m_cmd = new CommandBuffer();
        m_cmd.name = "Rebuild Light using SH Coefficient";
        
        m_camera = Camera.main;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Process"))
        {
            // // 调试用 //
            // Cubemap processedCubemap = ProcessCubemap(skyCubemap);
            // Cubemap cubemapCopy = CreateCubemapCopy(skyCubemap);
            // SaveCubemap(processedCubemap);
            
            Vector4[] shCoefficients = CalculateSHCoefficient(skyCubemap, SHDegree, sampleCount);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(m_camera.transform.position, m_camera.transform.rotation, new Vector3(1, 1, -1)).inverse;
            Matrix4x4 projectionMatrix = m_camera.projectionMatrix;
            rebuildMaterial.SetVectorArray("_SHCoefficients", shCoefficients);
            
            m_cmd.Clear();
            m_cmd.ClearRenderTarget(true, true, Color.black);
            m_cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            m_cmd.DrawMesh(sphereMesh, Matrix4x4.identity, rebuildMaterial);
        }
    }

    private void Update()
    {
        Graphics.ExecuteCommandBuffer(m_cmd);
    }

    private Cubemap ProcessCubemap(Cubemap cubemap)
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
    
    private Vector4[] CalculateSHCoefficient(Cubemap cubemap, int shDegree, int cubemapSampleCount)
    {
        if (cubemap == null)
        {
            return null;
        }

        Vector4[] shCoefficients = new Vector4[shDegree * shDegree];
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

            for (int l = 0; l < shDegree; l++)
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
    private void SaveCubemap(Cubemap cubemap)
    {
        if (cubemap == null)
        {
            return;
        }

        Texture2D tex2D = ConvertCubemapToTexture2D(cubemap);

#if UNITY_EDITOR
        string sampledCubemapPath = "Assets/UnitTest/SH/Sampled.asset";
        UnityEditor.AssetDatabase.CreateAsset(cubemap, sampledCubemapPath);
        UnityEditor.AssetDatabase.ImportAsset(sampledCubemapPath);
        UnityEditor.AssetDatabase.Refresh();

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
}
