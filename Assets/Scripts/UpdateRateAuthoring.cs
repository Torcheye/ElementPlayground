using Unity.Entities;
using UnityEngine;

public class UpdateRateAuthoring : MonoBehaviour
{
    public uint MilliSeconds;
}

public class UpdateRateBaker : Baker<UpdateRateAuthoring>
{
    public override void Bake(UpdateRateAuthoring authoring)
    {
        AddComponent<UpdateRate>(GetEntity(TransformUsageFlags.None), new UpdateRate()
        {
            MilliSeconds = authoring.MilliSeconds
        });
    }
}