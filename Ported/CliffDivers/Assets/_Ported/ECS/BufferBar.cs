using System;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct BufferBars : IBufferElementData
{
    public int bars;
}

[InternalBufferCapacity(8)]
public struct BufferBarLengths : IBufferElementData
{
    public float barLengths;
}

[InternalBufferCapacity(8)]
public struct BufferBarThickness : IBufferElementData
{
    public float barThicknesses;
}

[InternalBufferCapacity(8)]
public struct BufferFeetAnimating : IBufferElementData
{
    public bool feetAnimating;
}

[InternalBufferCapacity(8)]
public struct BufferFootAnimTimers : IBufferElementData
{
    public float footAnimTimers;
}

[InternalBufferCapacity(8)]
public struct BufferFootTargets : IBufferElementData
{
    public float3 footTargets;
}

[InternalBufferCapacity(8)]
public struct BufferPoints : IBufferElementData
{
    public float3 points;
}

[InternalBufferCapacity(8)]
public struct BufferPrevPoints : IBufferElementData
{
    public float3 prevPoints;
}

[InternalBufferCapacity(8)]
public struct BufferStepStartPos : IBufferElementData
{
    public float3 stepStartPositions;
}