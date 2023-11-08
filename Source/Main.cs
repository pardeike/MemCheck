using Brrainz;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace MemCheck
{
	public class MemCheck_Main : Mod
	{
		public MemCheck_Main(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("net.pardeike.memcheck");
			harmony.PatchAll();

			CrossPromotion.Install(76561197973010050);
		}
	}

	[HarmonyPatch]
	public static class DebugWindowOnGUI_Patch
	{
		static Snapshot current = new();
		static DateTime nextUpdate = DateTime.Now;
		static IntVec2 topLeft = new(10, 10), topLeftDrag = IntVec2.Zero;
		static Vector2 lastMouseDown;
		static bool isDragging = false;
		static readonly List<Snapshot> snapshots = new() { };
		static float lastUIScale = -1f, labelWidth = 0f, textHeight = 0f, bottomHeight = 0f;

		static Snapshot GetSnapshot(int idx) => idx == 0 ? current : snapshots[idx - 1];
		static void RemoveSnapshot(int idx) => snapshots.RemoveAt(idx - 1);

		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return SymbolExtensions.GetMethodInfo((UIRoot_Entry me) => me.DoMainMenu());
			yield return SymbolExtensions.GetMethodInfo(() => DebugTools.DebugToolsOnGUI());
		}

		public static void Postfix()
		{
			var eventType = Event.current.type;
			if (eventType == EventType.Layout)
				return;

			if (DateTime.Now > nextUpdate)
			{
				current = new Snapshot();
				nextUpdate = DateTime.Now.AddMilliseconds(500);
			}
			var labels = Snapshot.Labels();

			Text.Font = GameFont.Tiny;
			GUI.color = Color.white;
			var columnWidth = 80f;
			var padding = 10f;
			if (labelWidth == 0 || Prefs.UIScale != lastUIScale)
			{
				lastUIScale = Prefs.UIScale;
				var size = Text.CalcSize(labels);
				labelWidth = size.x + padding;
				textHeight = size.y;
				bottomHeight = Text.CalcSize("XXX\nXXX").y;
			}

			var x = Mathf.Clamp(topLeft.x + topLeftDrag.x, 0, UI.screenWidth - 16);
			var z = Mathf.Clamp(topLeft.z + topLeftDrag.z, 0, UI.screenHeight - 16);

			var r = new Rect(x, z, labelWidth + columnWidth * (snapshots.Count + 1) + 2 * padding, textHeight + 2 * padding);
			Widgets.DrawRectFast(r, new Color(0, 0, 0, 0.6f));

			var drag = new Rect(r.x, r.y, 16, 16);
			Widgets.DrawRectFast(drag, new Color(1, 1, 1, 0.2f));
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(drag, "☰");
			Text.Anchor = TextAnchor.UpperLeft;

			var mousePosition = Event.current.mousePosition;
			var mousePressed = Input.GetMouseButton(0);
			if (mousePressed == false)
			{
				if (isDragging)
				{
					topLeft += topLeftDrag;
					topLeftDrag = IntVec2.Zero;
				}
				isDragging = false;
			}
			if (Mouse.IsOver(drag) && isDragging == false && mousePressed)
			{
				lastMouseDown = mousePosition;
				isDragging = true;
				Event.current.Use();
			}
			else if (isDragging && eventType == EventType.MouseDrag)
			{
				var delta = mousePosition - lastMouseDown;
				topLeftDrag.x = (int)delta.x;
				topLeftDrag.z = (int)delta.y;
				Event.current.Use();
			}

			var br = new Rect(r.x + labelWidth + padding, r.y + r.height - padding - 22, columnWidth - padding, 22);
			if (Widgets.ButtonImageFitted(br.LeftPartPixels((columnWidth - padding - 5) / 2), Assets.addTexture))
				snapshots.Insert(0, current);

			if (snapshots.Count > 0)
				if (Widgets.ButtonImageFitted(br.RightPartPixels((columnWidth - padding - 5) / 2).LeftPartPixels(22), Assets.shareTexture))
					MakeMenu(0);

			r = r.ExpandedBy(-padding);
			Widgets.Label(r, labels);
			r.width = columnWidth;
			r.x += labelWidth;
			r.height -= bottomHeight;
			Widgets.Label(r, current.MainString(snapshots.Count > 0 ? snapshots.First() : null));

			for (var idx = 1; idx <= snapshots.Count; idx++)
			{
				r.x += columnWidth;
				br.x += columnWidth;
				var over = Mouse.IsOver(r);
				if (over)
				{
					var r2 = new Rect(r.x - 5, r.y - 5, r.width, r.height + 10);
					Widgets.DrawRectFast(r2, new Color(0, 0, 0, 0.2f));
				}
				var str = over
					? GetSnapshot(idx).DiffString(GetSnapshot(idx - 1))
					: GetSnapshot(idx).MainString(idx < snapshots.Count ? GetSnapshot(idx + 1) : null);
				Widgets.Label(r, str);

				if (Widgets.ButtonImageFitted(br.LeftPartPixels((columnWidth - padding - 5) / 2), Assets.deleteTexture))
				{
					RemoveSnapshot(idx);
					break;
				}

				if (idx < snapshots.Count)
					if (Widgets.ButtonImageFitted(br.RightPartPixels((columnWidth - padding - 5) / 2).LeftPartPixels(22), Assets.shareTexture))
						MakeMenu(idx);
			}
		}

		static void MakeMenu(int idx)
		{
			var options = new List<FloatMenuOption>();
			if (snapshots.Count > 0)
			{
				options.Add(new FloatMenuOption("Textures: Log names", () => LogNewTextures(idx)));
				options.Add(new FloatMenuOption("Textures: Export", () => ExportNewTextures(idx)));
				options.Add(new FloatMenuOption("Materials: Log names", () => LogNewMaterials(idx)));
				options.Add(new FloatMenuOption("Materials: Export", () => ExportNewMaterials(idx)));
				options.Add(new FloatMenuOption("Audio Clips: Log names", () => LogNewAudioClips(idx)));
				options.Add(new FloatMenuOption("Audio Clips: Export", () => ExportNewAudioClips(idx)));
				options.Add(new FloatMenuOption("Meshes: Log names", () => LogNewMeshes(idx)));
			}

			var menu = new FloatMenu(options);
			Find.WindowStack.Add(menu);
		}

		static void LogNewTextures(int idx)
		{
			var previousTextures = GetSnapshot(idx + 1).textures.Get().OfType<Texture2D>().ToHashSet();
			var textures = GetSnapshot(idx).textures.Get().OfType<Texture2D>().Except(previousTextures);
			Log.Error($"New texture names: " + textures.Select(t => t.name).OfType<string>().Join());
		}

		static void ExportNewTextures(int idx)
		{
			var path = Tools.CreateExportFolder();
			if (path == null)
				return;

			var previousTextures = GetSnapshot(idx + 1).textures.Get().OfType<Texture2D>().ToHashSet();
			var textures = GetSnapshot(idx).textures.Get().OfType<Texture2D>().Except(previousTextures);
			var prefix = $"texture-{DateTime.Now:HHmmss}_";

			var counter = 0;
			foreach (var texture in textures)
			{
				var temp = Tools.CreateReadableCopy(texture);
				var filename = $"{prefix}{++counter:D6}.png";
				Tools.SaveTexture(temp, path, filename);
				UnityEngine.Object.DestroyImmediate(temp);
			}

			Process.Start(new Uri(path).AbsoluteUri);
		}

		static void LogNewMaterials(int idx)
		{
			var previousMaterials = GetSnapshot(idx + 1).materials.Get().OfType<Material>().ToHashSet();
			var materials = GetSnapshot(idx).materials.Get().OfType<Material>().Except(previousMaterials);
			Log.Error($"New material names: " + materials.Select(m => $"{m.name ?? "-"}[{m.mainTexture?.name ?? "-"}]").Join());
		}

		static void ExportNewMaterials(int idx)
		{
			var path = Tools.CreateExportFolder();
			if (path == null)
				return;

			var previousMaterials = GetSnapshot(idx + 1).materials.Get().OfType<Material>().ToHashSet();
			var materials = GetSnapshot(idx).materials.Get().OfType<Material>().Except(previousMaterials);
			var prefix = $"material-{DateTime.Now:HHmmss}_";

			var counter = 0;
			foreach (var material in materials)
			{
				if (material.mainTexture is not Texture2D texture2D)
					continue;

				var temp = Tools.CreateReadableCopy(texture2D);
				var filename = $"{prefix}{++counter:D6}.png";
				Tools.SaveTexture(temp, path, filename);
				UnityEngine.Object.DestroyImmediate(temp);
			}

			Process.Start(new Uri(path).AbsoluteUri);
		}

		static void LogNewAudioClips(int idx)
		{
			var previousAudioClips = GetSnapshot(idx + 1).audioClips.Get().OfType<AudioClip>().ToHashSet();
			var audioClips = GetSnapshot(idx).audioClips.Get().OfType<AudioClip>().Except(previousAudioClips);
			Log.Error($"New audio clip names: " + audioClips.Select(m => $"{m.name ?? "-"}[{m.length}s]").Join());
		}

		static void ExportNewAudioClips(int idx)
		{
			var path = Tools.CreateExportFolder();
			if (path == null)
				return;

			var previousAudioClips = GetSnapshot(idx + 1).audioClips.Get().OfType<AudioClip>().ToHashSet();
			var audioClips = GetSnapshot(idx).audioClips.Get().OfType<AudioClip>().Except(previousAudioClips);
			var prefix = $"audioclip-{DateTime.Now:HHmmss}_";

			var counter = 0;
			foreach (var audioClip in audioClips)
			{
				var filename = $"{prefix}{++counter:D6}.png";
				SaveWav.Save(filename, path, audioClip);
			}

			Process.Start(new Uri(path).AbsoluteUri);
		}

		static void LogNewMeshes(int idx)
		{
			var previousMeshes = GetSnapshot(idx + 1).meshes.Get().OfType<Mesh>().ToHashSet();
			var meshes = GetSnapshot(idx).materials.Get().OfType<Mesh>().Except(previousMeshes);
			Log.Error($"New mesh names: " + meshes.Select(m => $"{m.name ?? "-"}[{m.triangles.Length}]").Join());
		}
	}
}