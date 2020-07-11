using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateAfter(typeof(PitGeneratonSystem))]
public class RunnerSpawnSystem : SystemBase
{
    BeginSimulationEntityCommandBufferSystem m_EcbSystem;
    private EntityQuery q_Spawner;

   protected override void OnCreate()
   {
       q_Spawner = GetEntityQuery(ComponentType.ReadOnly<RunnerSpawnData>());
       m_EcbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
   }

    protected override void OnUpdate()
    {
        var spawnerEntities = q_Spawner.ToEntityArray(Allocator.TempJob);
        if(spawnerEntities.Length == 0)
        {
            spawnerEntities.Dispose();
            return;
        } 
        var spawnerEntity = spawnerEntities[0];
        var ecb = m_EcbSystem.CreateCommandBuffer();
        var bufferBarsLength = 11;

        //Spawn 2 Runner everyframe
        Entities.ForEach((Entity e, in RunnerSpawnData runnerSpawnData) => 
        {
            for(int i = 0; i<2; i++)
            {
                var newRunner = ecb.Instantiate(runnerSpawnData.runnerPrefab);
                var bufferBarEntities = ecb.AddBuffer<BufferBarEntities>(newRunner);

                //Spawn the bar cubes for each runner
                for(int k=0; k < bufferBarsLength; k++)
                {
                    var newBar = ecb.Instantiate(runnerSpawnData.barPrefab);
                    ecb.AddComponent(newBar,new BelongsToRunnerData{entity = newRunner});
                    ecb.AddComponent(newBar,new BelongsToBarData{barID = k});
                    bufferBarEntities.Add(new BufferBarEntities{entity = newBar});
                    
                    //FOR DEBUGGING
                    //EntityManager.SetName(newBar,"Runner Bar");
                }
            }
        }).Schedule();

        //Get spawner settings
        var spacingData = GetComponentDataFromEntity<RunnerSpawnerSpacingData>(false);
        var spawnData = GetComponentDataFromEntity<RunnerSpawnData>(true);
        var pitRadiusData = GetComponentDataFromEntity<PitRadiusData>(true);
        float spacing = spacingData[spawnerEntity].Value;
        float spawnAngle = spacingData[spawnerEntity].spawnAngle;
        float pitRadius = pitRadiusData[spawnerEntity].Value;
        float distanceFromPit = spawnData[spawnerEntity].distanceFromPit;
        spawnerEntities.Dispose();

        //Init runner
        float time = (float)Time.ElapsedTime;
        Random _random = new Random((uint)(173859*time));
        Entities.WithAll<NotInitialisedTag>()
        .ForEach((
            Entity e,
            ref Translation tran, 
            ref RunnerColorData colData, 
            ref RunnerConstantData constData,
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

            //Init constant data
            constData.hipHeight = 1.8f;
            constData.shoulderHeight = 3.5f;
            constData.stepDuration = _random.NextFloat(.25f,.33f);
            constData.xzDamping = _random.NextFloat(0f,1f)*.02f+.002f;
            constData.spreadForce = _random.NextFloat(.0005f,.0015f);
            constData.stanceWidth = .35f;
            //constData.matricesPerRunner = bufferBarsLength / 2;
            constData.legLength = math.sqrt(constData.hipHeight * constData.hipHeight + constData.stanceWidth * constData.stanceWidth)*1.1f;

            //Remove initialize tag
            ecb.RemoveComponent<NotInitialisedTag>(e);

        }).Run();

        //Set spawner settings
        Entities.ForEach((ref RunnerSpawnerSpacingData runnerSpawnerSpacingData) => 
        {
            runnerSpawnerSpacingData.spawnAngle = spawnAngle;
            runnerSpawnerSpacingData.Value = spacing;

        }).Schedule();
        
        m_EcbSystem.AddJobHandleForProducer(this.Dependency);

        //Just for debug
        //this.Enabled = false;
    }
}