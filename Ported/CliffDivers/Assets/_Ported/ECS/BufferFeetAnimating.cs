using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BufferFeetAnimating : IBufferElementData
{
    public bool feetAnimating;
}
