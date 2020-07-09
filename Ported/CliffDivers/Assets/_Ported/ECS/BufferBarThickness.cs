using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferBarThickness : IBufferElementData
{
    public float barThicknesses;
}
