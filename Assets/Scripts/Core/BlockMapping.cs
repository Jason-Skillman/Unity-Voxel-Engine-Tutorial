namespace VoxelEngine.Core
{
	using Unity.Collections;
	using UnityEngine;

	/// <summary>
	/// <para>
	/// Contains a mapping between <see cref="BlockType"/> and <see cref="BlockDefinition"/>.
	/// This is used so that every block can have one centralized location to lookup what defines that block.
	/// </para>
	/// <para>
	/// Should be readonly as it should not be modified at runtime.
	/// This variable cant be readonly because then it could not be disposed when the application is shutdown since Dispose() is not a pure method.
	/// </para>
	/// <para>
	/// Use "ref BlockMapping.blockMapping" when using or writing burst methods.
	/// "ref" is needed instead of "in" since NativeHashMap does not have any pure methods due to how Unity manages/uses memory.
	/// </para>
	/// </summary>
	public static class BlockMapping
	{
		public static NativeHashMap<BlockTypeEquatable, BlockDefinition> blockMapping;

		static BlockMapping()
		{
			blockMapping = new NativeHashMap<BlockTypeEquatable, BlockDefinition>(BlockTypeUtils.GetBlockTypeCount, Allocator.Persistent);
			
			blockMapping.Add(new BlockTypeEquatable(BlockType.Empty), default);
			blockMapping.Add(new BlockTypeEquatable(BlockType.Stone), new BlockDefinition
			{
				materialIndexMapping = new MaterialIndexMapping(1),
			});
			blockMapping.Add(new BlockTypeEquatable(BlockType.Dirt), new BlockDefinition
			{
				materialIndexMapping = new MaterialIndexMapping(2),
			});
			blockMapping.Add(new BlockTypeEquatable(BlockType.Grass), new BlockDefinition
			{
				materialIndexMapping = new MaterialIndexMapping
				{
					front = 3,
					back = 3,
					left = 3,
					right = 3,
					up = 0,
					down = 2,
				},
			});
			blockMapping.Add(new BlockTypeEquatable(BlockType.Gravel), new BlockDefinition
			{
				materialIndexMapping = new MaterialIndexMapping(27),
			});
			blockMapping.Add(new BlockTypeEquatable(BlockType.Wood), new BlockDefinition
			{
				materialIndexMapping = new MaterialIndexMapping(4),
			});
			
			Application.quitting += OnApplicationQuit;
		}

		private static void OnApplicationQuit()
		{
			blockMapping.Dispose();
		}
	}
}
