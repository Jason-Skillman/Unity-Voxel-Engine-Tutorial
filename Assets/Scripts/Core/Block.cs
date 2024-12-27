namespace VoxelEngine.Core
{
	using System.Diagnostics.Contracts;
	using System.Runtime.CompilerServices;
	using Unity.Mathematics;

	/// <summary>
	/// Represents a single block.
	/// </summary>
	public struct Block
	{
		/// <summary>
		/// This block's block type.
		/// </summary>
		public BlockType blockType;
		
		/// <summary>
		/// The local position of the block in the chunk.
		/// </summary>
		public int3 position;

		/// <summary>
		/// Returns true if the block is considered solid.
		/// <example>Used when building cubes to see if it can skip building a face if the adjacent block is solid.</example>
		/// </summary>
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSolid() => blockType != BlockType.Empty;
	}
}
