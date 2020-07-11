using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(8)]
public struct BufferBarEntities : IBufferElementData
{
    public Entity entity;
}

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

[DisallowMultipleComponent]
[RequiresEntityConversion]
[ConverterVersion("joe", 1)]
public class BufferBarComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var bufferBars = dstManager.AddBuffer<BufferBars>(entity);
        bufferBars.Add(new BufferBars{bars = 0}); bufferBars.Add(new BufferBars{bars = 1});//thigh 1
        bufferBars.Add(new BufferBars{bars = 1}); bufferBars.Add(new BufferBars{bars = 2});// shin 1
        bufferBars.Add(new BufferBars{bars = 0}); bufferBars.Add(new BufferBars{bars = 3});// thigh 2
        bufferBars.Add(new BufferBars{bars = 3}); bufferBars.Add(new BufferBars{bars = 4});// shin 2
        bufferBars.Add(new BufferBars{bars = 0}); bufferBars.Add(new BufferBars{bars = 5});// lower spine
        bufferBars.Add(new BufferBars{bars = 5}); bufferBars.Add(new BufferBars{bars = 6});// upper spine
        bufferBars.Add(new BufferBars{bars = 6}); bufferBars.Add(new BufferBars{bars = 7});// bicep 1
        bufferBars.Add(new BufferBars{bars = 7}); bufferBars.Add(new BufferBars{bars = 8});// forearm 1
        bufferBars.Add(new BufferBars{bars = 6}); bufferBars.Add(new BufferBars{bars = 9});// bicep 2
        bufferBars.Add(new BufferBars{bars = 9}); bufferBars.Add(new BufferBars{bars = 10});// forearm 2
        bufferBars.Add(new BufferBars{bars = 6}); bufferBars.Add(new BufferBars{bars = 11});// head

        int bufferBarsLength = 22;
        var bufferBarsThickness = dstManager.AddBuffer<BufferBarThickness>(entity);
        int barThicknessesCount = bufferBarsLength / 2;
        for (int i = 0; i < barThicknessesCount; i++) 
        {
            bufferBarsThickness.Add(new BufferBarThickness{barThicknesses = .2f});
        }
        bufferBarsThickness[barThicknessesCount - 1] = new BufferBarThickness{barThicknesses = .4f};

        //Bar move arrays
        var points = dstManager.AddBuffer<BufferPoints>(entity);
        for (int i = 0; i < 12; i++) points.Add(new BufferPoints{points = 0f});

        var prevPoints = dstManager.AddBuffer<BufferPrevPoints>(entity);
        for (int i = 0; i < 12; i++)prevPoints.Add(new BufferPrevPoints{prevPoints = 0f});

        var barLengths = dstManager.AddBuffer<BufferBarLengths>(entity);
        for (int i = 0; i < bufferBarsLength / 2; i++) barLengths.Add(new BufferBarLengths{barLengths = 0f});

        var footTargets = dstManager.AddBuffer<BufferFootTargets>(entity);
        for (int i = 0; i < 2; i++) footTargets.Add(new BufferFootTargets{footTargets = 0f});

        var stepStartPos = dstManager.AddBuffer<BufferStepStartPos>(entity);
        for (int i = 0; i < 2; i++) stepStartPos.Add(new BufferStepStartPos{stepStartPositions = 0f});

        var footAnimTimers = dstManager.AddBuffer<BufferFootAnimTimers>(entity);
        for (int i = 0; i < 2; i++) footAnimTimers.Add(new BufferFootAnimTimers{footAnimTimers = UnityEngine.Random.value});

        var feetAnimating = dstManager.AddBuffer<BufferFeetAnimating>(entity);
        for (int i = 0; i < 2; i++) feetAnimating.Add(new BufferFeetAnimating{feetAnimating = true});

        #if UNITY_EDITOR
        dstManager.SetName(entity,"Runner");
        Debug.Log("converted");
        #endif
    }
}
