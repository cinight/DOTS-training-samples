using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PitGenData : IComponentData
{
    public int pitRingCount;
    public int quadsPerRing;
}
