using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PitDepthData : IComponentData
{
    public float Value;
}
