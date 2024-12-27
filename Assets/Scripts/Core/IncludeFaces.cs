namespace VoxelEngine.Core
{
	using System;

	/// <summary>
	/// Specifies the faces of a cube that can be included.
	/// </summary>
	[Flags]
	public enum IncludeFaces
	{
		None  = 0,
		Front = 1 << 0,	//1
		Back  = 1 << 1,	//2
		Left  = 1 << 2,	//4
		Right = 1 << 3,	//8
		Up	  = 1 << 4,	//16
		Down  = 1 << 5,	//32
		All = Front | Back | Left | Right | Up | Down,
	}

	/// <summary>
	/// Utility class for the IncludeFaces, providing constants and arrays related to face inclusion.
	/// </summary>
	public static class IncludeFacesUtils
	{
		public const int IncludeFacesCount = 6;
		
		public static readonly IncludeFaces[] IncludeFacesArray = {
			IncludeFaces.Front,
			IncludeFaces.Back,
			IncludeFaces.Left,
			IncludeFaces.Right,
			IncludeFaces.Up,
			IncludeFaces.Down
		};
	}
}
