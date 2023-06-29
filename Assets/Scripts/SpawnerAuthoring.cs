using UnityEngine;
using Unity.Entities;

class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float CellScale;
    public int CountX, CountY;
    public float Distance;
}

class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new Spawner
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            CellScale = authoring.CellScale,
            CountX = authoring.CountX,
            CountY = authoring.CountY,
            Distance = authoring.Distance
        });
    }
}