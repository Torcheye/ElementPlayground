using System;
using Unity.Entities;
using UnityEngine;

public class CellAuthoring : MonoBehaviour
{
    public int Type;
    public int Id;
    public bool Updated;
}

public class CellBaker : Baker<CellAuthoring>
{
    public override void Bake(CellAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Cell
        {
            Type = authoring.Type,
            Id = -1,
            Updated = false
        });
    }
}