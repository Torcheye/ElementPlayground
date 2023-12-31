﻿using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        var length = _query.CalculateEntityCount();
        var current = _query.ToComponentDataArray<Cell>(Allocator.TempJob);
        var change = new NativeArray<CellChange>(length, Allocator.TempJob);

        var handle = new CellJob()
        {
            CountX = _countX,
            CountY = _countY,
            Current = current,
            Change = change
        }.Schedule(length, 64, state.Dependency);
        handle.Complete();
        current.Dispose();
        
        var buffer = SystemAPI.GetSingletonBuffer<CellBuffer>().Reinterpret<Entity>();
        foreach (CellChange c in change)
        {
            if (!c.Set) continue;
            if (SystemAPI.GetComponentRO<Cell>(buffer[c.Target0.Id]).ValueRO.Updated || 
                SystemAPI.GetComponentRO<Cell>(buffer[c.Target1.Id]).ValueRO.Updated)
                continue;
            SystemAPI.SetComponent(buffer[c.Target0.Id], c.Target0);
            SystemAPI.SetComponent(buffer[c.Target0.Id], new URPMaterialPropertyBaseColor{Value = Util.Type2Color(c.Target0.Type)});
            SystemAPI.SetComponent(buffer[c.Target1.Id], c.Target1);
            SystemAPI.SetComponent(buffer[c.Target1.Id], new URPMaterialPropertyBaseColor{Value = Util.Type2Color(c.Target1.Type)});
        }
        
        change.Dispose();

        new CellResetJob().ScheduleParallel();
    }

    public void OnStopRunning(ref SystemState state) { }
    
    [BurstCompile]
    struct CellJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Cell> Current;
        [WriteOnly] public NativeArray<CellChange> Change; 
        public int CountX, CountY;
        
        public void Execute(int i)
        {
            var change = new CellChange() { Set = false };
            
            // sand
            if (Current[i].Type == CellType.Sand)
            {
                var low = GetNeighbor(i, 0, -1);
                var lowleft = GetNeighbor(i, -1, -1);
                var lowright = GetNeighbor(i, 1, -1);
                
                change.Target1 = new Cell{ Id = i, Type = 0, Updated = true };
                change.Set = true;
                
                if (low != -1 && Current[low].Type == 0)
                {
                    change.Target0 = new Cell{ Id = low, Type = CellType.Sand, Updated = true };
                }
                else if (lowleft != -1 && Current[lowleft].Type == 0)
                {
                    change.Target0 = new Cell{ Id = lowleft, Type = CellType.Sand, Updated = true };
                }
                else if (lowright != -1 && Current[lowright].Type == 0)
                {
                    change.Target0 = new Cell{ Id = lowright, Type = CellType.Sand, Updated = true };
                }
                else
                {
                    change.Set = false;
                }
            }
            else if (Current[i].Type == CellType.Water)
            {
                var low = GetNeighbor(i, 0, -1);
                var lowleft = GetNeighbor(i, -1, -1);
                var lowright = GetNeighbor(i, 1, -1);
                var left = GetNeighbor(i, -1, 0);
                var right = GetNeighbor(i, 1, 0);
                
                change.Target1 = new Cell{ Id = i, Type = 0, Updated = true };
                change.Set = true;
                
                if (low != -1 && Current[low].Type == 0)
                {
                    change.Target0 = new Cell{ Id = low, Type = CellType.Water, Updated = true };
                }
                else if (lowleft != -1 && Current[lowleft].Type == 0)
                {
                    change.Target0 = new Cell{ Id = lowleft, Type = CellType.Water, Updated = true };
                }
                else if (lowright != -1 && Current[lowright].Type == 0)
                {
                    change.Target0 = new Cell{ Id = lowright, Type = CellType.Water, Updated = true };
                }
                else if (left != -1 && Current[left].Type == 0)
                {
                    change.Target0 = new Cell{ Id = left, Type = CellType.Water, Updated = true };
                }
                else if (right != -1 && Current[right].Type == 0)
                {
                    change.Target0 = new Cell{ Id = right, Type = CellType.Water, Updated = true };
                }
                else
                {
                    change.Set = false;
                }
            }
            
            Change[i] = change;
        }

        private int GetNeighbor(in int id, in int x, in int y)
        {
            int row = id / CountX;
            int col = id % CountX;
        
            if (row == 0 && y < 0 || row == CountY - 1 && y > 0 ||
                col == 0 && x < 0 || col == CountX - 1 && x > 0)
                return -1;

            return id + y * CountX + x;
        }
    }

    /// <summary>
    /// Reset cells for next update
    /// </summary>
    [BurstCompile]
    partial struct CellResetJob : IJobEntity
    {
        void Execute(ref Cell c)
        {
            c.Updated = false;
        }
    }
}