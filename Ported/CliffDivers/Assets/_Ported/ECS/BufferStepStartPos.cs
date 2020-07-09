using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferStepStartPos : IBufferElementData
{
    public float3 stepStartPositions;
}
