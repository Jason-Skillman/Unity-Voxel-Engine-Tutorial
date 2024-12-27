namespace VoxelEngine.Jobs
{
	using Unity.Burst;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Jobs;
	using VoxelEngine.Core;

	/// <summary>
	/// Fills a chunk with blocks.
	/// </summary>
	[BurstCompile]
	public struct FillBlocksJob : IJob
	{
		//Output
		public UnsafeList<Block> blocks;
		
		//Input
		public BlockType blockType;

		public void Execute()
		{
			for(int y = 0; y < Chunk.Height; y++)
			for(int z = 0; z < Chunk.Width; z++)
			for(int x = 0; x < Chunk.Width; x++)
			{
				int blockIndex = Chunk.GetBlockIndex(x, y, z);

				Block blockCopy = blocks[blockIndex];
				blockCopy.blockType = blockType;
				blocks[blockIndex] = blockCopy;
			}
		}
	}
}
