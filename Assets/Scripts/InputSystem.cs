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
        if (Camera.main != null && Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            var pos = Input.mousePosition;
            pos.z = 10;
            var brush = SystemAPI.GetSingleton<Brush>();
            
            var input = new InputCreateJob()
            {
                Position = Camera.main.ScreenToWorldPoint(pos),
                Type = brush.Type,
                Size = brush.BrushSize,
                Remove = Input.GetMouseButton(1)
            }.ScheduleParallel(state.Dependency);
            input.Complete();
        }
    }
}

[BurstCompile]
partial struct InputCreateJob : IJobEntity
{
    public float3 Position;
    public CellType Type;
    public float Size;
    public bool Remove;
    void Execute(CellAspect ca, in LocalTransform transform) 
    {
        if (math.distancesq(transform.Position, Position) < Size * Size)
        {
            if (Remove)
            {
                ca.Cell.ValueRW.Type = 0;
                ca.Color.ValueRW.Value = Util.Type2Color(0);
            }
            else
            {
                ca.Cell.ValueRW.Type = Type;
                ca.Color.ValueRW.Value = Util.Type2Color(Type);
            }
            
            ca.Cell.ValueRW.Updated = true;
        }
    }
}