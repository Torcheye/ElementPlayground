using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

public struct CellBuffer : IBufferElementData
{
    public Entity CellEntity;
}