using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial struct CellUpdateSystem : ISystem, ISystemStartStop
{
    private int _countX, _countY;
    private EntityQuery _query;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
    }
    
    public void OnStartRunning(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<Spawner>();
        _countX = spawner.CountX;
        _countY = spawner.CountY;
        _query = SystemAPI.QueryBuilder().WithAspect<CellAspect>().Build();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var current = _query.ToComponentDataArray<Cell>(Allocator.TempJob);

        var handle = new CellJob()
        {
            _countX = _countX,
            _countY = _countY,
            Current = current
        }.ScheduleParallel(state.Dependency);
        handle.Complete();
        current.Dispose();

        //new CellResetJob().ScheduleParallel();
    }

    public void OnStopRunning(ref SystemState state) {}
    
    [BurstCompile]
    partial struct CellJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Cell> Current;
        public int _countX, _countY;
        
        public void Execute(CellAspect ca)
        {
            int i = ca.Cell.ValueRO.Id;

            if (ca.Cell.ValueRO.Type == 0)
            {
                // rule sand
                if (GetNeighbor(i, 0, 1) != -1 && Current[GetNeighbor(i, 0, 1)].Type == 1)
                {
                    ca.Cell.ValueRW.Type = 1;
                    ca.Color.ValueRW.Value = Type2Color(1);
                }
            }
            else if (ca.Cell.ValueRO.Type == 1)
            {
                // rule sand
                if (GetNeighbor(i, 0, -1) != -1 && Current[GetNeighbor(i, 0, -1)].Type == 0)
                {
                    ca.Cell.ValueRW.Type = 0;
                    ca.Color.ValueRW.Value = Type2Color(0);
                }
                // else if (GetNeighbor(i, -1, -1) != -1 && Current[GetNeighbor(i, -1, -1)].Type == 0)
                // {
                //     ca.Cell.ValueRW.Type = 0;
                //     ca.Color.ValueRW.Value = Type2Color(0);
                // }
                // else if (GetNeighbor(i, 1, -1) != -1 && Current[GetNeighbor(i, 1, -1)].Type == 0)
                // {
                //     ca.Cell.ValueRW.Type = 0;
                //     ca.Color.ValueRW.Value = Type2Color(0);
                // }
            }
            
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
    }

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