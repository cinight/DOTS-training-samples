using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(RunnerSpawnSystem))]
public class RunnerMoveSystem : SystemBase
{
    public static float runDirSway;

	static DynamicBuffer<BufferPoints> UpdateLimb( DynamicBuffer<BufferPoints> Inpoints, int index1, int index2, int jointIndex, float length, float3 perp)
	{
        var points = Inpoints;
        
		float3 point1 = points[index1].points;
		float3 point2 = points[index2].points;
		float dx = point2.x - point1.x;
		float dy = point2.y - point1.y;
		float dz = point2.z - point1.z;
		float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
		float lengthError = dist - length;
		if (lengthError > 0f) 
		{
			// requested limb is too long: clamp it

			length /= dist;

            points[index2] = new BufferPoints{points = 
                new float3(point1.x + dx * length,
										 point1.y + dy * length,
										 point1.z + dz * length)
            };
            points[jointIndex] = new BufferPoints{points = 
                new float3(point1.x + dx * length*.5f,
											 point1.y + dy * length*.5f,
											 point1.z + dz * length*.5f)
            };
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

            points[jointIndex] = new BufferPoints{points = 
                new float3((point1.x + point2.x) * .5f+bend.x,
				(point1.y + point2.y) * .5f+bend.y,
				(point1.z + point2.z) * .5f+bend.z)
            };
		}
        return points;
	}

    protected override void OnUpdate()
    {
        //update run direction
        runDirSway = math.sin((float)Time.ElapsedTime * .5f) * .5f;

        //Save PrevPoints
        Entities.WithNone<IsFallingTag>().ForEach
        ((
            ref DynamicBuffer<BufferPrevPoints> prevPoints,
            in DynamicBuffer<BufferPoints> points
        ) => 
        {
			for (int i = 0; i < points.Length; i++) 
			{
				prevPoints[i] = new BufferPrevPoints{prevPoints = points[i].points};
			}

        }).Schedule();

        var runDirSway_temp = runDirSway;

        //Update body part
        float deltatime = Time.DeltaTime;
        float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
        Entities.WithNone<IsFallingTag>().ForEach
        ((
            ref DynamicBuffer<BufferPoints> points,
            ref DynamicBuffer<BufferFootTargets> footTargets,
            ref DynamicBuffer<BufferFootAnimTimers> footAnimTimers,
            ref DynamicBuffer<BufferFeetAnimating> feetAnimating,
            ref DynamicBuffer<BufferStepStartPos> stepStartPos,
            ref RunnerTimeData timeData,
            ref Translation tran,
            in RunnerConstantData constData
        ) => 
        {
            //Time
            timeData.timeSinceSpawn += deltatime;
            timeData.timeSinceSpawn = math.saturate(timeData.timeSinceSpawn);

            float runSpeed = 10f;

			float3 runDir = -tran.Value;
			runDir += math.cross(runDir,math.up())*runDirSway_temp;
			runDir = math.normalize(runDir);
			tran.Value += runDir * runSpeed * fixedDeltaTime;
			float3 perp = new float3(-runDir.z,0f,runDir.x);

			// hip
            points[0] = new BufferPoints{points = new float3(tran.Value.x, tran.Value.y + constData.hipHeight, tran.Value.z)};

			// feet
			float3 stanceOffset = new float3(perp.x * constData.stanceWidth,perp.y * constData.stanceWidth,perp.z * constData.stanceWidth);
			footTargets[0] = new BufferFootTargets{footTargets = tran.Value - stanceOffset + runDir * (runSpeed * .1f)};
            footTargets[1] = new BufferFootTargets{footTargets = tran.Value + stanceOffset + runDir * (runSpeed * .1f)};
			for (int i = 0; i < 2; i++) 
            {
				int pointIndex = 2 + i * 2;
				float3 delta = footTargets[i].footTargets - points[pointIndex].points;
                float sqrMagnitude = math.distancesq(delta,0);
				if (sqrMagnitude > .25f) {
					if (feetAnimating[i].feetAnimating == false && (feetAnimating[1 - i].feetAnimating == false || footAnimTimers[1 - i].footAnimTimers > .9f)) 
					{
                        feetAnimating[i] = new BufferFeetAnimating{feetAnimating = true};
                        footAnimTimers[i] = new BufferFootAnimTimers{footAnimTimers = 0f};
                        stepStartPos[i] = new BufferStepStartPos{stepStartPositions = points[pointIndex].points};
					}
				}

				if (feetAnimating[i].feetAnimating) 
				{
                    footAnimTimers[i] = new BufferFootAnimTimers{footAnimTimers = math.saturate(footAnimTimers[i].footAnimTimers + fixedDeltaTime / constData.stepDuration)};
					float timer = footAnimTimers[i].footAnimTimers;
                    points[pointIndex] = new BufferPoints{points = math.lerp(stepStartPos[i].stepStartPositions,footTargets[i].footTargets,timer)};
					float step = 1f - 4f * (timer - .5f) * (timer - .5f);
                    points[pointIndex] = new BufferPoints{points = points[pointIndex].points + new float3(0,step,0)};
					if (footAnimTimers[i].footAnimTimers >= 1f) 
                    {
						feetAnimating[i] = new BufferFeetAnimating{feetAnimating = false};
					}
				}
			}

			// knees
			points = UpdateLimb(points,0,2,1,constData.legLength,perp);
			points = UpdateLimb(points,0,4,3,constData.legLength,perp);

			// shoulders
            points[6] = new BufferPoints{points = 
            new float3(tran.Value.x + runDir.x * runSpeed * .075f,
									tran.Value.y + constData.shoulderHeight,
									tran.Value.z + runDir.z * runSpeed * .075f)
            };

			// spine
			points = UpdateLimb(points,0,6,5,constData.shoulderHeight - constData.hipHeight,perp);

			// hands
			for (int i = 0; i < 2; i++) 
			{
				float3 oppositeFootOffset = points[4 - 2 * i].points - points[0].points;
				oppositeFootOffset.y = oppositeFootOffset.y*(-.5f)-1.7f;
                points[8 + i * 2] = new BufferPoints{points = 
                points[0].points - oppositeFootOffset*.65f - perp*(.8f*(-1f+i*2f)) + runDir*(runSpeed*.05f)
                };


				// elbows
				points = UpdateLimb(points,6,8 + i * 2,7 + i * 2,constData.legLength*.9f,new float3(0f,-1f+i*2f,0f));
			}

			// head
            points[11] = new BufferPoints{points = 
            points[6].points + math.normalize(tran.Value) * -.1f+new float3(0f,.4f,0f)
            };

        }).ScheduleParallel();
    }
}