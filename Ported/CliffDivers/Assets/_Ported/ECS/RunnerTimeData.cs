using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RunnerTimeData : IComponentData
{
    public float timeSinceSpawn;
}
