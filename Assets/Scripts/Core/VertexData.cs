namespace VoxelEngine.Core
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Runtime.CompilerServices;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;

	/// <summary>
	/// Represents vertex data of a mesh.
	/// Contains vertices, indices, normals, tangents, uvs.
	/// </summary>
	public struct VertexData : IDisposable
	{
		public UnsafeList<float3> vertices;
		public UnsafeList<int> indices;
		public UnsafeList<float3> normals;
		public UnsafeList<float4> tangents;
		public UnsafeList<float3> uv0;

		private Allocator allocator;

		public bool IsCreated
		{
			[Pure]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => allocator != Allocator.Invalid;
		}

		public VertexData(Allocator allocator, int initialVerticesCapacity = 0, int initialIndicesCapacity = 0) : this()
		{
			this.allocator = allocator;
			
			vertices = new UnsafeList<float3>(initialVerticesCapacity, allocator);
			indices = new UnsafeList<int>(initialIndicesCapacity, allocator);
			normals = new UnsafeList<float3>(initialVerticesCapacity, allocator);
			tangents = new UnsafeList<float4>(initialVerticesCapacity, allocator);
			uv0 = new UnsafeList<float3>(initialVerticesCapacity, allocator);
		}
		
		public void Dispose()
		{
			vertices.Dispose();
			indices.Dispose();
			normals.Dispose();
			tangents.Dispose();
			uv0.Dispose();
			
			allocator = Allocator.Invalid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			vertices.Clear();
			indices.Clear();
			normals.Clear();
			tangents.Clear();
			uv0.Clear();
		}
	}
}
