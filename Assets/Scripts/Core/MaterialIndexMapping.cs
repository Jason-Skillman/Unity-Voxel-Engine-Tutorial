namespace VoxelEngine.Core
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Maps a material index to a face of a cube.
	/// </summary>
	public struct MaterialIndexMapping
	{
		public int front;
		public int back;
		public int left;
		public int right;
		public int up;
		public int down;

		public MaterialIndexMapping(int materialIndex)
		{
			front = back = left = 
				right = up = down = materialIndex;
		}

		[Pure]
		public int GetMaterialIndex(Direction direction) =>
			direction switch
			{
				Direction.Forward => front,
				Direction.Backward => back,
				Direction.Left => left,
				Direction.Right => right,
				Direction.Up => up,
				Direction.Down => down,
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAll(int materialIndex)
		{
			front = back = left = 
				right = up = down = materialIndex;
		}
	}
}
