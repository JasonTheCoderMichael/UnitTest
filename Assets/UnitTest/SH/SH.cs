using System.IO;
using UnityEngine;

public class SH : MonoBehaviour
{
    public Cubemap skyCubemap;
    public int sampleCount;
    
    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "Process"))
        {
            Cubemap processedCubemap = ProcessCubemap(skyCubemap);
            
            // // 调试用 //
            // Cubemap cubemapCopy = CreateCubemapCopy(skyCubemap);
            
            SaveCubemap(processedCubemap);
        }
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
