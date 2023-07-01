using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Brush>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (Camera.main != null && Input.GetMouseButton(0))
        {
            var pos = Input.mousePosition;
            pos.z = 10;
            // process input
            var input = new InputCreateJob()
            {
                Position = Camera.main.ScreenToWorldPoint(pos),
                Size = SystemAPI.GetSingleton<Brush>().BrushSize
            }.ScheduleParallel(state.Dependency);
            input.Complete();
        }
    }
}

[BurstCompile]
partial struct InputCreateJob : IJobEntity
{
    public float3 Position;
    public float Size;
    void Execute(CellAspect ca, in LocalTransform transform) 
    {
        if (math.distancesq(transform.Position, Position) < Size * Size)
        {
            ca.Cell.ValueRW.Type = 1;
            ca.Cell.ValueRW.Updated = true;
            ca.Color.ValueRW.Value = Util.Type2Color(1);
        }
    }
}