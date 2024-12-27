namespace VoxelEngine.Core
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Runtime.CompilerServices;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;
	using VoxelEngine.Utilities;

	/// <summary>
	/// Represents a collection of blocks within a chunk.
	/// </summary>
	public struct Chunk : IDisposable
	{
		/// <summary>
		/// The width of the chunk.
		/// </summary>
		public const int Width = 16;
		
		/// <summary>
		/// The length of the chunk.
		/// </summary>
		public const int Length = Width;
		
		/// <summary>
		/// The height of the chunk.
		/// </summary>
		public const int Height = 16;
		
		/// <summary>
		/// The amount of blocks in a single horizontal layer.
		/// </summary>
		public const int LayerCount = Width * Length;

		/// <summary>
		/// The maximum amount of blocks that can fit in a chunk.
		/// </summary>
		public const int TotalBlockCount = Width * Length * Height;
		
		/// <summary>
		/// The maximum amount of possible vertices.
		/// </summary>
		public const int MaxVertices24 = TotalBlockCount * CubeBuilderUtils.CubeMaxVertices24;
		
		/// <summary>
		/// The maximum amount of possible indices.
		/// </summary>
		public const int MaxIndices = TotalBlockCount * CubeBuilderUtils.CubeMaxIndices;

		/// <summary>
		/// The local position of this chunk in the <see cref="ChunkWorld"/>.
		/// </summary>
		private int2 localPosition;
		
		private Allocator allocator;
		
		/// <summary>
		/// Container for all the blocks in this chunk.
		/// </summary>
		public UnsafeList<Block> blocks;
		
		/// <summary>
		/// Contains all the mesh vertex data of all blocks in this chunk.
		/// </summary>
		public VertexData meshVertexData;

		public int2 LocalPosition => localPosition;
		public Allocator Allocator => allocator;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => allocator != Allocator.Invalid;
		}
		
		public Chunk(int x, int y, Allocator allocator) : this(new int2(x, y), allocator) { }

		public Chunk(int2 localPosition, Allocator allocator)
		{
			this.localPosition = localPosition;
			this.allocator = allocator;
			
			blocks = new UnsafeList<Block>(TotalBlockCount, allocator);
			blocks.Resize(TotalBlockCount);
			
			meshVertexData = new VertexData(allocator, MaxVertices24, MaxIndices);

			ClearVertexData();
		}

		public void Dispose()
		{
			blocks.Dispose();
			meshVertexData.Dispose();
			
			allocator = Allocator.Invalid;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearVertexData()
		{
			meshVertexData.Clear();
		}
		
		/// <inheritdoc cref="GetBlock(int,int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Block GetBlock(int3 position) => GetBlock(position.x, position.y, position.z);

		/// <summary>
		/// Retrieves a block at the specified 3D coordinates (x, y, z).
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Block GetBlock(int x, int y, int z)
		{
			int index = GetBlockIndex(x, y, z);
			return GetBlock(index);
		}
		
		/// <summary>
		/// Reterns a block at the <see cref="blockIndex"/>.
		/// Throws an exception if the block index is out of range.
		/// </summary>
		/// <param name="blockIndex">The block index.</param>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Block GetBlock(int blockIndex)
		{
			CheckBlockIndexAndThrowException(blockIndex);
			return blocks[blockIndex];
		}
		
		/// <inheritdoc cref="GetBlockIndex(int,int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBlockIndex(in int3 position) => GetBlockIndex(position.x, position.y, position.z);

		/// <summary>
		/// Calculates the blocks index in the 2d array but using local 3d coordinates.
		/// </summary>
		/// <returns>The index of the block.</returns>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBlockIndex(int x, int y, int z) => x + (z * Width) + (y * Width * Length);
		
		/// <inheritdoc cref="WorldToLocalPosition(int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 WorldToLocalPosition(in int2 position) => WorldToLocalPosition(position.x, position.y);
		
		/// <summary>
		/// Converts a 2d world position to a local 2d chunk position.
		/// </summary>
		/// <param name="x">The x position in world space.</param>
		/// <param name="y">The y position in world space.</param>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 WorldToLocalPosition(int x, int y) =>
			new(
				x % Width,
				y % Length
			);
		
		/// <inheritdoc cref="LocalToWorldPosition(int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 LocalToWorldPosition(in int2 position) => LocalToWorldPosition(position.x, position.y);
		
		/// <summary>
		/// Converts a local 2D chunk position to a 2D world position.
		/// </summary>
		/// <param name="x">The x position in local space.</param>
		/// <param name="y">The y position in local space.</param>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 LocalToWorldPosition(int x, int y) =>
			new(
				x * Width,
				y * Length
			);
		
		/// <summary>
		/// Removes a block at the specified 3D coordinates (x, y, z).
		/// </summary>
		/// <param name="x">The x coordinate in local space.</param>
		/// <param name="y">The y coordinate in local space.</param>
		/// <param name="z">The z coordinate in local space.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveBlock(int x, int y, int z)
		{
			int index = GetBlockIndex(x, y, z);
			RemoveBlock(index);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveBlock(int blockIndex)
		{
			CheckBlockIndexAndThrowException(blockIndex);

			Block blockCopy = blocks[blockIndex];
			blockCopy.blockType = BlockType.Empty;
			blocks[blockIndex] = blockCopy;
		}
		
		/// <summary>
		/// Checks if the adjacent block in the specified direction is solid.
		/// This method uses the direction and block position to determine the adjacent block and returns whether it is solid.
		/// </summary>
		/// <param name="direction">The direction to check from the current block position.</param>
		/// <param name="blockPosition">The position of the current block.</param>
		/// <returns>True if the adjacent block is solid; otherwise, false.</returns>
		[Pure]
		public bool CheckAdjacentBlockIsSolid(Direction direction, in int3 blockPosition)
		{
			int3 directionOffset = DirectionUtils.GetDirection(direction);
			int3 adjacentBlockPosition = directionOffset + blockPosition;

			if(!IsValidPosition(adjacentBlockPosition)) return false;
			
			//Check if the adjacent block position is within the chunk.
			Block adjacentBlock = GetBlock(adjacentBlockPosition);
			return adjacentBlock.IsSolid();
		}
		
		#region Validation

		/// <inheritdoc cref="IsValidPosition(int,int,int)"/>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidPosition(int3 localPosition) => 
			IsValidPosition(localPosition.x, localPosition.y, localPosition.z);
		
		/// <summary>
		/// Returns true if a valid local position in the chunk.
		/// </summary>
		/// <param name="x">Local x position.</param>
		/// <param name="y">Local y position.</param>
		/// <param name="z">Local z position.</param>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidPosition(int x, int y, int z) =>
			x >= 0 && x < Width &&
			y >= 0 && y < Height &&
			z >= 0 && z < Length;

		/// <summary>
		/// Returns true if the index is valid.
		/// </summary>
		/// <param name="index">The blocks index in the chunk.</param>
		/// <returns></returns>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidBlockIndex(int index) => index is >= 0 and < TotalBlockCount;

		/// <summary>
		/// Checks if the block index is valid. If not then an exception is thrown
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CheckBlockIndexAndThrowException(int index)
		{
			if(!IsValidBlockIndex(index))
				throw new IndexOutOfRangeException($"[{nameof(Chunk)}] Block index is invalid {index}");
		}

		#endregion
	}
}
