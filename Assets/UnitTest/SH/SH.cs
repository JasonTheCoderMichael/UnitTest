using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class SH : MonoBehaviour
{
    public Cubemap skyCubemap;
    public int SHOrder;

    private Vector4[] m_shCoefficients;
    private Cubemap m_rebuiltCubemap;
    
    private Vector4[] m_shCoefficients2;
    private Cubemap m_rebuiltCubemap2;

    private Vector3[] m_rndPoints;
    private Vector4[] m_gizmosPoints;
    private int m_l = 0;
    private int m_m = 0;

    private SHVisualizeData[] m_shVisualDatas;
    
    private void OnEnable()
    {
    }
    
    public struct SHVisualizeData
    {
        public Vector3 position;
        public float distanceFromOrigin;
        public Color color;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Generate SH_3 Coefficient"))
        {
            using (Timer timer = new Timer("CalculateSHCoefficient", Timer.ETimerUnit.Millisecond))
            {
                m_shCoefficients = CalculateCoefficientSH_3(skyCubemap, m_rndPoints);
            }
        }
        
        if (GUI.Button(new Rect(200, 0, 200, 100), "Rebuild SH_3 To Cubemap"))
        {
            using (Timer timer = new Timer("RebuildCubemap", Timer.ETimerUnit.Millisecond))
            {
                m_rebuiltCubemap = RebuildSH3ToCubemap(skyCubemap.width, m_shCoefficients, 1024 * 1024);
                // skyboxMaterial.SetTexture("_SkyboxTex", m_rebuiltCubemap);
                // RenderSettings.skybox = skyboxMaterial;
            }
        }
        
        if (GUI.Button(new Rect(400, 0, 200, 100), "Save Cubemap To Png"))
        {
            using (Timer timer = new Timer("SaveCubeToPng", Timer.ETimerUnit.Millisecond))
            {
                SaveCubeAsPng(m_rebuiltCubemap, "RebuildSH3");
            }
        }
        
        if (GUI.Button(new Rect(0, 100, 200, 100), "Generate SH_N Coefficient"))
        {
            using (Timer timer = new Timer("CalculateSHCoefficient", Timer.ETimerUnit.Millisecond))
            {
                m_shCoefficients2 = CalculateCoefficientSH_N(skyCubemap, SHOrder, m_rndPoints);
            }
        }
        
        if (GUI.Button(new Rect(200, 100, 200, 100), "Rebuild SH_N To Cubemap"))
        {
            using (Timer timer = new Timer("Rebuild Optimized", Timer.ETimerUnit.Millisecond))
            {
                m_rebuiltCubemap2 = RebuildToCubemap(skyCubemap.width, m_shCoefficients2, 1024 * 1024);
            }
        }

        if (GUI.Button(new Rect(400, 100, 200, 100), "Save Cubemap To Png"))
        {
            using (Timer timer = new Timer("SaveCubeToPng", Timer.ETimerUnit.Millisecond))
            {
                SaveCubeAsPng(m_rebuiltCubemap2, "RebuildSH_" + SHOrder);
            }
        }

        if (GUI.Button(new Rect(200, 600, 200, 100), "Generate Random Points"))
        {
            m_rndPoints = GenerateRandomPoints(4096);
        }
        if (GUI.Button(new Rect(0, 400, 200, 100), "SH Coefficient Visualization"))
        {
            VisualizationSH_N(m_l, m_m, 1024 * 64, out m_shVisualDatas);

            m_gizmosPoints = new Vector4[m_shVisualDatas.Length];
            for (int i = 0; i < m_shVisualDatas.Length; i++)
            {
                m_gizmosPoints[i] = m_shVisualDatas[i].position;
            }
        }

        GUILayout.BeginHorizontal();
        m_l = (int)GUI.HorizontalSlider(new Rect(0, 500, 200, 20), m_l, 0, 10);
        GUI.Label(new Rect(220, 500, 200, 20), $"L: {m_l}");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        m_m = Mathf.Max(-m_l, m_m);
        m_m = Mathf.Min(m_l, m_m);
        m_m = (int)GUI.HorizontalSlider(new Rect(0, 530, 200, 50), m_m, -m_l, m_l);
        GUI.Label(new Rect(220, 530, 200, 50), $"M: {m_m}");
        GUILayout.EndHorizontal();
    }

    private void OnDrawGizmos()
    {
        if (m_shVisualDatas != null)
        {
            for (int i = 0; i < m_shVisualDatas.Length; i++)
            {
                Color originColor = Gizmos.color;
                Gizmos.color = m_shVisualDatas[i].color;
                Gizmos.DrawSphere(m_shVisualDatas[i].position, 0.005f);
                Gizmos.color = originColor;
            }
        }
    }

    private Vector3[] GenerateRandomPoints(int pointCount)
    {
        Vector3[] rndPoints = new Vector3[pointCount];
        
        for (int i = 0; i < rndPoints.Length; i++)
        {
            rndPoints[i] = Random.onUnitSphere;
        }

        return rndPoints;
    }
    
    private void VisualizationSH_3(int yIndex, int pointCount, out SHVisualizeData[] visualDatas)
    {
        visualDatas = new SHVisualizeData[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector4 randomPoint = Random.onUnitSphere;
            float coefficient = SHCoefficient.Y(yIndex, randomPoint);
            float distanceFromOrigin = Mathf.Abs(coefficient);

            Vector3 visualizedPosition = randomPoint * distanceFromOrigin;
            Vector3 convertedPosition = new Vector3(visualizedPosition.y, visualizedPosition.z, -visualizedPosition.x);
            
            Color color = coefficient > 0 ? Color.green : Color.red;
            color = color * distanceFromOrigin;
            
            SHVisualizeData data = new SHVisualizeData();
            data.position = convertedPosition;
            data.distanceFromOrigin = distanceFromOrigin;
            data.color = color;
            visualDatas[i] = data;
        }
    }
    
    private void VisualizationSH_N(int l, int m, int pointCount, out SHVisualizeData[] visualDatas)
    {
        visualDatas = new SHVisualizeData[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector4 randomPoint = Random.onUnitSphere;
            float theta, phi;
            CartesianToSphere2(randomPoint, out theta, out phi);
            float coefficient = SHCoefficient2.Y(l, m, theta, phi);
            float distanceFromOrigin = Mathf.Abs(coefficient);

            Vector3 visualizedPosition = randomPoint * distanceFromOrigin;
            Vector3 convertedPosition = new Vector3(visualizedPosition.y, visualizedPosition.z, -visualizedPosition.x);
            
            Color color = coefficient > 0 ? Color.green : Color.red;
            color = color * distanceFromOrigin;
            
            SHVisualizeData data = new SHVisualizeData();
            data.position = convertedPosition;
            data.distanceFromOrigin = distanceFromOrigin;
            data.color = color;
            visualDatas[i] = data;
        }
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
            int coordX = (int) (cubemap.width * uv.x);     // [0, width - 1] //
            int coordY = (int) (cubemap.height * uv.y);    // [0, height - 1] //
            Color sourceColor = cubemap.GetPixel(face, coordX, coordY);
            sourceColor = sourceColor.linear;
            destCubemap.SetPixel(face, coordX, coordY, sourceColor);
            
            // ???????????? //
            // destCubemap.SetPixel(face, coordX, skyCubemap.height - 1 - coordY, sourceColor);
        }
        destCubemap.Apply();
        return destCubemap;
    }

    private Vector4[] CalculateCoefficientSH_3(Cubemap cubemap, Vector3[] rndPoints)
    {
        if (cubemap == null || rndPoints == null)
        {
            return null;
        }

        Vector4[] shCoefficients = new Vector4[9];
        for (int i = 0; i < 9; i++)
        {
            shCoefficients[i] = Vector4.zero;
        }

        int randomPointCount = rndPoints.Length;
        for (int i = 0; i < randomPointCount; i++)
        {
            Vector3 rndPoint = rndPoints[i];
            CubemapFace face = GetCubemapFace(rndPoint);
            Vector2 uv = GetUV(face, rndPoint);
            int coordX = (int) (cubemap.width * uv.x);     // [0, width - 1] //
            int coordY = (int) (cubemap.height * uv.y);    // [0, height - 1] //
            Color sourceColor = cubemap.GetPixel(face, coordX, coordY);

            for (int coeffIndex = 0; coeffIndex < 9; coeffIndex++)
            {
                float shBasis = SHCoefficient.Y(coeffIndex, rndPoint);
                Vector4 coefficient = shCoefficients[coeffIndex];
                coefficient.x += sourceColor.r * shBasis;
                coefficient.y += sourceColor.g * shBasis;
                coefficient.z += sourceColor.b * shBasis;
                shCoefficients[coeffIndex] = coefficient;
            }
        }

        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] *= 4 * Mathf.PI / randomPointCount;
        }

        return shCoefficients;
    }
    
    private Vector4[] CalculateCoefficientSH_N(Cubemap cubemap, int shOrder, Vector3[] rndPoints)
    {
        if (cubemap == null || rndPoints == null)
        {
            return null;
        }

        Vector4[] shCoefficients = new Vector4[shOrder * shOrder];
        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] = Vector4.zero;
        }
        
        Color[][] faceColors = new Color[6][];
        for (int i = 0; i < 6; i++)
        {
            faceColors[i] = cubemap.GetPixels((CubemapFace)i);
        }
        
        for (int i = 0; i < rndPoints.Length; i++)
        {
            Vector3 rndPoint = rndPoints[i];
            CubemapFace face = GetCubemapFace(rndPoint);
            Vector2 uv = GetUV(face, rndPoint);
            int pixelIndex = GetPixelIndex(cubemap.width, uv);
            Color randiance = faceColors[(int) face][pixelIndex];

            float theta = 0;
            float phi = 0;  
            CartesianToSphere(rndPoint, ref theta, ref phi);
            
            float[] shBases = new float[shCoefficients.Length];
            for (int l = 0; l < shOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    shBases[l*(l+1)+m] = SHCoefficient2.Y(l, m, theta, phi);
                }
            }
            
            for (int j = 0; j < shCoefficients.Length; j++)
            {
                float shBasis = shBases[j];
                shCoefficients[j].x += randiance.r * shBasis;
                shCoefficients[j].y += randiance.g * shBasis;
                shCoefficients[j].z += randiance.b * shBasis;
            }
        }

        for (int i = 0; i < shCoefficients.Length; i++)
        {
            shCoefficients[i] *= 4 * Mathf.PI / rndPoints.Length;
        }

        return shCoefficients;
    }

    private Cubemap RebuildToCubemap(int width, Vector4[] shCoefficients, int rebuildSampleCount)
    {
        if (shCoefficients == null)
        {
            return null;
        }

        int shOrder = (int)Mathf.Sqrt(shCoefficients.Length);
        Cubemap rebuildCubemap = new Cubemap(width, TextureFormat.ARGB32, false);

        Color[][] faceColors = new Color[6][];
        for (int i = 0; i < 6; i++)
        {
            Color[] colors = new Color[width * width];   
            for (int j = 0; j < colors.Length; j++)
            {
                colors[j] = Color.black;
            }
            faceColors[i] = colors;
        }
        
        float[] shBases = new float[shCoefficients.Length];
        
        for (int i = 0; i < rebuildSampleCount; i++)
        {
            Vector3 pointOnUnitSphere = Random.onUnitSphere;
            float theta = 0;
            float phi = 0;
            CartesianToSphere(pointOnUnitSphere, ref theta, ref phi);

            for (int l = 0; l < shOrder; l++)
            {
                for (int m = -l; m <= l; m++)
                {
                    shBases[l*(l+1)+m] = SHCoefficient2.Y(l, m, theta, phi);
                }
            }
            
            CubemapFace face = GetCubemapFace(pointOnUnitSphere);
            Vector2 uv = GetUV(face, pointOnUnitSphere);
            Color[] colors = faceColors[(int) face];
            int pixelIndex = GetPixelIndex(width, uv, true);         // ???????????? //
            
            Color rebuiltColor = Color.black;
            for (int j = 0; j < shCoefficients.Length; j++)
            {
                Vector4 coefficient = shCoefficients[j];
                float shBasis = shBases[j];
                rebuiltColor.r += coefficient.x * shBasis;
                rebuiltColor.g += coefficient.y * shBasis;
                rebuiltColor.b += coefficient.z * shBasis;
            }
            
            colors[pixelIndex] = rebuiltColor;
        }
        
        for (int i = 0; i < 6; i++)
        { 
            rebuildCubemap.SetPixels(faceColors[i], (CubemapFace)i);
        }
        rebuildCubemap.Apply();
        return rebuildCubemap;
    }

    // ??????cubemap?????? //
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
            
            // for (int row = 0; row < height; row++)        // ???????????? //
            // {
            //     for (int column = 0; column < halfWidth; column++)
            //     {
            //         int curIndex = row * width + column;
            //         int flippedColumn = width - 1 - column;
            //         int flippedIndex = row * width + flippedColumn;
            //         Swap(ref colors, curIndex, flippedIndex);
            //     }
            // }
            
            for (int row = 0; row < halfHeight; row++)        // ???????????? //
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

    // ??????cubemap????????? //
    private void SaveCubeAsPng(Cubemap cubemap, string pngFileName)
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

        string sampledPNGPath = "Assets/UnitTest/SH/" + pngFileName + ".png";
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
        
        // Test //
        if (Mathf.Abs(uv.x - 1.0f) < float.Epsilon)
        {
            uv.x = 0.9999f;
        }

        if (Mathf.Abs(uv.y - 1.0f) < float.Epsilon)
        {
            uv.y = 0.9999f;
        }
        return uv;
    }

    private int GetPixelIndex(int size, Vector2 uv, bool flipY = false)
    {
        int coordX = (int) (size * uv.x);     // [0, width - 1] //
        int coordY = (int) (size * uv.y);     // [0, height - 1] //
        int pixelIndex = coordY * size + coordX;
        if (flipY)
        {
             pixelIndex = (size - 1 - coordY) * size + coordX;    
        }
        return pixelIndex;
    }

    private Texture2D ConvertCubemapToTexture2D(Cubemap cubemap)
    {
        if (cubemap == null)
        {
            return null;
        }

        Texture2D tex = new Texture2D(cubemap.width * 4, cubemap.height * 3, TextureFormat.ARGB32, false, true);
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

    private Cubemap RebuildSH3ToCubemap(int size, Vector4[] shCoefficients, int sampleCount)
    {
        Cubemap rebuildCubemap = new Cubemap(size, TextureFormat.ARGB32, false);
        
        Color[][] faceColors = new Color[6][];
        for (int i = 0; i < 6; i++)
        {
            Color[] colors = new Color[size * size];   
            for (int j = 0; j < colors.Length; j++)
            {
                colors[j] = Color.black;
            }
            faceColors[i] = colors;
        }
        
        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 pointOnUnitSphere = Random.onUnitSphere;
            CubemapFace face = GetCubemapFace(pointOnUnitSphere);
            Vector2 uv = GetUV(face, pointOnUnitSphere);
            int pixelIndex = GetPixelIndex(size, uv, true);         // ???????????? //
            
            Color rebuiltColor = Color.black;
            for (int coeffIndex = 0; coeffIndex < 9; coeffIndex++)
            {
                Vector4 coefficient = shCoefficients[coeffIndex];
                float shBasis = SHCoefficient.Y(coeffIndex, pointOnUnitSphere);
                rebuiltColor.r += coefficient.x * shBasis;
                rebuiltColor.g += coefficient.y * shBasis;
                rebuiltColor.b += coefficient.z * shBasis;
            }
            
            faceColors[(int)face][pixelIndex] = rebuiltColor;
        }

        for (int i = 0; i < 6; i++)
        {
            rebuildCubemap.SetPixels(faceColors[i], (CubemapFace)i);
        }
        rebuildCubemap.Apply();
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

    // return theta: [0, pi],   phi: [0, 2*pi] //
    private void CartesianToSphere(Vector3 pointOnSphere, ref float theta, ref float phi)
    {
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
    
    private void CartesianToSphere2(Vector3 pointOnSphere, out float theta, out float phi)
    {
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

    private void IndexToLM(int index, out int l, out int m)
    {
        l = (int)Mathf.Sqrt(index);
        m = index - l * (l + 1);
    }
}
