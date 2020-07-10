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
			ref DynamicBuffer<BufferPoints> points,
			ref DynamicBuffer<BufferPrevPoints> prevPoints,
			in RunnerConstantData constData, 
			in DynamicBuffer<BufferBars> bufferBars,
			in DynamicBuffer<BufferBarLengths> barLengths
			) => 
        {
			float averageX=0f;
			float averageY=0f;
			float averageZ=0f;
			for (int i=0;i<points.Length;i++) 
			{
				averageX += points[i].points.x;
				averageY += points[i].points.y;
				averageZ += points[i].points.z;
			}
			float3 averagePos = new float3(averageX / points.Length,averageY / points.Length,averageZ / points.Length);

			for (int i=0;i<points.Length;i++) 
			{
				float3 startPos = points[i].points;
                var prevPoint = prevPoints[i].prevPoints;
                var point = points[i].points;

				prevPoint.y += .005f;

				prevPoint.x-=(points[i].points.x - averagePos.x) * constData.spreadForce;
				prevPoint.y-=(points[i].points.y - averagePos.y) * constData.spreadForce;
				prevPoint.z-=(points[i].points.z - averagePos.z) * constData.spreadForce;

				point.x += (points[i].points.x - prevPoints[i].prevPoints.x)*(1f-constData.xzDamping);
				point.y += points[i].points.y - prevPoints[i].prevPoints.y;
				point.z += (points[i].points.z - prevPoints[i].prevPoints.z)*(1f-constData.xzDamping);
				prevPoint = startPos;

				prevPoints[i] = new BufferPrevPoints{prevPoints = prevPoint};
				points[i] = new BufferPoints{points = point};
			}

			for (int i=0;i<bufferBars.Length/2;i++) 
			{
				float3 point1 = points[bufferBars[i * 2].bars].points;
				float3 point2 = points[bufferBars[i * 2 + 1].bars].points;
				float dx = point1.x - point2.x;
				float dy = point1.y - point2.y;
				float dz = point1.z - point2.z;
				float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
				float pushDist = (dist - barLengths[i].barLengths)*.5f/dist;
				point1.x -= dx * pushDist;
				point1.y -= dy * pushDist;
				point1.z -= dz * pushDist;
				point2.x += dx * pushDist;
				point2.y += dy * pushDist;
				point2.z += dz * pushDist;

				points[bufferBars[i * 2].bars] = new BufferPoints{points = point1};
				points[bufferBars[i * 2 + 1].bars] = new BufferPoints{points = point2};
			}
        }).Schedule();
    }
}
