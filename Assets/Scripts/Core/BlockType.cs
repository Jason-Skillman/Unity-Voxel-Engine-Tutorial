namespace VoxelEngine.Core
{
	using System;

	/// <summary>
	/// Defines what type of blocks exist.
	/// </summary>
	public enum BlockType
	{
		Empty,
		Stone,
		Dirt,
		Grass,
		Gravel,
		Wood,
	}
	
	/// <summary>
	/// Utility functions for <see cref="BlockType"/>.
	/// </summary>
	public static class BlockTypeUtils
	{
		/// <summary>
		/// The maximum amount of <see cref="BlockType"/> that exist.
		/// </summary>
		public static int GetBlockTypeCount = Enum.GetValues(typeof(BlockType)).Length;
	}
	
	/// <summary>
	/// Represents a block type that can be compared for equality and provides a custom hash code implementation.
	/// </summary>
	public struct BlockTypeEquatable : IEquatable<BlockTypeEquatable>
	{
		public BlockType Value;

		public BlockTypeEquatable(BlockType value)
		{
			Value = value;
		}

		public bool Equals(BlockTypeEquatable other)
		{
			return Value == other.Value;
		}

		public override int GetHashCode() => (int)Value;
	}
}
