using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RunnerBarMoveData : IComponentData
{
	public NativeArray<float3> points;
	public NativeArray<float3> prevPoints;
	public NativeArray<float> barLengths;
	public NativeArray<float3> footTargets;
	public NativeArray<float3> stepStartPositions;
	public NativeArray<float> footAnimTimers;
	public NativeArray<bool> feetAnimating;
}
