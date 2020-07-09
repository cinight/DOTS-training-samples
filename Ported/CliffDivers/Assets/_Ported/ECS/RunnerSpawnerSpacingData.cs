using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RunnerSpawnerSpacingData : IComponentData
{
    public float Value;
    public float spawnAngle;
}
