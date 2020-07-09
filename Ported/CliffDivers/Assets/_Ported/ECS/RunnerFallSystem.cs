using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RunnerFallSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<IsFallingTag>().
        ForEach((ref RunnerBarMoveData moveData, in RunnerConstantData constData, in RunnerBarData barData) => 
        {
			float averageX=0f;
			float averageY=0f;
			float averageZ=0f;
			for (int i=0;i<moveData.points.Length;i++) 
			{
				averageX += moveData.points[i].x;
				averageY += moveData.points[i].y;
				averageZ += moveData.points[i].z;
			}
			float3 averagePos = new float3(averageX / moveData.points.Length,averageY / moveData.points.Length,averageZ / moveData.points.Length);

			for (int i=0;i<moveData.points.Length;i++) 
			{
				float3 startPos = moveData.points[i];
                var prevPoint = moveData.prevPoints[i];
                var point = moveData.points[i];

				prevPoint.y += .005f;

				prevPoint.x-=(moveData.points[i].x - averagePos.x) * constData.spreadForce;
				prevPoint.y-=(moveData.points[i].y - averagePos.y) * constData.spreadForce;
				prevPoint.z-=(moveData.points[i].z - averagePos.z) * constData.spreadForce;

				point.x += (moveData.points[i].x - moveData.prevPoints[i].x)*(1f-constData.xzDamping);
				point.y += moveData.points[i].y - moveData.prevPoints[i].y;
				point.z += (moveData.points[i].z - moveData.prevPoints[i].z)*(1f-constData.xzDamping);
				prevPoint = startPos;

                moveData.prevPoints[i] = prevPoint;
                moveData.points[i] = point;
			}

			for (int i=0;i<barData.bars.Length/2;i++) 
			{
				float3 point1 = moveData.points[barData.bars[i * 2]];
				float3 point2 = moveData.points[barData.bars[i * 2 + 1]];
				float dx = point1.x - point2.x;
				float dy = point1.y - point2.y;
				float dz = point1.z - point2.z;
				float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
				float pushDist = (dist - moveData.barLengths[i])*.5f/dist;
				point1.x -= dx * pushDist;
				point1.y -= dy * pushDist;
				point1.z -= dz * pushDist;
				point2.x += dx * pushDist;
				point2.y += dy * pushDist;
				point2.z += dz * pushDist;

				moveData.points[barData.bars[i * 2]] = point1;
				moveData.points[barData.bars[i * 2 + 1]] = point2;
			}
        }).Schedule();
    }
}
