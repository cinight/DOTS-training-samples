using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(BodyPartRenderSystem))]
public class RunnerDestroySystem : SystemBase
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

        //Destroy runner entity
        Entities.WithAll<IsFallingTag>().
        ForEach((Entity e, in Translation tran) => 
        {
            if (tran.Value.y<-150f) 
            {
                ecb.DestroyEntity(e);
                //EntityManager.DestroyEntity(e);
            }
        }).Schedule();

        //Destroy bar entity
        Entities.
        WithAll<BelongsToRunnerData>().
        ForEach((Entity e, in Translation tran) => 
        {
            if (tran.Value.y<-150f) 
            {
                ecb.DestroyEntity(e);
                //EntityManager.DestroyEntity(e);
            }
        }).Schedule();

        // Make sure that the ECB system knows about our job
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

//NEED OPTIMIZATION