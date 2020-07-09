using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferPrevPoints : IBufferElementData
{
    public float3 prevPoints;
}
