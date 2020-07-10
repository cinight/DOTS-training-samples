using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(RunnerMoveSystem))]
public class RunnerModeChangeSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        // Find the ECB system once and store it for later usage
        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

        //time
        float time = (float)Time.ElapsedTime;
        float fixedTime = Time.fixedDeltaTime;
        Random _random = new Random((uint)(173859*time));

        float runDirSway = RunnerMoveSystem.runDirSway;
        float pitRadius = PitGenerator.pitRadius;
        float runSpeed =  10f;

        Entities.WithNone<IsFallingTag>()
        .ForEach((
            Entity e, int entityInQueryIndex,
            ref DynamicBuffer<BufferBarLengths> barLengths,
            ref DynamicBuffer<BufferPrevPoints> prevPoints,
            in DynamicBuffer<BufferPoints> points,
            in Translation tran, 
            in DynamicBuffer<BufferBars> bufferBars, 
            in RunnerConstantData constData
            ) => 
        {
            float distance = math.distance(tran.Value,0);
            if (distance<pitRadius+1.5f) 
            {
                ecb.AddComponent(e,new IsFallingTag{});

                for (int i=0;i<barLengths.Length;i++) 
                {
                    var pos = (points[bufferBars[i * 2].bars].points - points[bufferBars[i * 2 + 1].bars].points);
                    barLengths[i] = new BufferBarLengths{barLengths = math.distance(pos,0)};
                }

                // final frame of animated mode - prepare point velocities:
                float3 runDir = -tran.Value;
                runDir += math.cross(runDir,math.up())*runDirSway;
                runDir = math.normalize(runDir);
                for (int i=0;i<points.Length;i++) 
                {
                    var pp = prevPoints[i].prevPoints*.5f + (points[i].points - runDir * runSpeed * fixedTime*(.5f+points[i].points.y*.5f / constData.shoulderHeight))*.5f;
                    prevPoints[i] = new BufferPrevPoints{prevPoints = pp};
                    
                    // jump
                    if (i==0 || i > 4) 
                    {
                        pp = prevPoints[i].prevPoints - new float3(0f,_random.NextFloat(.05f,.15f),0f);
                        prevPoints[i] = new BufferPrevPoints{prevPoints = pp};
                    }
                }
			}
        }).Schedule();

        // Make sure that the ECB system knows about our job
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

//NEED OPTIMIZATION