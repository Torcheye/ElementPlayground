using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Rendering;

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
        var entityManager = state.EntityManager;
        state.World.GetExistingSystemManaged<VariableRateSimulationSystemGroup>().RateManager = new RateUtils.VariableRateManager(SystemAPI.GetSingleton<UpdateRate>().MilliSeconds);
        Spawner spawner = SystemAPI.GetSingleton<Spawner>();
        entityManager.CreateSingleton<MouseHit>();
        entityManager.CreateSingleton<Brush>();
        
        var bufferEntity = entityManager.CreateEntity();
        entityManager.AddBuffer<CellBuffer>(bufferEntity);

        int id = 0;
        float startX = -(spawner.CountX - 1) * 0.5f * spawner.CellScale;
        float startY = -(spawner.CountY - 1) * 0.5f * spawner.CellScale;
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
                
                Entity cell = entityManager.Instantiate(spawner.Prefab);
                SystemAPI.SetComponent(cell, cellComponent);
                SystemAPI.SetComponent(cell, LocalTransform.FromPositionRotationScale(
                    new float3(startX + x * spawner.CellScale, startY + y * spawner.CellScale, 0), quaternion.identity, spawner.CellScale));
                entityManager.AddComponentData(cell, new URPMaterialPropertyBaseColor() { Value = float4.zero });

                SystemAPI.GetBuffer<CellBuffer>(bufferEntity).Add(new CellBuffer() { CellEntity = cell});

                id++;
            } 
        }
    }

    public void OnStopRunning(ref SystemState state) {}
}



