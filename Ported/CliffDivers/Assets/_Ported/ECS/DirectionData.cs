﻿using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct DirectionData : IComponentData
{
    public float3 Value;
}
