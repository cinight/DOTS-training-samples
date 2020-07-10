using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateAfter(typeof(RunnerFallSystem))]
public class BodyPartRenderSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Get runner arrays
        var constantDataType = GetComponentDataFromEntity<RunnerConstantData>(true);
        var runnerColorType = GetComponentDataFromEntity<RunnerColorData>(true);
        var runnerTimeType = GetComponentDataFromEntity<RunnerTimeData>(true);

        //Get buffers
        var bufferBars = GetBufferFromEntity<BufferBars>(true);
        var bufferBarThickness = GetBufferFromEntity<BufferBarThickness>(true);
        var bufferPoints = GetBufferFromEntity<BufferPoints>(true);
        var bufferPrevPoints = GetBufferFromEntity<BufferPrevPoints>(true);

        //Factors
        //int instancesPerBatch = 1023;
        float t = (UnityEngine.Time.time - UnityEngine.Time.fixedTime) / Time.fixedDeltaTime;
        
        //Update runner bars
        Entities.ForEach
        ((
            ref NonUniformScale sca, 
            ref Translation tran, 
            ref Rotation rot, 
            ref MaterialColor matCol, 
            in BelongsToRunnerData runnerEntityData,
            in BelongsToBarData barIdData
        ) => 
        {
            var barData = bufferBars[runnerEntityData.entity];
            var barThicknessData = bufferBarThickness[runnerEntityData.entity];
            var points = bufferPoints[runnerEntityData.entity];
            var prevPoints = bufferPrevPoints[runnerEntityData.entity];
            
            var constData = constantDataType[runnerEntityData.entity];
            var timeData = runnerTimeType[runnerEntityData.entity];
            var colorData = runnerColorType[runnerEntityData.entity];

            int j = barIdData.barID;

            float3 point1 = points[barData[j*2].bars].points;
            float3 point2 = points[barData[j*2 + 1].bars].points;
            float3 oldPoint1 = prevPoints[barData[j * 2].bars].prevPoints;
            float3 oldPoint2 = prevPoints[barData[j*2 + 1].bars].prevPoints;

            point1 += (point1 - oldPoint1) * t;
            point2 += (point2 - oldPoint2) * t;

            float3 delta = point2 - point1;
            float3 position = (point1 + point2) * .5f;
            quaternion rotation = quaternion.LookRotation(delta+0.001f,math.up());
            float3 scale = new float3(barThicknessData[j].barThicknesses*timeData.timeSinceSpawn,
                                        barThicknessData[j].barThicknesses*timeData.timeSinceSpawn,
                                        math.sqrt(delta.x*delta.x+delta.y*delta.y+delta.z*delta.z)*timeData.timeSinceSpawn);
            
            tran.Value = position;
            rot.Value = rotation;
            sca.Value = scale;
            matCol.Value = colorData.color;

        }).Run();
    }
}

//NEED OPTIMIZATION