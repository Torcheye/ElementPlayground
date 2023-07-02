using Unity.Mathematics;

public static class Util
{
    public static float4 Type2Color(in CellType type)
    {
        return type switch
        {
            0 => float4.zero,  
            CellType.Sand => new float4(0.761f, 0.698f, 0.502f, 1),
            CellType.Water => new float4(0.353f, 0.737f, 0.847f, 1),
            _ => new float4(1, 0, 0, 1)
        };
    }
}