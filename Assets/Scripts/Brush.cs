﻿using Unity.Entities;

public struct Brush : IComponentData
{
    public float BrushSize;
    public CellType Type;
}