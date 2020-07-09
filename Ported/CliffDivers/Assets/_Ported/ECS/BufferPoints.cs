using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferPoints : IBufferElementData
{
    public float3 points;
}
