using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial struct CellUpdateSystem : ISystem, ISystemStartStop
{
    private int _countX, _countY;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
    }
    
    public void OnStartRunning(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<Spawner>();
        _countX = spawner.CountX;
        _countY = spawner.CountY;
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (CellAspect cellAspect in SystemAPI.Query<CellAspect>())
        {
            if (cellAspect.Cell.ValueRO.Updated) continue;
            if (cellAspect.Cell.ValueRO.Type == 0) continue;
            
            // 1. check down
            int n = GetNeighbor(cellAspect.Cell.ValueRO.Id, 0, -1);
            if (n == -1) continue;

            var handle = new CellUpdateJob
            {
                TargetId = n,
                TargetFrom = 0,
                TargetTo = 1
            }.ScheduleParallel(state.Dependency);
            handle.Complete(); 
            
            //update self
            cellAspect.Cell.ValueRW.Updated = true;
            cellAspect.Cell.ValueRW.Type = 0;
            cellAspect.Color.ValueRW.Value = Type2Color(0);
        }

        new CellResetJob().ScheduleParallel();
    }
    
    [BurstCompile]
    private int GetNeighbor(in int id, in int x, in int y)
    {
        int row = id / _countX;
        int col = id % _countX;
        
        if (row == 0 && y < 0 || row == _countY - 1 && y > 0 ||
            col == 0 && x < 0 || col == _countX - 1 && x > 0)
        {
            return -1;
        }

        return id + y * _countX + x;
    }

    public void OnStopRunning(ref SystemState state) {}

    /// <summary>
    /// Update target cell
    /// </summary>
    [BurstCompile]
    partial struct CellUpdateJob : IJobEntity
    {
        public int TargetId;
        public int TargetFrom;
        public int TargetTo;

        void Execute(CellAspect c)
        {
            // update target
            
            if (c.Cell.ValueRO.Id == TargetId)
            {
                if (!c.Cell.ValueRO.Updated)
                {
                    if (c.Cell.ValueRO.Type == TargetFrom)
                    {
                        c.Cell.ValueRW.Updated = true;
                        c.Cell.ValueRW.Type = TargetTo;
                        c.Color.ValueRW.Value = Type2Color(TargetTo);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reset cells for next update
    /// </summary>
    [BurstCompile]
    partial struct CellResetJob : IJobEntity
    {
        void Execute(CellAspect c)
        {
            if (!c.Cell.ValueRO.Updated)
                return;
            c.Cell.ValueRW.Updated = false;
        }
    }

    private static float4 Type2Color(in int type)
    {
        return type switch
        {
            0 => float4.zero,
            1 => new float4(0, 1, 1, 1),
            _ => new float4(1, 0, 0, 1)
        };
    }
}