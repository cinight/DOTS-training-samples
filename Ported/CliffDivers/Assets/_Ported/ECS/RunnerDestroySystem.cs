using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RunnerDestroySystem : SystemBase
{
    protected override void OnUpdate()
    {      
        Entities.WithStructuralChanges().WithAll<IsFallingTag>().
        ForEach((Entity e, in RunnerBarMoveData moveData) => 
        {
            for (int i=0;i<moveData.points.Length;i++) 
            {
                if (moveData.points[i].y<-150f) 
                {
                    EntityManager.DestroyEntity(e);
                }
            }
        }).Run();
    }
}

//NEED OPTIMIZATION