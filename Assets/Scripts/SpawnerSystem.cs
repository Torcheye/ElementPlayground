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
public partial struct SpawnerSystem : ISystem, ISystemStartStop
{
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
        var bufferEntity = entityManager.CreateEntity();
        entityManager.AddBuffer<CellBuffer>(bufferEntity);
        
        Spawner spawner = SystemAPI.GetSingleton<Spawner>();
        
        int id = 0;
        float startX = -(spawner.CountX - 1) * 0.5f * spawner.Distance;
        float startY = -(spawner.CountY - 1) * 0.5f * spawner.Distance;
        for (float y = 0; y < spawner.CountY; y ++)
        {
            for (float x = 0; x <spawner.CountX; x ++)
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

    public void OnStopRunning(ref SystemState state) {}
}



