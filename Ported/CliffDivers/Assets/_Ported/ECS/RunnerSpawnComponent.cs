using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System;

[Serializable]
public struct RunnerSpawnData : IComponentData
{
    public Entity runnerPrefab;
    public Entity barPrefab;
    public float distanceFromPit;
}

[RequiresEntityConversion]
public class RunnerSpawnComponent : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject runnerPrefab;
    public GameObject barPrefab;
    public float distanceFromPit;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(runnerPrefab);
        referencedPrefabs.Add(barPrefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new RunnerSpawnData
        {
            runnerPrefab = conversionSystem.GetPrimaryEntity(runnerPrefab),
            barPrefab = conversionSystem.GetPrimaryEntity(barPrefab),
            distanceFromPit = distanceFromPit
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}