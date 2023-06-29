using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial struct SpawnerSystem : ISystem, ISystemStartStop
{
    private int _countX, _countY;
    public Entity bufferEntity;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UpdateRate>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Spawner>();
    }
    
    public void OnStartRunning(ref SystemState state)
    {
        state.World.GetExistingSystemManaged<VariableRateSimulationSystemGroup>().RateManager = new RateUtils.VariableRateManager(SystemAPI.GetSingleton<UpdateRate>().MilliSeconds);
        
        var entityManager = state.EntityManager;
        bufferEntity = entityManager.CreateEntity();
        entityManager.AddBuffer<CellBuffer>(bufferEntity);
        
        Spawner spawner = SystemAPI.GetSingleton<Spawner>();
        _countX = spawner.CountX;
        _countY = spawner.CountY;
        
        int id = 0;
        float startX = -(_countX - 1) * 0.5f * spawner.Distance;
        float startY = -(_countY - 1) * 0.5f * spawner.Distance;
        for (float y = 0; y < _countY; y ++)
        {
            for (float x = 0; x <_countX; x ++)
            {
                Cell cellComponent = new Cell
                {
                    Id = id,
                    Type = 0,
                    Updated = false
                };
                if (id == 0)
                {
                    cellComponent.Type = 1;
                }

                Entity cell = entityManager.Instantiate(spawner.Prefab);
                SystemAPI.SetComponent(cell, cellComponent);
                SystemAPI.SetComponent(cell, LocalTransform.FromPositionRotationScale(
                    new float3(startX + x * spawner.Distance, startY + y * spawner.Distance, 0), quaternion.identity, spawner.CellScale));
                var color = new URPMaterialPropertyBaseColor() { Value = float4.zero };
                entityManager.AddComponentData(cell, color);

                SystemAPI.GetBuffer<CellBuffer>(bufferEntity).Add(new CellBuffer() { CellEntity = cell});

                id++;
            } 
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var buffer = SystemAPI.GetBuffer<CellBuffer>(bufferEntity);
        var eBuffer = buffer.Reinterpret<Entity>();

        foreach (RefRW<Cell> c in SystemAPI.Query<RefRW<Cell>>())
        {
            int id = c.ValueRO.Id;
            if (c.ValueRO.Updated)
                continue;
            if (c.ValueRO.Type == 0) 
                continue;
            
            int n = GetNeighbor(id, 1, 0);

            if (n == -1) 
                continue;
            if (SystemAPI.GetComponentRO<Cell>(eBuffer[n]).ValueRO.Updated)
                continue;
                    
            // set neighbor
            SystemAPI.SetComponent(eBuffer[n], new Cell()
            {
                Id = n,
                Type = 1,
                Updated = true
            });
            SystemAPI.SetComponent(eBuffer[n], new URPMaterialPropertyBaseColor()
            {
                Value = new float4(0, 1, 1, 1)
            });
                    
            // set self
            c.ValueRW.Type = 0;
            c.ValueRW.Updated = true;
            SystemAPI.SetComponent(eBuffer[id], new URPMaterialPropertyBaseColor()
            {
                Value = float4.zero
            });
        }

        // reset
        foreach (RefRW<Cell> c in SystemAPI.Query<RefRW<Cell>>())
        {
            if (!c.ValueRO.Updated)
                continue;
            c.ValueRW.Updated = false;
        }
    }

    public void OnStopRunning(ref SystemState state) {}
}