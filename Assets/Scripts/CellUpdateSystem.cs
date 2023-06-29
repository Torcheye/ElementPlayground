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
public partial struct CellUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
    }
    
    
    public void OnUpdate(ref SystemState state)
    {
        var eBuffer = SystemAPI.GetSingletonBuffer<CellBuffer>().Reinterpret<Entity>();
        var spawner = SystemAPI.GetSingleton<Spawner>();

        new CellUpdateJob()
        {
            eBuffer = eBuffer,
            entityManager = state.EntityManager,
            spawner = spawner
        }.Run();

        new CellResetJob().ScheduleParallel();
    }
}

partial struct CellUpdateJob : IJobEntity
{
    public DynamicBuffer<Entity> eBuffer;
    public EntityManager entityManager;
    public Spawner spawner;

    [BurstCompile]
    void Execute(ref Cell c)
    {
        int id = c.Id;
        if (c.Updated)
            return;
        if (c.Type == 0)
            return;

        int n = GetNeighbor(id, 1, 0);

        if (n == -1)
            return;
        if (entityManager.GetComponentData<Cell>(eBuffer[n]).Updated)
            return;

        // update neighbor
        entityManager.SetComponentData(eBuffer[n], new Cell()
        {
            Id = n,
            Type = 1,
            Updated = true
        });
        entityManager.SetComponentData(eBuffer[n], new URPMaterialPropertyBaseColor()
        {
            Value = new float4(0, 1, 1, 1)
        });

        // update self
        c.Type = 0;
        c.Updated = true;
        entityManager.SetComponentData(eBuffer[id], new URPMaterialPropertyBaseColor()
        {
            Value = float4.zero
        });
    }
    
    [BurstCompile]
    private int GetNeighbor(in int id, in int x, in int y)
    {
        int row = id / spawner.CountX;
        int col = id % spawner.CountX;
        
        if (row == 0 && y < 0 || row == spawner.CountY - 1 && y > 0 ||
            col == 0 && x < 0 || col == spawner.CountX - 1 && x > 0)
        {
            return -1;
        }

        return id + y * spawner.CountX + x;
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
        if (!c.Updated)
            return;
        c.Updated = false;
    }
}