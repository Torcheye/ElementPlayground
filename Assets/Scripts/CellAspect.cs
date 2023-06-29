using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

public readonly partial struct CellAspect : IAspect
{
    public readonly RefRW<Cell> Cell;
    public readonly RefRW<URPMaterialPropertyBaseColor> Color;

    public void UpdateType(in int type)
    {
        Cell.ValueRW.Type = type;
        Color.ValueRW.Value = type switch
        {
            0 => float4.zero,
            1 => new float4(0, 1, 0, 0),
            _ => float4.zero
        };
    }
}