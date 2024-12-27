namespace VoxelEngine
{
	using System;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using UnityEngine.Rendering;
	using Utilities;
	using VoxelEngine.Core;
	using VoxelEngine.Jobs;

	public class VoxelTester : MonoBehaviour
	{
		[SerializeField]
		private MeshFilter meshFilter;

		private void Start()
		{
			//Create the chunk
			Chunk chunk = new(0, 0, Allocator.Persistent);
			
			//Initialize the chunk.
			InitializeBlocksJob initializeBlocksJob = new()
			{
				//Output
				blocks = chunk.blocks,
			};
			JobHandle initializeBlocksJobHandle = initializeBlocksJob.Schedule();
			initializeBlocksJobHandle.Complete();

			//Fill the chunk.
			FillBlocksJob fillBlocksJob = new()
			{
				//Output
				blocks = chunk.blocks,
				//Input
				blockType = BlockType.Dirt,
			};
			JobHandle fillBlocksJobHandle = fillBlocksJob.Schedule();
			fillBlocksJobHandle.Complete();

			unsafe
			{
				//Build the chunk
				BuildBlocksJob buildBlocksJob = new()
				{
					//Output
					chunk = &chunk,
					//meshVertexData = &chunk.meshVertexData,
					//Input
					//blocks = chunk.blocks,
					blockMapping = BlockMapping.blockMapping,
				};
				JobHandle buildBlocksJobHandle = buildBlocksJob.Schedule();
				buildBlocksJobHandle.Complete();
			}
			
			Mesh createdMesh = CreateMesh(ref chunk.meshVertexData);
			meshFilter.mesh = createdMesh;
			
			chunk.Dispose();
		}

		private void Test1()
		{
			VertexData vertexData = new(Allocator.Temp, 4 * 5, 6 * 5);

			Block stoneBlock = new()
			{
				position = new int3(0, 0, 0),
				blockType = BlockType.Stone,
			};
			Block dirtBlock = new()
			{
				position = new int3(1, 0, 0),
				blockType = BlockType.Dirt,
			};
			Block grassBlock = new()
			{
				position = new int3(2, 0, 0),
				blockType = BlockType.Grass,
			};
			Block gravelBlock = new()
			{
				position = new int3(3, 0, 0),
				blockType = BlockType.Gravel,
			};
			Block woodBlock = new()
			{
				position = new int3(4, 0, 0),
				blockType = BlockType.Wood,
			};

			int vertexIndex = 0;
			
			CubeBuilderUtils.BuildCube_24Verts(ref vertexData, ref vertexIndex, stoneBlock, ref BlockMapping.blockMapping);
			CubeBuilderUtils.BuildCube_24Verts(ref vertexData, ref vertexIndex, dirtBlock, ref BlockMapping.blockMapping);
			CubeBuilderUtils.BuildCube_24Verts(ref vertexData, ref vertexIndex, grassBlock, ref BlockMapping.blockMapping);
			CubeBuilderUtils.BuildCube_24Verts(ref vertexData, ref vertexIndex, gravelBlock, ref BlockMapping.blockMapping);
			CubeBuilderUtils.BuildCube_24Verts(ref vertexData, ref vertexIndex, woodBlock, ref BlockMapping.blockMapping);

			Mesh createdMesh = CreateMesh(ref vertexData);
			meshFilter.mesh = createdMesh;
			
			vertexData.Dispose();
		}
		
		private Mesh CreateMesh(ref VertexData vertexData)
		{
			Mesh mesh = new()
			{
				indexFormat = IndexFormat.UInt32,
				vertices = UnsafeUtils.ConvertToArray(vertexData.vertices),
				triangles = UnsafeUtils.ConvertToArray(vertexData.indices),
				normals = UnsafeUtils.ConvertToArray(vertexData.normals),
				tangents = UnsafeUtils.ConvertToArray(vertexData.tangents),
			};

			mesh.SetUVs(0, UnsafeUtils.ConvertToArray(vertexData.uv0));

			mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexData.indices.Length), MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
			
			mesh.RecalculateBounds();

			return mesh;
		}
		
		/*private Mesh CreateMesh(ref VertexData vertexData)
		{
			Mesh mesh = new()
			{
				indexFormat = IndexFormat.UInt32,
				vertices = UnsafeUtils.ConvertToArray(vertexData.vertices),
				normals = UnsafeUtils.ConvertToArray(vertexData.normals),
				tangents = UnsafeUtils.ConvertToArray(vertexData.tangents),
			};

			mesh.SetUVs(0, UnsafeUtils.ConvertToArray(vertexData.uv0));

			// Split indices into two halves
			int halfIndexCount = vertexData.indices.Length / 2;
			int[] indicesPart1 = new int[halfIndexCount];
			int[] indicesPart2 = new int[vertexData.indices.Length - halfIndexCount];

			Array.Copy(UnsafeUtils.ConvertToArray(vertexData.indices), 0, indicesPart1, 0, halfIndexCount);
			Array.Copy(UnsafeUtils.ConvertToArray(vertexData.indices), halfIndexCount, indicesPart2, 0, vertexData.indices.Length - halfIndexCount);

			mesh.subMeshCount = 2;
			// Assign the indices to the submeshes
			mesh.SetIndices(indicesPart1, MeshTopology.Triangles, 0);
			mesh.SetIndices(indicesPart2, MeshTopology.Triangles, 1);
			
			// Set up the submeshes
			//mesh.SetSubMesh(0, new SubMeshDescriptor(0, indicesPart1.Length), MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
			mesh.SetSubMesh(1, new SubMeshDescriptor(halfIndexCount, indicesPart2.Length), MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);

			
			mesh.RecalculateBounds();

			return mesh;
		}*/
	}
}
