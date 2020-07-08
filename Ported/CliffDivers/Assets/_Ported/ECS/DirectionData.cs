using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DirectionData : IComponentData
{
    public float3 Value;
}
