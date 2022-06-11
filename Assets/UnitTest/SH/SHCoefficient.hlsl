
#ifndef SH_COEFFICIENT_INCLUDE
#define SH_COEFFICIENT_INCLUDE

#define pi 3.1415926f

float Y0()
{
    return 1 / 2.0f * sqrt(1.0f / pi);
}

float Y1_1(float3 direction)
{
    return sqrt(3.0f / (4 * pi)) * direction.y;
}
    
float Y10(float3 direction)
{
    return sqrt(3.0f / (4 * pi)) * direction.z;
}
    
float Y11(float3 direction)
{
    return sqrt(3.0f / (4 * pi)) * direction.x;
}

float Y2_2(float3 direction)
{
    return 1 / 2.0f * sqrt(15.0f/pi) * direction.x * direction.y;
}

float Y2_1(float3 direction)
{
    return 1 / 2.0f * sqrt(15.0f/pi) * direction.y * direction.z;
}
    
float Y20(float3 direction)
{
    return 1 / 4.0f * sqrt(5.0f/pi) * (3 * direction.z * direction.z - 1);
}
    
float Y21(float3 direction)
{
    return 1 / 2.0f * sqrt(15.0f/pi) * direction.x * direction.z;
}
    
float Y22(float3 direction)
{
    return 1 / 4.0f * sqrt(15.0f/pi) * (direction.x * direction.x - direction.y * direction.y);
}

float SHBasis(int index, float3 direciton)
{
    float shBasis = 0;
    switch(index)
    {
        case 0:
            shBasis = Y0();
            break;
        case 1:
            shBasis = Y1_1(direciton);
            break;
        case 2:
            shBasis = Y10(direciton);
            break;
        case 3:
            shBasis = Y11(direciton);
            break;
        case 4:
            shBasis = Y2_2(direciton);
            break;
        case 5:
            shBasis = Y2_1(direciton);
            break;
        case 6:
            shBasis = Y20(direciton);
            break;
        case 7:
            shBasis = Y21(direciton);
            break;
        case 8:
            shBasis = Y22(direciton);
            break;
    }
    return shBasis;
}

#endif