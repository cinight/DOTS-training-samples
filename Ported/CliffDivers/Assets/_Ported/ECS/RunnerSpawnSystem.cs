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
    private EntityQuery q_Spawner;
    Entity barPrefab;
    NativeArray<Entity> newBars;

   protected override void OnCreate()
   {
       q_Spawner = GetEntityQuery(ComponentType.ReadOnly<RunnerSpawnData>());
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

        //Spawn 2 Runner everyframe
        Entities.WithStructuralChanges().ForEach((Entity e, in RunnerSpawnData runnerSpawnData) => 
        {
            barPrefab = runnerSpawnData.barPrefab;
            var newEntities = EntityManager.Instantiate(runnerSpawnData.runnerPrefab,2,Allocator.Temp);

        }).Run();

        //Adding all the buffers
        Entities.WithStructuralChanges().WithAll<NotInitialisedTag>().ForEach((Entity e) => 
        {
            EntityManager.AddBuffer<BufferBars>(e);
            EntityManager.AddBuffer<BufferBarLengths>(e);
            EntityManager.AddBuffer<BufferBarThickness>(e);
            EntityManager.AddBuffer<BufferFeetAnimating>(e);
            EntityManager.AddBuffer<BufferFootAnimTimers>(e);
            EntityManager.AddBuffer<BufferFootTargets>(e);
            EntityManager.AddBuffer<BufferPoints>(e);
            EntityManager.AddBuffer<BufferPrevPoints>(e);
            EntityManager.AddBuffer<BufferStepStartPos>(e);

            //FOR DEBUGGING
            EntityManager.SetName(e,"Runner");

        }).Run();

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
            ref Translation tran, 
            ref RunnerColorData colData, 
            ref RunnerConstantData constData,
            ref RunnerTimeData timeData,
            ref DynamicBuffer<BufferBars> bufferBars,
            ref DynamicBuffer<BufferBarThickness> bufferBarsThickness
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
            int barThicknessesCount = bufferBars.Length / 2;
			for (int i = 0; i < barThicknessesCount; i++) 
            {
                bufferBarsThickness.Add(new BufferBarThickness{barThicknesses = .2f});
			}
			bufferBarsThickness[barThicknessesCount - 1] = new BufferBarThickness{barThicknesses = .4f};

            //Init constant data
            constData.hipHeight = 1.8f;
            constData.shoulderHeight = 3.5f;
            constData.stepDuration = _random.NextFloat(.25f,.33f);
            constData.xzDamping = _random.NextFloat(0f,1f)*.02f+.002f;
            constData.spreadForce = _random.NextFloat(.0005f,.0015f);
            constData.stanceWidth = .35f;
            constData.matricesPerRunner = bufferBars.Length / 2;
            constData.legLength = math.sqrt(constData.hipHeight * constData.hipHeight + constData.stanceWidth * constData.stanceWidth)*1.1f;

        }).Run();

        //Bar movement arrays
        Entities.WithAll<NotInitialisedTag>()
        .ForEach((
            ref DynamicBuffer<BufferPoints> points,
            ref DynamicBuffer<BufferPrevPoints> prevPoints,
            ref DynamicBuffer<BufferBarLengths> barLengths,
            ref DynamicBuffer<BufferFootTargets> footTargets,
            ref DynamicBuffer<BufferStepStartPos> stepStartPos,
            ref DynamicBuffer<BufferFootAnimTimers> footAnimTimers,
            ref DynamicBuffer<BufferFeetAnimating> feetAnimating,
            in DynamicBuffer<BufferBars> bufferBars
        ) => 
        {
			for (int i = 0; i < 12; i++)                    points.Add(new BufferPoints{points = 0f});
            for (int i = 0; i < points.Length; i++)         prevPoints.Add(new BufferPrevPoints{prevPoints = 0f});
            for (int i = 0; i < bufferBars.Length / 2; i++) barLengths.Add(new BufferBarLengths{barLengths = 0f});
            for (int i = 0; i < 2; i++)                     footTargets.Add(new BufferFootTargets{footTargets = 0f});
            for (int i = 0; i < 2; i++)                     stepStartPos.Add(new BufferStepStartPos{stepStartPositions = 0f});
            for (int i = 0; i < 2; i++)                     footAnimTimers.Add(new BufferFootAnimTimers{footAnimTimers = _random.NextFloat(0f,1f)});
            for (int i = 0; i < 2; i++)                     feetAnimating.Add(new BufferFeetAnimating{feetAnimating = true});
        }).Run();

        //Spawn the bar cubes
        var bufferBarsLength = 11;
        Entities.WithStructuralChanges().WithAll<NotInitialisedTag>().ForEach((Entity e) => 
        {
            newBars = EntityManager.Instantiate(barPrefab,bufferBarsLength,Allocator.Temp);
            for(int k=0; k < newBars.Length; k++)
            {
                EntityManager.AddComponentData(newBars[k],new BelongsToRunnerData{entity = e});
                EntityManager.AddComponentData(newBars[k],new BelongsToBarData{barID = k});

                //FOR DEBUGGING
                EntityManager.SetName(newBars[k],"Runner Bar");
            }
            
        }).Run();

        //Set spawner settings
        Entities.ForEach((ref RunnerSpawnerSpacingData runnerSpawnerSpacingData) => 
        {
            runnerSpawnerSpacingData.spawnAngle = spawnAngle;
            runnerSpawnerSpacingData.Value = spacing;

        }).Run();

        //Remove initialize tag
        Entities.WithStructuralChanges().WithAll<NotInitialisedTag>().ForEach((Entity e) => 
        {
            EntityManager.RemoveComponent(e,typeof(NotInitialisedTag));
            
        }).Run();
        

        //Just for debug
        //this.Enabled = false;
    }
}