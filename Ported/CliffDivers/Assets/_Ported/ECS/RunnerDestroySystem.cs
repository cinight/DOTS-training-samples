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
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        //Destroy runner entity
        Entities.WithAll<IsFallingTag>().
        ForEach((Entity e, int entityInQueryIndex, in Translation tran, in DynamicBuffer<BufferBarEntities> barEntities) => 
        {
            if (tran.Value.y<-150f) 
            {
                //Destroy bar entities
                for(int i = 0; i< barEntities.Length; i++)
                {
                    ecb.DestroyEntity(entityInQueryIndex,barEntities[i].entity);
                }

                ecb.DestroyEntity(entityInQueryIndex,e);
                //EntityManager.DestroyEntity(e);
            }
        }).ScheduleParallel();

        // Make sure that the ECB system knows about our job
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

//NEED OPTIMIZATION