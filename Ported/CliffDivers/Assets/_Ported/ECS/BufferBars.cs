using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferBars : IBufferElementData
{
    public int bars;
}
