using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

public readonly partial struct CellAspect : IAspect
{
    public readonly RefRW<Cell> Cell;
    public readonly RefRW<URPMaterialPropertyBaseColor> Color;
}