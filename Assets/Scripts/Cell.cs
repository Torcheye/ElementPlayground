using Unity.Entities;
using UnityEngine;

public struct Cell : IComponentData
{
    public CellType Type;
    public int Id;
    public bool Updated;
}

public enum CellType
{
    None,
    Sand,
    Water
}
