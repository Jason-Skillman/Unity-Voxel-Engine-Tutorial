namespace VoxelEngine.Core
{
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using UnityEngine.Rendering;
	using VoxelEngine.Jobs;
	using VoxelEngine.Utilities;

	public class ChunkWorldManager : MonoBehaviour
	{
		[SerializeField]
		private ChunkBehaviour chunkPrefab;
		
		private ChunkWorld chunkWorld;
		private ChunkBehaviour[] chunkBehaviours;

		public ChunkWorld ChunkWorld => chunkWorld;
		public ref ChunkWorld ChunkWorldRef => ref chunkWorld;

		private void Awake()
		{
			chunkBehaviours = new ChunkBehaviour[ChunkWorld.TotalChunkCount];
			chunkWorld = new ChunkWorld(Allocator.Persistent);
		}

		private void Start()
		{
			Initialize();
		}
		
		private void OnDestroy()
		{
			chunkWorld.Dispose();
		}
		
		private void Initialize()
		{
			JobHandle initializeJob = InitializeChunkJob(chunkWorld);
			initializeJob.Complete();
			
			JobHandle fillJob = FillBlockJob(BlockType.Gravel, chunkWorld);
			fillJob.Complete();
			
			JobHandle buildChunkWorldMeshJob = BuildChunkWorldMeshJob(chunkWorld);
			buildChunkWorldMeshJob.Complete();
			
			//After all vertex data was built, create a mesh and add it to its ChunkBehaviour.
			for(int i = 0; i < ChunkWorld.TotalChunkCount; i++)
			{
				ref Chunk chunk = ref chunkWorld.GetChunkRef(i);
				
				ChunkBehaviour chunkBehaviour = GetOrCreateChunkBehaviour(i, chunk);
				
				Mesh mesh = CreateMesh(ref chunk.meshVertexData);
				chunkBehaviour.SetMesh(mesh);
			}
		}
		
		private ChunkBehaviour GetOrCreateChunkBehaviour(int index, in Chunk chunk)
		{
			ChunkBehaviour chunkBehaviour = chunkBehaviours[index];
			if(chunkBehaviour == null)
			{
				//Create a new ChunkBehaviour if one does not exist
				chunkBehaviour = Instantiate(chunkPrefab, gameObject.transform);
				chunkBehaviour.Initialize(chunk);
				
				chunkBehaviours[index] = chunkBehaviour;
			}

			return chunkBehaviour;
		}

		private Mesh CreateMesh(ref VertexData vertexData)
		{
			Mesh mesh = new()
			{
				indexFormat = IndexFormat.UInt32,
				vertices = UnsafeUtils.ConvertToArray(vertexData.vertices),
				triangles = UnsafeUtils.ConvertToArray(vertexData.indices),
				normals = UnsafeUtils.ConvertToArray(vertexData.normals),
				tangents = UnsafeUtils.ConvertToArray(vertexData.tangents)
			};

			mesh.SetUVs(0, UnsafeUtils.ConvertToArray(vertexData.uv0));
			
			mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexData.indices.Length), MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
			
			mesh.RecalculateBounds();

			return mesh;
		}
		
		public void RemoveBlock(in int3 worldPosition)
		{
			int2 chunkPosition = ChunkWorld.GetChunkPosition(worldPosition.x, worldPosition.z);
			int chunkIndex = ChunkWorld.GetChunkIndex(chunkPosition);
			ref Chunk chunk = ref chunkWorld.GetChunkRef(chunkIndex);
			
			int3 localPosition = ChunkWorld.GetLocalPosition(worldPosition, chunkPosition);
				
			int blockIndex = Chunk.GetBlockIndex(localPosition);
			
			chunkWorld.RemoveBlock(chunkIndex, blockIndex);

			BuildChunkMeshJob buildChunkMeshJob = new()
			{
				chunkWorld = chunkWorld,
				chunkIndex = chunkIndex,
				blockMapping = BlockMapping.blockMapping,
			};
			JobHandle jobHandle = buildChunkMeshJob.Schedule();
			jobHandle.Complete();
			
			
			//Setup mesh
			ChunkBehaviour chunkBehaviour = GetOrCreateChunkBehaviour(chunkIndex, chunk);
			
			Mesh mesh = CreateMesh(ref chunk.meshVertexData);
			chunkBehaviour.SetMesh(mesh);
		}

		#region Jobs

		private static JobHandle InitializeChunkJob(in ChunkWorld chunkWorld)
		{
			NativeArray<JobHandle> jobHandles = new(ChunkWorld.TotalChunkCount, Allocator.Temp);
			
			for(int i = 0; i < ChunkWorld.TotalChunkCount; i++)
			{
				ref Chunk chunk = ref chunkWorld.GetChunkRef(i);
				
				InitializeBlocksJob job = new()
				{
					//Output
					blocks = chunk.blocks,
				};
				jobHandles[i] = job.Schedule();
			}

			JobHandle allJobHandles = JobHandle.CombineDependencies(jobHandles);
			jobHandles.Dispose();
			return allJobHandles;
		}
		
		private static JobHandle FillBlockJob(BlockType blockType, in ChunkWorld chunkWorld)
		{
			NativeArray<JobHandle> jobHandles = new(ChunkWorld.TotalChunkCount, Allocator.Temp);
			
			for(int i = 0; i < ChunkWorld.TotalChunkCount; i++)
			{
				ref Chunk chunk = ref chunkWorld.GetChunkRef(i);
				
				FillBlocksJob job = new()
				{
					//Output
					blocks = chunk.blocks,
					//Input
					blockType = blockType,
				};
				jobHandles[i] = job.Schedule();
			}

			JobHandle allJobHandles = JobHandle.CombineDependencies(jobHandles);
			jobHandles.Dispose();
			return allJobHandles;
		}
		
		private static JobHandle BuildChunkWorldMeshJob(in ChunkWorld chunkWorld)
		{
			BuildChunkWorldJob job = new()
			{
				chunkWorld = chunkWorld,
				blockMapping = BlockMapping.blockMapping,
			};
			JobHandle handle = job.Schedule(chunkWorld.chunks.Length, 1);
			
			return handle;
		}

		#endregion
	}
}
