using System;
using System.Linq;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine;
using Verse;

namespace MemCheck
{
	public struct Snapshot
	{
		public DateTime time;
		public System.WeakReference<Texture>[] textures;
		public System.WeakReference<Material>[] materials;
		public System.WeakReference<Mesh>[] meshes;
		public System.WeakReference<AudioClip>[] audioClips;

		public long monoHeapSizeLong;
		public long monoUsedSizeLong;
		public long usedHeapSizeLong;
		public long totalReservedMemoryLong;
		public long totalUnusedReservedMemoryLong;

		public long currentTextureMemory;
		public long nonStreamingTextureCount;
		public long nonStreamingTextureMemory;

		public long totalTextureCount;
		public long totalTextureMemory;
		public long totalMaterialCount;
		public long totalMaterialMemory;
		public long totalMeshCount;
		public long totalMeshMemory;
		public long totalObjectCount;
		public long totalAudioClipCount;

		public readonly string[] TextureNames => textures.ObjectNames();
		public readonly string[] MaterialNames => materials.ObjectNames();
		public readonly string[] MeshNames => meshes.ObjectNames();
		public readonly string[] AudioClipNames => audioClips.ObjectNames();

		public Snapshot()
		{
			time = DateTime.Now;

			monoHeapSizeLong = Profiler.GetMonoHeapSizeLong() / 1048576L;
			monoUsedSizeLong = Profiler.GetMonoUsedSizeLong() / 1048576L;
			usedHeapSizeLong = Profiler.usedHeapSizeLong / 1048576L;
			totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong() / 1048576L;
			totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong() / 1048576L;

			currentTextureMemory = (long)(Texture.currentTextureMemory / 1048576uL);
			nonStreamingTextureCount = (long)Texture.nonStreamingTextureCount;
			nonStreamingTextureMemory = (long)(Texture.nonStreamingTextureMemory / 1048576uL);

			var allTextures = Resources.FindObjectsOfTypeAll(typeof(Texture));
			textures = allTextures.Get<Texture>();
			totalTextureCount = allTextures.Length;
			totalTextureMemory = allTextures.Sum(Profiler.GetRuntimeMemorySizeLong) / 1048576L;

			var allMaterials = Resources.FindObjectsOfTypeAll(typeof(Material));
			materials = allMaterials.Get<Material>();
			totalMaterialCount = allMaterials.Length;
			totalMaterialMemory = allMaterials.Sum(Profiler.GetRuntimeMemorySizeLong) / 1048576L;

			var allMeshes = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			meshes = allMeshes.Get<Mesh>();
			totalMeshCount = allMeshes.Length;
			totalMeshMemory = allMeshes.Sum(Profiler.GetRuntimeMemorySizeLong) / 1048576L;

			totalObjectCount = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length;

			var allAudioClips = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
			audioClips = allAudioClips.Get<AudioClip>();
			totalAudioClipCount = allAudioClips.Length;
		}

		static readonly Color[] colors = [new Color(0.4f, 1f, 0.4f), Color.white, new Color(1f, 0.4f, 0.4f)];
		static string C(long value, long? prevValue, string unit) => $"{value}{unit}".Colorize(prevValue == null ? Color.white : colors[value.CompareTo(prevValue) + 1]);

