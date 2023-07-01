using Unity.Entities;
using Unity.Mathematics;

public struct MouseHit : IComponentData
{
    public float3 Position;
    public bool Changed;
}