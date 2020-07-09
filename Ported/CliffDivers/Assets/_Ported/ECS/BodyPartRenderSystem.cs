using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class BodyPartRenderSystem : SystemBase
{
   private EntityQuery qRunner;
   
   protected override void OnCreate()
   {
       qRunner = GetEntityQuery(ComponentType.ReadOnly<RunnerConstantData>(),ComponentType.ReadOnly<RunnerBarData>());
   }

    protected override void OnUpdate()
    {
        //Get runner arrays
        //var runnerEntities = qRunner.ToEntityArray(Allocator.TempJob);
        var constantDataType = GetComponentDataFromEntity<RunnerConstantData>(true);
        var barDataType = GetComponentDataFromEntity<RunnerBarData>(true);
        var barMoveDataType = GetComponentDataFromEntity<RunnerBarMoveData>(true);
        var runnerColorType = GetComponentDataFromEntity<RunnerColorData>(true);
        var runnerTimeType = GetComponentDataFromEntity<RunnerTimeData>(true);

        //Factors
        //int instancesPerBatch = 1023;
        float t = (UnityEngine.Time.time - UnityEngine.Time.fixedTime) / Time.fixedDeltaTime;
        
        //Update runner bars
        Entities.ForEach((ref NonUniformScale sca, ref Translation tran, ref Rotation rot, ref MaterialColor matCol, in BelongsToRunnerData runnerEntityData) => 
        {
            var barData = barDataType[runnerEntityData.entity];
            var moveData = barMoveDataType[runnerEntityData.entity];
            var colorData = runnerColorType[runnerEntityData.entity];
            var constData = constantDataType[runnerEntityData.entity];
            var timeData = runnerTimeType[runnerEntityData.entity];

            //int i = for each runner

			for (int j = 0; j < barData.bars.Length/2; j++) 
			{
				float3 point1 = moveData.points[barData.bars[j*2]];
				float3 point2 = moveData.points[barData.bars[j*2 + 1]];
				float3 oldPoint1 = moveData.prevPoints[barData.bars[j * 2]];
				float3 oldPoint2 = moveData.prevPoints[barData.bars[j*2 + 1]];

				point1 += (point1 - oldPoint1) * t;
				point2 += (point2 - oldPoint2) * t;

				float3 delta = point2 - point1;
				float3 position = (point1 + point2) * .5f;
				quaternion rotation = quaternion.LookRotation(delta,math.up());
				float3 scale = new float3(barData.barThicknesses[j]*timeData.timeSinceSpawn,
											barData.barThicknesses[j]*timeData.timeSinceSpawn,
											math.sqrt(delta.x*delta.x+delta.y*delta.y+delta.z*delta.z)*timeData.timeSinceSpawn);
				//int index = i * constData.matricesPerRunner + j;
				//matrices[index/instancesPerBatch][index%instancesPerBatch] = Matrix4x4.TRS(position,rotation,scale);
                tran.Value = position;
                rot.Value = rotation;
                sca.Value = scale;
				//colors[index / instancesPerBatch][index % instancesPerBatch] = colorData.color;
                matCol.Value = colorData.color;
			}
        }).Schedule();
    }
}
