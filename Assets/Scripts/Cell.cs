using Unity.Entities;
using UnityEngine;


public struct Cell : IComponentData
{
    public int Type;
    public int Id;
    public bool Updated;
}


