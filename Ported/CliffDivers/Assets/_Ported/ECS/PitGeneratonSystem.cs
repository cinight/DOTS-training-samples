using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public class PitGeneratonSystem : SystemBase
{
	public Vector3 DistortPoint(Vector3 point, float pitRadius) 
    {
		float dist = Mathf.Sqrt(point.x * point.x + point.z * point.z)-pitRadius;
		float distort = Mathf.Clamp01(1f-dist)*.9f+.1f;
		Vector3 offset = new Vector3(Mathf.PerlinNoise(point.x,point.y + point.z),
									 Mathf.PerlinNoise(point.x + 20f,point.y + point.z + 50f),
									 Mathf.PerlinNoise(point.x+50f,point.y+point.z+90f));
		return point + (offset * 2f-Vector3.one)*distort*2f;
	}

    Mesh CreatePitMesh(int pitRingCount, int quadsPerRing, float pitRadius, float ringHeight)
    {
        Mesh pitMesh;
		pitMesh = new Mesh();
		pitMesh.name = "Pit Mesh";

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();

		for (int i=-pitRingCount;i<pitRingCount;i++) 
		{
			for (int j = 0; j < quadsPerRing; j++) 
			{
				float angle1 = j / (float)quadsPerRing * Mathf.PI*2f;
				float angle2 = (j + 1f) / quadsPerRing * Mathf.PI*2f;

				Vector3 pos1, pos2, pos3, pos4;

				if (i < 0) 
				{
					pos1 = new Vector3(Mathf.Cos(angle1) * (pitRadius - i * ringHeight),
											   0f,
											   Mathf.Sin(angle1) * (pitRadius - i * ringHeight));
					pos2 = new Vector3(Mathf.Cos(angle2) * (pitRadius - i * ringHeight),
											   0f,
											   Mathf.Sin(angle2) * (pitRadius - i * ringHeight));
					pos3 = new Vector3(Mathf.Cos(angle1) * (pitRadius-(i+1f)*ringHeight),
											   0f,
											   Mathf.Sin(angle1) * (pitRadius-(i+1f)*ringHeight));
					pos4 = new Vector3(Mathf.Cos(angle2) * (pitRadius-(i+1f)*ringHeight),
											   0f,
											   Mathf.Sin(angle2) * (pitRadius-(i+1f)*ringHeight));
				} 
				else 
				{
					pos1 = new Vector3(Mathf.Cos(angle1) * pitRadius,
											   -i * ringHeight,
											   Mathf.Sin(angle1) * pitRadius);
					pos2 = new Vector3(Mathf.Cos(angle2) * pitRadius,
											   -i * ringHeight,
											   Mathf.Sin(angle2) * pitRadius);
					pos3 = pos1 - Vector3.up * ringHeight;
					pos4 = pos2 - Vector3.up * ringHeight;
				}

				pos1 = DistortPoint(pos1,pitRadius);
				pos2 = DistortPoint(pos2,pitRadius);
				pos3 = DistortPoint(pos3,pitRadius);
				pos4 = DistortPoint(pos4,pitRadius);

				int vertIndex = vertices.Count;
				vertices.Add(pos1);
				vertices.Add(pos2);
				vertices.Add(pos3);
				vertices.Add(pos4);

				triangles.Add(vertIndex + 2);
				triangles.Add(vertIndex + 1);
				triangles.Add(vertIndex + 0);

				triangles.Add(vertIndex + 2);
				triangles.Add(vertIndex + 3);
				triangles.Add(vertIndex + 1);
			}
		}

		pitMesh.SetVertices(vertices);
		pitMesh.SetTriangles(triangles,0);
		pitMesh.RecalculateBounds();
		pitMesh.RecalculateNormals();

        return pitMesh;
    }

    protected override void OnUpdate()
    {
        Entities.WithStructuralChanges().ForEach((Entity e, in RenderMesh ren, in PitGenData genData, in PitDepthData depthData, in PitRadiusData radiusData) => 
        {
            Mesh pitMesh = CreatePitMesh(genData.pitRingCount,genData.quadsPerRing,radiusData.Value,depthData.Value);
            EntityManager.SetSharedComponentData(e,new RenderMesh{ mesh = pitMesh, material = ren.material, castShadows = ShadowCastingMode.TwoSided, receiveShadows = true});
            EntityManager.RemoveComponent(e,typeof(PitGenData));
        }).Run();
    }
}
