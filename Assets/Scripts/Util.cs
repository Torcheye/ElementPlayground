using Unity.Mathematics;

public static class Util
{
    public static float4 Type2Color(in int type)
    {
        return type switch
        {
            0 => float4.zero,
            1 => new float4(0.761f, 0.698f, 0.502f, 1),
            _ => new float4(1, 0, 0, 1)
        };
    }
}