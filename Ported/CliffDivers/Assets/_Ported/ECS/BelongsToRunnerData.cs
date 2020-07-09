using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BelongsToRunnerData : IComponentData
{
    public Entity entity;
}
