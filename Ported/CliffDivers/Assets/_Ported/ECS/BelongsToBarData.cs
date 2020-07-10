using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BelongsToBarData : IComponentData
{
    public int barID;
}
