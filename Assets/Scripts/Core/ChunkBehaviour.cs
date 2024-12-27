namespace VoxelEngine.Core
{
	using Unity.Mathematics;
	using UnityEngine;

	public class ChunkBehaviour : MonoBehaviour
	{
		[SerializeField]
		private MeshFilter meshFilter;
		[SerializeField]
		private MeshCollider meshCollider;

		public void Initialize(in Chunk chunk)
		{
			int2 worldChunkPosition = Chunk.LocalToWorldPosition(chunk.LocalPosition);

			transform.localPosition = new Vector3(worldChunkPosition.x, 0.0f, worldChunkPosition.y);
		}

		public void SetMesh(Mesh mesh)
		{
			meshFilter.mesh = mesh;
			meshCollider.sharedMesh = mesh;
		}
	}
}
