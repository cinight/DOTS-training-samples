using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RunnerMoveSystem : SystemBase
{
    public static float runDirSway;

    // EntityQuery m_Group;

    // protected override void OnCreate()
    // {
    //     m_Group = GetEntityQuery(typeof(RunnerBarMoveData));
    // }

    // [BurstCompile]
    // struct UpdateLimbJob : IJobChunk
    // {
    //     public ComponentTypeHandle<RunnerBarMoveData> moveDataType;
    //     public int index1;
    //     public int index2;
    //     public int jointIndex;
    //     public float length;
    //     public float3 perp;

    //     public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    //     {
    //         var chunkMoveDatas = chunk.GetNativeArray(moveDataType);

    //         for (var i = 0; i < chunk.Count; i++)
    //         {
    //             var pData = chunkMoveDatas[i];

    //             float3 point1 = pData.points[index1];
    //             float3 point2 = pData.points[index2];
    //             float dx = point2.x - point1.x;
    //             float dy = point2.y - point1.y;
    //             float dz = point2.z - point1.z;
    //             float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
    //             float lengthError = dist - length;
    //             if (lengthError > 0f) 
    //             {
    //                 // requested limb is too long: clamp it

    //                 length /= dist;
    //                 pData.points[index2] = new float3(point1.x + dx * length,
    //                                             point1.y + dy * length,
    //                                             point1.z + dz * length);
    //                 pData.points[jointIndex] = new float3(point1.x + dx * length*.5f,
    //                                                 point1.y + dy * length*.5f,
    //                                                 point1.z + dz * length*.5f);
    //             }
    //             else
    //             {
    //                 // requested limb is too short: bend it

    //                 lengthError *= .5f;
    //                 dx *= lengthError;
    //                 dy *= lengthError;
    //                 dz *= lengthError;

    //                 // cross product of (dx,dy,dz) and (perp)
    //                 float3 bend = new float3(dy * perp.z - dz * perp.y,dz * perp.x - dx * perp.z,dx * perp.y - dy * perp.x);

    //                 pData.points[jointIndex] = new float3((point1.x + point2.x) * .5f+bend.x,
    //                                                 (point1.y + point2.y) * .5f+bend.y,
    //                                                 (point1.z + point2.z) * .5f+bend.z);
    //             }

    //             //Assign values back to component data
    //             chunkMoveDatas[i] = pData;
    //         }
    //     }
    // }


	void UpdateLimb( ref NativeArray<float3> points, int index1, int index2, int jointIndex, float length, float3 perp)
	{
		float3 point1 = points[index1];
		float3 point2 = points[index2];
		float dx = point2.x - point1.x;
		float dy = point2.y - point1.y;
		float dz = point2.z - point1.z;
		float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
		float lengthError = dist - length;
		if (lengthError > 0f) 
		{
			// requested limb is too long: clamp it

			length /= dist;
			points[index2] = new float3(point1.x + dx * length,
										 point1.y + dy * length,
										 point1.z + dz * length);
			points[jointIndex] = new float3(point1.x + dx * length*.5f,
											 point1.y + dy * length*.5f,
											 point1.z + dz * length*.5f);
		}
		else
		{
			// requested limb is too short: bend it

			lengthError *= .5f;
			dx *= lengthError;
			dy *= lengthError;
			dz *= lengthError;

			// cross product of (dx,dy,dz) and (perp)
			float3 bend = new float3(dy * perp.z - dz * perp.y,dz * perp.x - dx * perp.z,dx * perp.y - dy * perp.x);

			points[jointIndex] = new float3((point1.x + point2.x) * .5f+bend.x,
											 (point1.y + point2.y) * .5f+bend.y,
											 (point1.z + point2.z) * .5f+bend.z);
		}
        //return points;
	}

    protected override void OnUpdate()
    {
        //update run direction
        runDirSway = math.sin((float)Time.ElapsedTime * .5f) * .5f;

        //For jobs
        //var moveDataType = GetComponentTypeHandle<RunnerBarMoveData>();

        //Update body part
        float deltatime = Time.DeltaTime;
        float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
        Entities.WithoutBurst().ForEach((ref RunnerBarMoveData moveData, ref RunnerTimeData timeData, ref Translation tran, in RunnerConstantData constData) => 
        {
            //Time
            timeData.timeSinceSpawn += deltatime;
            timeData.timeSinceSpawn = math.saturate(timeData.timeSinceSpawn);

            //Prev points
			for (int i = 0; i < moveData.points.Length; i++) 
			{
				moveData.prevPoints[i] = moveData.points[i];
			}

            float runSpeed = 10f;

			float3 runDir = -tran.Value;
			runDir += math.cross(runDir,math.up())*runDirSway;
			runDir = math.normalize(runDir);
			tran.Value += runDir * runSpeed * fixedDeltaTime;
			float3 perp = new float3(-runDir.z,0f,runDir.x);

			// hip
			moveData.points[0] = new float3(tran.Value.x, tran.Value.y + constData.hipHeight, tran.Value.z);

			// feet
			float3 stanceOffset = new float3(perp.x * constData.stanceWidth,perp.y * constData.stanceWidth,perp.z * constData.stanceWidth);
			moveData.footTargets[0] = tran.Value - stanceOffset + runDir * (runSpeed * .1f);
			moveData.footTargets[1] = tran.Value + stanceOffset + runDir * (runSpeed * .1f);
			for (int i = 0; i < 2; i++) {
				int pointIndex = 2 + i * 2;
				float3 delta = moveData.footTargets[i] - moveData.points[pointIndex];
                float sqrMagnitude = math.distancesq(delta,0);
				if (sqrMagnitude > .25f) {
					if (moveData.feetAnimating[i] == false && (moveData.feetAnimating[1 - i] == false || moveData.footAnimTimers[1 - i] > .9f)) 
					{
						moveData.feetAnimating[i] = true;
						moveData.footAnimTimers[i] = 0f;
						moveData.stepStartPositions[i] = moveData.points[pointIndex];
					}
				}

				if (moveData.feetAnimating[i]) 
				{
					moveData.footAnimTimers[i] = math.saturate(moveData.footAnimTimers[i] + fixedDeltaTime / constData.stepDuration);
					float timer = moveData.footAnimTimers[i];
					moveData.points[pointIndex] = math.lerp(moveData.stepStartPositions[i],moveData.footTargets[i],timer);
					float step = 1f - 4f * (timer - .5f) * (timer - .5f);
					moveData.points[pointIndex] += new float3(0,step,0);
					if (moveData.footAnimTimers[i] >= 1f) 
                    {
						moveData.feetAnimating[i] = false;
					}
				}
			}

			// knees
			UpdateLimb(ref moveData.points,0,2,1,constData.legLength,perp);
			UpdateLimb(ref moveData.points,0,4,3,constData.legLength,perp);

            //Job for knees
            // var job = new UpdateLimbJob()
            // {
            //     moveDataType = moveDataType,
            //     index1 = 0,
            //     index2 = 2,
            //     jointIndex = 1,
            //     length = constData.legLength,
            //     perp = perp
            // };
            // Dependency = job.Schedule(m_Group, Dependency);
            // job = new UpdateLimbJob()
            // {
            //     moveDataType = moveDataType,
            //     index1 = 0,
            //     index2 = 4,
            //     jointIndex = 3,
            //     length = constData.legLength,
            //     perp = perp
            // };
            // Dependency = job.Schedule(m_Group, Dependency);

			// shoulders
			moveData.points[6] = new float3(tran.Value.x + runDir.x * runSpeed * .075f,
									tran.Value.y + constData.shoulderHeight,
									tran.Value.z + runDir.z * runSpeed * .075f);

			// spine
			UpdateLimb(ref moveData.points,0,6,5,constData.shoulderHeight - constData.hipHeight,perp);

            //Job for spine
            // job = new UpdateLimbJob()
            // {
            //     moveDataType = moveDataType,
            //     index1 = 0,
            //     index2 = 6,
            //     jointIndex = 5,
            //     length = constData.shoulderHeight - constData.hipHeight,
            //     perp = perp
            // };
            // Dependency = job.Schedule(m_Group, Dependency);

			// hands
			for (int i = 0; i < 2; i++) 
			{
				float3 oppositeFootOffset = moveData.points[4 - 2 * i] - moveData.points[0];
				oppositeFootOffset.y = oppositeFootOffset.y*(-.5f)-1.7f;
				moveData.points[8 + i * 2] = moveData.points[0] - oppositeFootOffset*.65f - perp*(.8f*(-1f+i*2f)) + runDir*(runSpeed*.05f);

				// elbows
				UpdateLimb(ref moveData.points,6,8 + i * 2,7 + i * 2,constData.legLength*.9f,new float3(0f,-1f+i*2f,0f));

                //Job for elbows
                // job = new UpdateLimbJob()
                // {
                //     moveDataType = moveDataType,
                //     index1 = 6,
                //     index2 = 8 + i * 2,
                //     jointIndex = 7 + i * 2,
                //     length = constData.legLength*.9f,
                //     perp = new float3(0f,-1f+i*2f,0f)
                // };
                // Dependency = job.Schedule(m_Group, Dependency);
			}

			// head
			moveData.points[11] = moveData.points[6] + math.normalize(tran.Value) * -.1f+new float3(0f,.4f,0f);

        }).Run();
    }
}

//NEED OPTIMIZATION