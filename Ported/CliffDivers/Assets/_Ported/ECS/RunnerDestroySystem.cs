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
        ForEach((Entity e, in Translation tran) => 
        {
            if (tran.Value.y<-150f) 
            {
                EntityManager.DestroyEntity(e); //Destroy runner entity
            }
        }).Run();

        Entities.WithStructuralChanges().
        WithAll<BelongsToRunnerData>().
        ForEach((Entity e, in Translation tran) => 
        {
            if (tran.Value.y<-150f) 
            {
                EntityManager.DestroyEntity(e); //Destroy bar entity
            }
        }).Run();
    }
}

//NEED OPTIMIZATION