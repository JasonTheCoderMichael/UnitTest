using UnityEngine;

public class SHCoefficient
{
    public static float SHBasis(int l, int m, Vector3 direction)
    {
        float SHBasis = 0;
        switch (l)
        {
            case 0:
                SHBasis = Y0(direction);
                break;
            case 1:
                switch (m)
                {
                    case -1:
                        SHBasis = Y1_1(direction);
                        break;
                    case 0:         // modification1: use wrong function at first , fixed already //
                        SHBasis = Y10(direction);
                        break;
                    case 1:
                        SHBasis = Y11(direction);
                        break;
                }
                break;
            case 2:
                switch (m)
                {
                    case -2:
                        SHBasis = Y2_2(direction);
                        break;
                    case -1:
                        SHBasis = Y2_1(direction);
                        break;
                    case 0:
                        SHBasis = Y20(direction);
                        break;
                    case 1:
                        SHBasis = Y21(direction);
                        break;
                    case 2:
                        SHBasis = Y22(direction);
                        break;
                }
                break;
        }

        return SHBasis;
    }

    private delegate float SHFunc(Vector3 direction);

    private static readonly SHFunc[] shfunctions = new SHFunc[9] { Y0, Y1_1, Y10, Y11, Y2_2, Y2_1, Y20, Y21, Y22 };
    
    public static float Y(int index, Vector3 direction)
    {
        return shfunctions[index](direction);
    }

    private static float Y0(Vector3 direction)
    {
        return 1/2.0f * sqrt(1.0f / pi);
    }

    private static float Y1_1(Vector3 direction)
    {
        return sqrt(3.0f / (4 * pi)) * direction.y;
    }
    
    private static float Y10(Vector3 direction)
    {
        return sqrt(3.0f / (4 * pi)) * direction.z;
    }
    
    private static float Y11(Vector3 direction)
    {
        return sqrt(3.0f / (4 * pi)) * direction.x;
    }

    private static float Y2_2(Vector3 direction)
    {
        return 1.0f / 2.0f * sqrt(15.0f/pi) * direction.x * direction.y;
    }

    private static float Y2_1(Vector3 direction)
    {
        return 1.0f / 2.0f * sqrt(15.0f/pi) * direction.y * direction.z;
    }
    
    private static float Y20(Vector3 direction)
    {
        return 1 / 4.0f * sqrt(5.0f/pi) * (3 * direction.z * direction.z - 1);
    }
    
    private static float Y21(Vector3 direction)
    {
        return 1 / 2.0f * sqrt(15.0f/pi) * direction.x * direction.z;
    }
    
    private static float Y22(Vector3 direction)
    {
        return 1 / 4.0f * sqrt(15.0f/pi) * (direction.x * direction.x - direction.y * direction.y);
    }
    
    private static float sqrt(float f)
    {
        return Mathf.Sqrt(f);
    }

    private static float pi = 3.1415926f;
}
