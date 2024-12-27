namespace VoxelEngine.Utilities
{
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;
	using UnityEngine;

	public static class UnsafeUtils
	{
		public static int[] ConvertToArray(UnsafeList<int> list)
		{
			// Allocate a new array with the same length as the UnsafeList
			int[] array = new int[list.Length];

			// Copy each element from the UnsafeList to the array
			for (int i = 0; i < list.Length; i++)
			{
				int value = list[i];
				array[i] = value;
			}

			return array;
		}
		
		public static Vector3[] ConvertToArray(UnsafeList<float3> unsafeList)
		{
			// Allocate a new array with the same length as the UnsafeList
			Vector3[] array = new Vector3[unsafeList.Length];

			// Copy each element from the UnsafeList to the array
			for (int i = 0; i < unsafeList.Length; i++)
			{
				float3 value = unsafeList[i];
				array[i] = new Vector3(value.x, value.y, value.z);
			}

			return array;
		}
		
		public static Vector4[] ConvertToArray(UnsafeList<float4> unsafeList)
		{
			// Allocate a new array with the same length as the UnsafeList
			Vector4[] array = new Vector4[unsafeList.Length];

			// Copy each element from the UnsafeList to the array
			for (int i = 0; i < unsafeList.Length; i++)
			{
				float4 value = unsafeList[i];
				array[i] = new Vector4(value.x, value.y, value.z, value.w);
			}

			return array;
		}
	}
}
