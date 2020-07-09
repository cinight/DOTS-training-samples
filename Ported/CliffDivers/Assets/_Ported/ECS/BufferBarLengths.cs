using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferBarLengths : IBufferElementData
{
    public float barLengths;
}
