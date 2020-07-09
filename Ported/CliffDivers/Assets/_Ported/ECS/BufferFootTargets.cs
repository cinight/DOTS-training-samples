using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferFootTargets : IBufferElementData
{
    public float3 footTargets;
}