		public readonly string MainString(Snapshot? prev)
		{
			var str = new StringBuilder();
			str.AppendLine(time.ToLongTimeString());
			str.AppendLine();
			str.AppendLine(C(monoHeapSizeLong, prev?.monoHeapSizeLong, " MB"));
			str.AppendLine(C(monoUsedSizeLong, prev?.monoUsedSizeLong, " MB"));
			str.AppendLine(C(usedHeapSizeLong, prev?.usedHeapSizeLong, " MB"));
			str.AppendLine(C(totalReservedMemoryLong, prev?.totalReservedMemoryLong, " MB"));
			str.AppendLine(C(totalUnusedReservedMemoryLong, prev?.totalUnusedReservedMemoryLong, " MB"));
			str.AppendLine();
			str.AppendLine(C(currentTextureMemory, prev?.currentTextureMemory, " MB"));
			str.AppendLine(C(nonStreamingTextureCount, prev?.nonStreamingTextureCount, ""));
			str.AppendLine(C(nonStreamingTextureMemory, prev?.nonStreamingTextureMemory, " MB"));
			str.AppendLine();
			str.AppendLine(C(totalTextureCount, prev?.totalTextureCount, ""));
			str.AppendLine(C(totalTextureMemory, prev?.totalTextureMemory, " MB"));
			str.AppendLine();
			str.AppendLine(C(totalMaterialCount, prev?.totalMaterialCount, ""));
			str.AppendLine(C(totalMaterialMemory, prev?.totalMaterialMemory, " MB"));
			str.AppendLine();
			str.AppendLine(C(totalMeshCount, prev?.totalMeshCount, ""));
			str.AppendLine(C(totalMeshMemory, prev?.totalMeshMemory, " MB"));
			str.AppendLine();
			str.AppendLine(C(totalObjectCount, prev?.totalObjectCount, ""));
			str.AppendLine(C(totalAudioClipCount, prev?.totalAudioClipCount, ""));
			return str.ToString();
		}

		public readonly string DiffString(Snapshot from)
		{
			var str = new StringBuilder();
			str.AppendLine($"-{time - from.time:m\\:ss}");
			str.AppendLine();
			str.AppendLine($"{monoHeapSizeLong - from.monoHeapSizeLong} MB");
			str.AppendLine($"{monoUsedSizeLong - from.monoUsedSizeLong} MB");
			str.AppendLine($"{usedHeapSizeLong - from.usedHeapSizeLong} MB");
			str.AppendLine($"{totalReservedMemoryLong - from.totalReservedMemoryLong} MB");
			str.AppendLine($"{totalUnusedReservedMemoryLong - from.totalUnusedReservedMemoryLong} MB");
			str.AppendLine();
			str.AppendLine($"{currentTextureMemory - from.currentTextureMemory} MB");
			str.AppendLine($"{nonStreamingTextureCount - from.nonStreamingTextureCount}");
			str.AppendLine($"{nonStreamingTextureMemory - from.nonStreamingTextureMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalTextureCount - from.totalTextureCount}");
			str.AppendLine($"{totalTextureMemory - from.totalTextureMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalMaterialCount - from.totalMaterialCount}");
			str.AppendLine($"{totalMaterialMemory - from.totalMaterialMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalMeshCount - from.totalMeshCount}");
			str.AppendLine($"{totalMeshMemory - from.totalMeshMemory} MB");
			str.AppendLine();
			str.AppendLine($"{totalObjectCount - from.totalObjectCount}");
			str.AppendLine($"{totalAudioClipCount - from.totalAudioClipCount}");
			return str.ToString();
		}

		public static string Labels()
		{
			var labelBuilder = new StringBuilder();
			labelBuilder.AppendLine($"      Time:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Mono Heap:");
			labelBuilder.AppendLine($"Mono Used:");
			labelBuilder.AppendLine($"Used Heap:");
			labelBuilder.AppendLine($"Total Reserved:");
			labelBuilder.AppendLine($"Total Unused Reserved:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Current Textures:");
			labelBuilder.AppendLine($"Non Streaming Tex #:");
			labelBuilder.AppendLine($"Non Streaming Tex:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Total Textures:");
			labelBuilder.AppendLine($"Total Texture Mem:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Total Materials:");
			labelBuilder.AppendLine($"Total Material Mem:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Total Meshs:");
			labelBuilder.AppendLine($"Total Mesh Mem:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Total Objects:");
			labelBuilder.AppendLine($"Total AudioClips:");
			labelBuilder.AppendLine();
			labelBuilder.Append($"Target Budget: {QualitySettings.streamingMipmapsMemoryBudget} MB");
			return labelBuilder.ToString();
		}
	}
}
