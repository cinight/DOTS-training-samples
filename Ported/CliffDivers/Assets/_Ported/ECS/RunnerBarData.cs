using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RunnerBarData : IComponentData
{
	public NativeArray<int> bars;
	public NativeArray<float> barThicknesses;
}
