namespace VoxelEngine.Core
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;
	using UnityEngine;

	/// <summary>
	/// Contains all chunks in a world
	/// </summary>
	public struct ChunkWorld : IDisposable
	{
		public const int Width = 10; //X
		public const int Length = Width; //Y
		public const int TotalChunkCount = Width * Length;
		
		private Allocator allocator;
		public Allocator Allocator => allocator;
		
		public UnsafeList<Chunk> chunks;

		public bool IsCreated
		{
			[Pure]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => allocator != Allocator.Invalid;
		}

		public ChunkWorld(Allocator allocator)
		{
			this.allocator = allocator;
			
			chunks = new UnsafeList<Chunk>(TotalChunkCount, allocator);
			chunks.Resize(TotalChunkCount);

			for(int y = 0; y < Length; y++)
			for(int x = 0; x < Width; x++)
			{
				int index = GetChunkIndex(x, y);
				chunks[index] = new Chunk(x, y, allocator);
			}
		}
		
		public void Dispose()
		{
			for(int i = 0; i < TotalChunkCount; i++) 
				chunks[i].Dispose();
			chunks.Dispose();

			allocator = Allocator.Invalid;
		}

		/// <inheritdoc cref="GetChunkIndex(int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetChunkIndex(in int2 chunkPosition) => GetChunkIndex(chunkPosition.x, chunkPosition.y);

		/// <summary>
		/// Calculates the chunk index based on local x and y coordinates.
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetChunkIndex(int x, int y) => x + (y * Width);
		
		/// <inheritdoc cref="GetChunkPosition(int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 GetChunkPosition(in int2 worldPosition) => 
			GetChunkPosition(worldPosition.x, worldPosition.y);
		
		/// <summary>
		/// Calculates the chunk position from a given position in the world.
		/// </summary>
		/// <param name="x">The x position in world space.</param>
		/// <param name="y">The y position in world space.</param>
		/// <returns>The chunk position.</returns>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 GetChunkPosition(int x, int y) =>
			new(
				Mathf.FloorToInt(x / (float)Chunk.Width),
				Mathf.FloorToInt(y / (float)Chunk.Width)
			);
		
		/// <summary>
		/// Calculates the local position of a block in a chunk using the world space position.
		/// </summary>
		/// <param name="worldPosition">The blocks position in world space.</param>
		/// <param name="chunkPosition">The 2d position of the chunk.</param>
		/// <returns>The local position of the block in the chunk.</returns>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int3 GetLocalPosition(in int3 worldPosition, in int2 chunkPosition) =>
			new(
				worldPosition.x - chunkPosition.x * Chunk.Width,
				worldPosition.y,
				worldPosition.z - chunkPosition.y * Chunk.Width
			);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveBlock(int chunkIndex, int blockIndex)
		{
			ref Chunk chunk = ref GetChunkRef(chunkIndex);
			chunk.RemoveBlock(blockIndex);
		}
		
		#region GetChunk

		/// <summary>
		/// Returns a copy of the chunk at <see cref="chunkIndex"/>.
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Chunk GetChunk(int chunkIndex)
		{
			CheckChunkIndexAndThrowException(chunkIndex);
			return chunks[chunkIndex];
		}
		
		/// <summary>
		/// Returns a reference of the chunk at <see cref="chunkIndex"/>.
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref Chunk GetChunkRef(int chunkIndex)
		{
			CheckChunkIndexAndThrowException(chunkIndex);
			return ref chunks.ElementAt(chunkIndex);
		}

		/// <summary>
		/// Returns a copy of the chunk using local x and y coordinates.
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Chunk GetChunk(int x, int y)
		{
			int chunkIndex = GetChunkIndex(x, y);
			return GetChunk(chunkIndex);
		}
		
		/// <summary>
		/// Returns a reference of the chunk using local x and y coordinates.
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref Chunk GetChunkRef(int x, int y)
		{
			int chunkIndex = GetChunkIndex(x, y);
			return ref GetChunkRef(chunkIndex);
		}

		#endregion

		#region Validation

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidPosition(in int2 worldPosition)
		{
			//Early out if world position is in the negatives in either x or y direction or
			//if world position is larger than the last chunk.
			if(worldPosition.x < 0 || worldPosition.y < 0 ||
			   worldPosition.x >= Width * Chunk.Width || worldPosition.y >= Length * Chunk.Length) return false;
			
			int2 chunkPosition = GetChunkPosition(worldPosition);
			int chunkIndex = GetChunkIndex(chunkPosition);
			return chunkIndex >= 0 && chunkIndex < TotalChunkCount;
		}
		
		/// <summary>
		/// Returns true if the index is valid.
		/// </summary>
		/// <param name="index">The blocks index in the chunk.</param>
		/// <returns></returns>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidChunkIndex(int index) => index is >= 0 and < TotalChunkCount;
		
		/// <summary>
		/// Checks if the block index is valid. If not then an <see cref="IndexOutOfRangeException"/> is thrown
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CheckChunkIndexAndThrowException(int index)
		{
			if(!IsValidChunkIndex(index))
				throw new IndexOutOfRangeException($"[{nameof(Chunk)}] Chunk index is invalid {index}");
		}

		#endregion
	}
}
