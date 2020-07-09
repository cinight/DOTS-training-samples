using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RunnerColorData : IComponentData
{
    public float4 color;
}
