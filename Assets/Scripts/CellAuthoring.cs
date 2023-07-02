using System;
using Unity.Entities;
using UnityEngine;

public class CellAuthoring : MonoBehaviour
{
}

public class CellBaker : Baker<CellAuthoring>
{
    public override void Bake(CellAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Cell
        {
            Type = 0,
            Id = -1,
            Updated = false
        });
    }
}