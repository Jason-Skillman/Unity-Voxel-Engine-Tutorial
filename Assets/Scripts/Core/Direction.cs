namespace VoxelEngine.Core
{
	using System;
	using Unity.Mathematics;

	public enum Direction
	{
		Forward,
		Backward,
		Left,
		Right,
		Up, 
		Down,
	}
	
	public static class DirectionUtils
	{
		public static int3 GetDirection(Direction direction) =>
			direction switch
			{
				Direction.Forward	=> new int3(0, 0, 1),
				Direction.Backward	=> new int3(0, 0, -1),
				Direction.Left		=> new int3(-1, 0, 0),
				Direction.Right		=> new int3(1, 0, 0),
				Direction.Up		=> new int3(0, 1, 0),
				Direction.Down		=> new int3(0, -1, 0),
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
	}
}
