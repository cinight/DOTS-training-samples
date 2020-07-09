using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RunnerModeChangeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float time = (float)Time.ElapsedTime;
        Random _random = new Random((uint)(173859*time));

        float runDirSway = RunnerMoveSystem.runDirSway;

        Entities.WithStructuralChanges().WithNone<IsFallingTag>()
        .ForEach((
            Entity e, 
            ref DynamicBuffer<BufferBarLengths> barLengths,
            ref DynamicBuffer<BufferPrevPoints> prevPoints,
            in DynamicBuffer<BufferPoints> points,
            in Translation tran, 
            in DynamicBuffer<BufferBars> bufferBars, 
            in RunnerConstantData constData
            ) => 
        {
            float distance = math.distance(tran.Value,0);
            if (distance<PitGenerator.pitRadius+1.5f) 
            {
                EntityManager.AddComponent(e,typeof(IsFallingTag));
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
                    var pp = prevPoints[i].prevPoints*.5f + (points[i].points - runDir * RunnerManager.runSpeed * Time.fixedDeltaTime*(.5f+points[i].points.y*.5f / constData.shoulderHeight))*.5f;
                    prevPoints[i] = new BufferPrevPoints{prevPoints = pp};
                    
                    // jump
                    if (i==0 || i > 4) 
                    {
                        pp = prevPoints[i].prevPoints - new float3(0f,_random.NextFloat(.05f,.15f),0f);
                        prevPoints[i] = new BufferPrevPoints{prevPoints = pp};
                    }
                }
			}
        }).Run();
    }
}

//NEED OPTIMIZATION