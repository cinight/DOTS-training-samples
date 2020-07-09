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
        .ForEach((Entity e, ref RunnerBarMoveData moveData, in Translation tran, in RunnerBarData barData, in RunnerConstantData constData ) => 
        {
            float distance = math.distance(tran.Value,0);
            if (distance<PitGenerator.pitRadius+1.5f) 
            {
                EntityManager.AddComponent(e,typeof(IsFallingTag));
                for (int i=0;i<moveData.barLengths.Length;i++) 
                {
                    var pos = (moveData.points[barData.bars[i * 2]] - moveData.points[barData.bars[i * 2 + 1]]);
                    moveData.barLengths[i] = math.distance(pos,0);
                }

                // final frame of animated mode - prepare point velocities:
                float3 runDir = -tran.Value;
                runDir += math.cross(runDir,math.up())*runDirSway;
                runDir = math.normalize(runDir);
                for (int i=0;i<moveData.points.Length;i++) 
                {
                    moveData.prevPoints[i] = moveData.prevPoints[i]*.5f + (moveData.points[i] - runDir * RunnerManager.runSpeed * Time.fixedDeltaTime*(.5f+moveData.points[i].y*.5f / constData.shoulderHeight))*.5f;
                    
                    // jump
                    if (i==0 || i > 4) {
                        moveData.prevPoints[i] -= new float3(0f,_random.NextFloat(.05f,.15f),0f);
                    }
                }
			}
        }).Run();
    }
}

//NEED OPTIMIZATION