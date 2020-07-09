using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct RunnerConstantData : IComponentData
{
	public int matricesPerRunner;
	public float hipHeight;
	public float shoulderHeight;
	public float stanceWidth;
	public float stepDuration;
	public float legLength;
	public float xzDamping;
	public float spreadForce;
}
