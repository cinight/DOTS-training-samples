using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct RunnerSpawnData : IComponentData
{
    public Entity runnerPrefab;
    public Entity barPrefab;
    public float distanceFromPit;
}
