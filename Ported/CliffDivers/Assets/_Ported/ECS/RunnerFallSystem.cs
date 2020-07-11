using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(RunnerModeChangeSystem))]
public class RunnerFallSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<IsFallingTag>().ForEach
		((
			ref Translation tran,
			ref DynamicBuffer<BufferPoints> points,
			ref DynamicBuffer<BufferPrevPoints> prevPoints,
			in RunnerConstantData constData, 
			in DynamicBuffer<BufferBars> bufferBars,
			in DynamicBuffer<BufferBarLengths> barLengths
			) => 
        {
			float3 average=0f;
			for (int i=0;i<points.Length;i++) 
			{
				average += points[i].points;
			}
			float3 averagePos = average / points.Length;

			for (int i=0;i<points.Length;i++) 
			{
				float3 startPos = points[i].points;
                var prevPoint = prevPoints[i].prevPoints;
                var point = points[i].points;

				prevPoint.y += .005f;
				prevPoint -=(point - averagePos) * constData.spreadForce;

				point.x += (point.x - prevPoint.x)*(1f-constData.xzDamping);
				point.y += point.y - prevPoint.y;
				point.z += (point.z - prevPoint.z)*(1f-constData.xzDamping);
				prevPoint = startPos;

				prevPoints[i] = new BufferPrevPoints{prevPoints = prevPoint};
				points[i] = new BufferPoints{points = point};
			}

			for (int i=0;i<bufferBars.Length/2;i++) 
			{
				float3 point1 = points[bufferBars[i * 2].bars].points;
				float3 point2 = points[bufferBars[i * 2 + 1].bars].points;
				float3 d = point1 - point2;
				float dist = math.sqrt(d.x * d.x + d.y * d.y + d.z * d.z);
				float pushDist = (dist - barLengths[i].barLengths)*.5f/dist;
				point1 -= d * pushDist;
				point2 += d * pushDist;

				points[bufferBars[i * 2].bars] = new BufferPoints{points = point1};
				points[bufferBars[i * 2 + 1].bars] = new BufferPoints{points = point2};
			}

			//Sync runner translation with first point
			tran.Value = points[0].points;

        }).ScheduleParallel();
    }
}
