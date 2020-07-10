using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(RunnerFallSystem))]
public class RunnerDestroySystem : SystemBase
{
    protected override void OnUpdate()
    {      
        Entities.WithStructuralChanges().WithAll<IsFallingTag>().
        ForEach((Entity e, in DynamicBuffer<BufferPoints> points) => 
        {
            for (int i=0;i<points.Length;i++) 
            {
                if (points[i].points.y<-150f) 
                {
                    EntityManager.DestroyEntity(e);
                }
            }
        }).Run();
    }
}

//NEED OPTIMIZATION