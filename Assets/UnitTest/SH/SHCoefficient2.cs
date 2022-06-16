using UnityEngine;

public class SHCoefficient2
{
    private static float pi = 3.1415926f;

    public static float Y(int l, int m, float theta, float phi)
    {
        if (m > 0)
        {
            return sqrt(2.0f) * K(l, m) * cos(m * phi) * P(l, m, cos(theta));
        }

        if (m < 0)
        {
            return sqrt(2.0f) * K(l, m) * sin(-m * phi) * P(l, -m, cos(theta));
        }
        
        return K(l, 0) * P(l, 0, cos(theta));
    }

    // 错误1: 最后一项写成了 Factorial(l - m) //
    private static float K(int l, int m)
    {
        m = (int)abs(m);
        float item = (2 * l + 1) * Factorial(l - m) / (4 * pi * Factorial(l + m));
        return sqrt(item);
    }

    private static float P(int l, int m, float x)
    {
        if (l == m)
        {
            return pow(-1.0f, m) * DoubleFactorial(2 * m - 1) * pow(1 - x * x, m / 2.0f);
        }

        if (l == m + 1)
        {
            return x * (2 * m + 1) * P(m, m, x);
        }

        return (x * (2 * l - 1) * P(l - 1, m, x) - (l + m - 1) * P(l - 2, m, x)) / (l - m);
    }

    // 当 m==0 时传入的参数为 -1 //
    // private static float DoubleFactorial(int x)
    // {
    //     if (x == 0 || x == -1)
    //     {
    //         return 1;
    //     }
    //
    //     int result = x;
    //     while (x > 2)
    //     {
    //         x -= 2;
    //         result *= x;
    //     }
    //     return result;
    // }
    
    private static float DoubleFactorial(int x)
    {
        if (x == 0 || x == -1)
        {
            return 1;
        }

        int result = x;
        while ((x -= 2) > 0)
        {
            result *= x;
        }
        return result;
    }

    // 阶乘 //
    // private static int Factorial(int v)
    // {
    //     if (v == 0)
    //     {
    //         return 1;
    //     }
    //
    //     int result = v;
    //     while (v > 1)
    //     {
    //         v--;
    //         result *= v;
    //     }
    //     return result;
    // }
    
    private static int Factorial(int v)
    {
        if (v == 0)
        {
            return 1;
        }

        int result = v;
        while (--v > 0)
        {
            result *= v;
        }
        return result;
    }

    private static float abs(float f)
    {
        return Mathf.Abs(f);
    }
    
    private static float sqrt(float f)
    {
        return Mathf.Sqrt(f);
    }

    private static float pow(float f, float p)
    {
        return Mathf.Pow(f, p);
    }
    
    private static float sin(float radian)
    {
        return Mathf.Sin(radian);
    }
    
    private static float cos(float radian)
    {
        return Mathf.Cos(radian);
    }
}