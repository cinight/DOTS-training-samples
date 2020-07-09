using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class RunnerSpawnSystem : SystemBase
{
    Entity spawnerEntity;
    NativeArray<Entity> newEntities;

    protected override void OnUpdate()
    {
        //Spawn 2 Entites everyframe
        Entities.WithStructuralChanges().ForEach((Entity e, in RunnerSpawnData runnerSpawnData) => 
        {
            spawnerEntity = e;
            newEntities = EntityManager.Instantiate(runnerSpawnData.runnerPrefab,2,Allocator.Temp);
        }).Run();

        //Get spawner settings
        var spacingData = GetComponentDataFromEntity<RunnerSpawnerSpacingData>(false);
        var spawnData = GetComponentDataFromEntity<RunnerSpawnData>(true);
        var pitRadiusData = GetComponentDataFromEntity<PitRadiusData>(true);
        float spacing = spacingData[spawnerEntity].Value;
        float spawnAngle = spacingData[spawnerEntity].spawnAngle;
        float pitRadius = pitRadiusData[spawnerEntity].Value;
        float distanceFromPit = spawnData[spawnerEntity].distanceFromPit;

        //Init runner
        float time = (float)Time.ElapsedTime;
        Random _random = new Random((uint)(173859*time));
        Entities.WithAll<NotInitialisedTag>()
        .ForEach((
            ref Translation tran, 
            ref RunnerColorData colData, 
            ref RunnerConstantData constData,
            ref RunnerBarData barData,
            ref RunnerBarMoveData barMoveData,
            ref RunnerTimeData timeData
        ) => 
        {
            //Init runner position
            float spawnRadius = pitRadius + distanceFromPit;
		    float3 pos = new float3(math.cos(spawnAngle) * spawnRadius,0f,math.sin(spawnAngle) * spawnRadius);
            tran.Value = pos;

            //Spawning position mode
            int mode = (int)math.floor(time / 8f);
            if (mode % 2 == 0) 
            {
                spawnAngle = _random.NextFloat(0f,1f) * 2f * math.PI;
                spacing = _random.NextFloat(0.04f,math.PI*.4f);
            }
            else
            {
                spawnAngle += spacing;
            }

            //Init time data
            timeData.timeSinceSpawn = 0f;

            //Init runner color
            float hue = math.sin(time)*.5f+.5f;
            float sat = math.sin(time / 1.37f)*.5f+.5f;
            float value = math.sin(time / 1.618f) * .5f + .5f;
            UnityEngine.Color col = UnityEngine.Random.ColorHSV(
                hue, hue, 
                sat*.2f+.1f, sat*.4f+.15f,
                value*.15f+.25f, value*.35f+.25f
                );
            colData.color = new float4(col.r,col.g,col.b,col.a);

            //Bar constant arrays
			var bars = new int[] { 0,1,    //thigh 1
							   1,2,    // shin 1
							   0,3,    // thigh 2
							   3,4,    // shin 2
							   0,5,    // lower spine
							   5,6,    // upper spine
							   6,7,    // bicep 1
							   7,8,    // forearm 1
							   6,9,    // bicep 2
							   9,10,   // forearm 2
							   6,11};  // head
			barData.bars = new NativeArray<int>();
            barData.bars.CopyFrom(bars);
            var barThicknesses = new float[bars.Length / 2];
			for (int i = 0; i < barThicknesses.Length - 1; i++) 
            {
				barThicknesses[i] = .2f;
			}
			barThicknesses[barThicknesses.Length - 1] = .4f;
            barData.barThicknesses.CopyFrom(barThicknesses);

            //Bar movement arrays
            barMoveData.points = new NativeArray<float3>(12,Allocator.Persistent);
			barMoveData.prevPoints = new NativeArray<float3>(barMoveData.points.Length,Allocator.Persistent);
			barMoveData.barLengths = new NativeArray<float>(bars.Length / 2,Allocator.Persistent);
			barMoveData.footTargets = new NativeArray<float3>(2,Allocator.Persistent);
			barMoveData.stepStartPositions = new NativeArray<float3>(2,Allocator.Persistent);
			barMoveData.footAnimTimers = new NativeArray<float>(2,Allocator.Persistent);
			barMoveData.feetAnimating = new NativeArray<bool>(2,Allocator.Persistent);
            barMoveData.footAnimTimers[0] = _random.NextFloat(0f,1f);
            barMoveData.footAnimTimers[1] = _random.NextFloat(0f,1f);
            barMoveData.feetAnimating[0] = true;
            barMoveData.feetAnimating[1] = true;

            //Init constant data
            constData.hipHeight = 1.8f;
            constData.shoulderHeight = 3.5f;
            constData.stepDuration = _random.NextFloat(.25f,.33f);
            constData.xzDamping = _random.NextFloat(0f,1f)*.02f+.002f;
            constData.spreadForce = _random.NextFloat(.0005f,.0015f);
            constData.stanceWidth = .35f;
            constData.matricesPerRunner = bars.Length / 2;
            constData.legLength = math.sqrt(constData.hipHeight * constData.hipHeight + constData.stanceWidth * constData.stanceWidth)*1.1f;

        }).Run();

        //Set spawner settings
        EntityManager.SetComponentData(spawnerEntity,new RunnerSpawnerSpacingData{ spawnAngle = spawnAngle , Value = spacing });

        //Remove initialize tag
        EntityManager.RemoveComponent(newEntities,typeof(NotInitialisedTag));
    }
}