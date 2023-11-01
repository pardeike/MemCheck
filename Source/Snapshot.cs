using System;
using System.Linq;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine;

namespace MemCheck
{
	public struct Snapshot
	{
		public DateTime time;
		public string[] textureNames;

		public long usedHeapSizeLong;
		public long monoHeapSizeLong;
		public long totalAllocatedMemoryLong;
		public long totalReservedMemoryLong;
		public long totalUnusedReservedMemoryLong;
		public long monoUsedSizeLong;

		public long currentTextureMemory;
		public long desiredTextureMemory;
		public long nonStreamingTextureCount;
		public long nonStreamingTextureMemory;
		public long streamingTextureCount;
		public long targetTextureMemory;
		public long totalTextureMemory;

		public long totalRuntimeTextureCount;
		public long totalRuntimeTextureMemory;
		public long totalRuntimeMeshCount;
		public long totalRuntimeMeshMemory;

		public long allocatedMemoryForGraphicsDriver;
		public long streamingMipmapUploadCount;
		public long streamingTextureLoadingCount;
		public long streamingTexturePendingLoadCount;

		public Snapshot()
		{
			time = DateTime.Now;

			usedHeapSizeLong = Profiler.usedHeapSizeLong / 1048576L;
			monoHeapSizeLong = Profiler.GetMonoHeapSizeLong() / 1048576L;
			totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong() / 1048576L;
			totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong() / 1048576L;
			totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong() / 1048576L;
			monoUsedSizeLong = Profiler.GetMonoUsedSizeLong() / 1048576L;

			currentTextureMemory = (long)(Texture.currentTextureMemory / 1048576uL);
			desiredTextureMemory = (long)(Texture.desiredTextureMemory / 1048576uL);
			nonStreamingTextureCount = (long)Texture.nonStreamingTextureCount;
			nonStreamingTextureMemory = (long)Texture.nonStreamingTextureMemory / 1048;
			streamingTextureCount = (long)Texture.streamingTextureCount;
			targetTextureMemory = (long)(Texture.targetTextureMemory / 1048576uL);
			totalTextureMemory = (long)(Texture.totalTextureMemory / 1048576uL);

			var allTextures = Resources.FindObjectsOfTypeAll(typeof(Texture));
			textureNames = allTextures.Where(t => t.name?.Length > 0).Select(t => t.name).ToArray();
			totalRuntimeTextureCount = allTextures.Length;
			totalRuntimeTextureMemory = allTextures.Sum(Profiler.GetRuntimeMemorySizeLong) / 1048576L;
			var allMeshes = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			totalRuntimeMeshCount = allMeshes.Length;
			totalRuntimeMeshMemory = allMeshes.Sum(Profiler.GetRuntimeMemorySizeLong) / 1048576L;

			allocatedMemoryForGraphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576L;
			streamingMipmapUploadCount = (long)Texture.streamingMipmapUploadCount;
			streamingTextureLoadingCount = (long)Texture.streamingTextureLoadingCount;
			streamingTexturePendingLoadCount = (long)Texture.streamingTexturePendingLoadCount;
		}

		public override readonly string ToString()
		{
			var str = new StringBuilder();
			str.AppendLine(time.ToLongTimeString());
			str.AppendLine();
			str.AppendLine($"{usedHeapSizeLong} MB");
			str.AppendLine($"{monoHeapSizeLong} MB");
			str.AppendLine($"{totalAllocatedMemoryLong} MB");
			str.AppendLine($"{totalReservedMemoryLong} MB");
			str.AppendLine($"{totalUnusedReservedMemoryLong} MB");
			str.AppendLine($"{monoUsedSizeLong} MB");
			str.AppendLine();
			str.AppendLine($"{currentTextureMemory} MB");
			str.AppendLine($"{desiredTextureMemory} MB");
			str.AppendLine($"{nonStreamingTextureCount}");
			str.AppendLine($"{nonStreamingTextureMemory} MB");
			str.AppendLine($"{streamingTextureCount}");
			str.AppendLine($"{targetTextureMemory} MB");
			str.AppendLine($"{totalTextureMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalRuntimeTextureCount}");
			str.AppendLine($"{totalRuntimeTextureMemory} MB");
			str.AppendLine($"{totalRuntimeMeshCount}");
			str.AppendLine($"{totalRuntimeMeshMemory} MB");
			str.AppendLine();
			str.AppendLine($"{allocatedMemoryForGraphicsDriver}");
			str.AppendLine($"{streamingMipmapUploadCount}");
			str.AppendLine($"{streamingTextureLoadingCount}");
			str.AppendLine($"{streamingTexturePendingLoadCount}");
			return str.ToString();
		}

		public readonly string ToString(Snapshot from)
		{
			var str = new StringBuilder();
			str.AppendLine($"-{time - from.time:m\\:ss}");
			str.AppendLine();
			str.AppendLine($"{usedHeapSizeLong - from.usedHeapSizeLong} MB");
			str.AppendLine($"{monoHeapSizeLong - from.monoHeapSizeLong} MB");
			str.AppendLine($"{totalAllocatedMemoryLong - from.totalAllocatedMemoryLong} MB");
			str.AppendLine($"{totalReservedMemoryLong - from.totalReservedMemoryLong} MB");
			str.AppendLine($"{totalUnusedReservedMemoryLong - from.totalUnusedReservedMemoryLong} MB");
			str.AppendLine($"{monoUsedSizeLong - from.monoUsedSizeLong} MB");
			str.AppendLine();
			str.AppendLine($"{currentTextureMemory - from.currentTextureMemory} MB");
			str.AppendLine($"{desiredTextureMemory - from.desiredTextureMemory} MB");
			str.AppendLine($"{nonStreamingTextureCount - from.nonStreamingTextureCount}");
			str.AppendLine($"{nonStreamingTextureMemory - from.nonStreamingTextureMemory} MB");
			str.AppendLine($"{streamingTextureCount - from.streamingTextureCount}");
			str.AppendLine($"{targetTextureMemory - from.targetTextureMemory} MB");
			str.AppendLine($"{totalTextureMemory - from.totalTextureMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalRuntimeTextureCount - from.totalRuntimeTextureCount}");
			str.AppendLine($"{totalRuntimeTextureMemory - from.totalRuntimeTextureMemory} MB");
			str.AppendLine($"{totalRuntimeMeshCount - from.totalRuntimeMeshCount}");
			str.AppendLine($"{totalRuntimeMeshMemory - from.totalRuntimeMeshMemory} MB");
			str.AppendLine();
			str.AppendLine($"{allocatedMemoryForGraphicsDriver - from.allocatedMemoryForGraphicsDriver}");
			str.AppendLine($"{streamingMipmapUploadCount - from.streamingMipmapUploadCount}");
			str.AppendLine($"{streamingTextureLoadingCount - from.streamingTextureLoadingCount}");
			str.AppendLine($"{streamingTexturePendingLoadCount - from.streamingTexturePendingLoadCount}");
			return str.ToString();
		}
	}
}
