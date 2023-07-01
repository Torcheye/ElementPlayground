using Unity.Entities;
using UnityEngine;

public class BrushAuthoring : MonoBehaviour
{
    public float brushSize;
}

public class BrushBaker : Baker<BrushAuthoring>
{
    public override void Bake(BrushAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent<Brush>(entity,new Brush()
        {
            BrushSize = authoring.brushSize * 0.01f
        });
    }
}

