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

    protected override void OnCreate()
    {
        m_EcbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EcbSystem.CreateCommandBuffer().AsParallelWriter();
        var bufferBarsLength = 11;

        //Spawn 2 Runner everyframe
        Entities.ForEach((Entity e, int entityInQueryIndex, in RunnerSpawnData runnerSpawnData) => 
        {
            for(int i = 0; i<2; i++)
            {
                var newRunner = ecb.Instantiate(entityInQueryIndex,runnerSpawnData.runnerPrefab);
                var bufferBarEntities = ecb.AddBuffer<BufferBarEntities>(entityInQueryIndex,newRunner);

                //Spawn the bar cubes for each runner
                for(int k=0; k < bufferBarsLength; k++)
                {
                    var newBar = ecb.Instantiate(entityInQueryIndex,runnerSpawnData.barPrefab);
                    ecb.AddComponent(entityInQueryIndex,newBar,new BelongsToRunnerData{entity = newRunner});
                    ecb.AddComponent(entityInQueryIndex,newBar,new BelongsToBarData{barID = k});
                    bufferBarEntities.Add(new BufferBarEntities{entity = newBar});
                    
                    //FOR DEBUGGING
                    //EntityManager.SetName(newBar,"Runner Bar");
                }
            }
        }).Schedule();

        //Get spawner settings
        float spacing = 0;
        float spawnAngle = 0;
        float pitRadius = 0;
        float distanceFromPit = 0;

        //Set spawner settings
        float time = (float)Time.ElapsedTime;
        int mode = (int)math.floor(time / 8f);
        Random _random = new Random((uint)(173859*time));
        Entities.ForEach((ref RunnerSpawnerSpacingData runnerSpawnerSpacingData, in RunnerSpawnData spawnData, in PitRadiusData radiusData) => 
        {
            spacing = runnerSpawnerSpacingData.Value;
            spawnAngle = runnerSpawnerSpacingData.spawnAngle;
            pitRadius = radiusData.Value;
            distanceFromPit = spawnData.distanceFromPit;

            //Spawning position mode
            if (mode % 2 == 0) 
            {
                spawnAngle = _random.NextFloat(0f,1f) * 2f * math.PI;
                spacing = _random.NextFloat(0.04f,math.PI*.4f);
            }
            else
            {
                spawnAngle += spacing;
            }

            runnerSpawnerSpacingData.spawnAngle = spawnAngle;
            runnerSpawnerSpacingData.Value = spacing;

        }).Run();

        //Init runner
        Entities.WithAll<NotInitialisedTag>()
        .ForEach((
            Entity e,
            int entityInQueryIndex,
            ref Translation tran, 
            ref RunnerColorData colData, 
            ref RunnerConstantData constData,
            ref RunnerTimeData timeData
        ) => 
        {
            var angle = spawnAngle;
            if (mode % 2 != 0) 
            {
                angle += spacing * entityInQueryIndex;
            }

            //Init runner position
            float spawnRadius = pitRadius + distanceFromPit;
		    float3 pos = new float3(math.cos(angle) * spawnRadius,0f,math.sin(angle) * spawnRadius);
            tran.Value = pos;

            //Init time data
            timeData.timeSinceSpawn = 0f;

            //Init runner color
            float hue = math.sin(time)*.5f+.5f;
            float sat = math.sin(time / 1.37f)*.5f+.5f;
            float value = math.sin(time / 1.618f) * .5f + .5f;
            float4 col = ColorHSV(
                _random,
                hue, hue, 
                sat*.2f+.1f, sat*.4f+.15f,
                value*.15f+.25f, value*.35f+.25f,
                1,1
                );
            colData.color = col;

            //Init constant data
            constData.hipHeight = 1.8f;
            constData.shoulderHeight = 3.5f;
            constData.stepDuration = _random.NextFloat(.25f,.33f);
            constData.xzDamping = _random.NextFloat(0f,1f)*.02f+.002f;
            constData.spreadForce = _random.NextFloat(.0005f,.0015f);
            constData.stanceWidth = .35f;
            constData.legLength = math.sqrt(constData.hipHeight * constData.hipHeight + constData.stanceWidth * constData.stanceWidth)*1.1f;

            //Remove initialize tag
            ecb.RemoveComponent<NotInitialisedTag>(entityInQueryIndex,e);

        }).ScheduleParallel();

        m_EcbSystem.AddJobHandleForProducer(this.Dependency);

        //Just for debug
        //this.Enabled = false;
    }

    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Random/Random.cs
    public static float4 ColorHSV(Random _random, float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax, float alphaMin, float alphaMax)
    {
        var h = math.lerp(hueMin, hueMax, _random.NextFloat(0f,1f));
        var s = math.lerp(saturationMin, saturationMax, _random.NextFloat(0f,1f));
        var v = math.lerp(valueMin, valueMax, _random.NextFloat(0f,1f));
        var color = HSVToRGB(h, s, v, true);
        color.w = math.lerp(alphaMin, alphaMax, _random.NextFloat(0f,1f));
        return color;
    }

    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Color.cs
    public static float4 HSVToRGB(float H, float S, float V, bool hdr)
    {
        float4 retval = 1f;
        if (S == 0)
        {
            retval = V;
        }
        else if (V == 0)
        {
            retval = 0;
        }
        else
        {
            retval = 0;

            //crazy hsv conversion
            float t_S = S;
            float t_V = V;
            float h_to_floor = H * 6.0f;

            int temp = (int)math.floor(h_to_floor);
            float t = h_to_floor - ((float)temp);
            float var_1 = (t_V) * (1 - t_S);
            float var_2 = t_V * (1 - t_S *  t);
            float var_3 = t_V * (1 - t_S * (1 - t));

            switch (temp)
            {
                case 0:
                    retval.x = t_V;
                    retval.y = var_3;
                    retval.z = var_1;
                    break;

                case 1:
                    retval.x = var_2;
                    retval.y = t_V;
                    retval.z = var_1;
                    break;

                case 2:
                    retval.x = var_1;
                    retval.y = t_V;
                    retval.z = var_3;
                    break;

                case 3:
                    retval.x = var_1;
                    retval.y = var_2;
                    retval.z = t_V;
                    break;

                case 4:
                    retval.x = var_3;
                    retval.y = var_1;
                    retval.z = t_V;
                    break;

                case 5:
                    retval.x = t_V;
                    retval.y = var_1;
                    retval.z = var_2;
                    break;

                case 6:
                    retval.x = t_V;
                    retval.y = var_3;
                    retval.z = var_1;
                    break;

                case -1:
                    retval.x = t_V;
                    retval.y = var_1;
                    retval.z = var_2;
                    break;
            }

            if (!hdr) retval = math.clamp(retval.x, 0.0f, 1.0f);
        }
        return retval;
    }
}

