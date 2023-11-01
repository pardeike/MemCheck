using Brrainz;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

			var labelBuilder = new StringBuilder();
			labelBuilder.AppendLine($"    Time:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Used Heap:");
			labelBuilder.AppendLine($"Mono Heap:");
			labelBuilder.AppendLine($"Total Allocated:");
			labelBuilder.AppendLine($"Total Reserved:");
			labelBuilder.AppendLine($"Total Unused Reserved:");
			labelBuilder.AppendLine($"Mono Used:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Current Tex:");
			labelBuilder.AppendLine($"Desired Tex:");
			labelBuilder.AppendLine($"Non Streaming Tex #:");
			labelBuilder.AppendLine($"Non Streaming Tex:");
			labelBuilder.AppendLine($"Streaming Tex #:");
			labelBuilder.AppendLine($"Target Tex:");
			labelBuilder.AppendLine($"Total Tex:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Total Runtime Textures:");
			labelBuilder.AppendLine($"Total Runtime Text Mem:");
			labelBuilder.AppendLine($"Total Runtime Meshs:");
			labelBuilder.AppendLine($"Total Runtime Mesh Mem:");
			labelBuilder.AppendLine();
			labelBuilder.AppendLine($"Allocated Mem Graphics Driver:");
			labelBuilder.AppendLine($"Streaming Tex Mipmap Upload #:");
			labelBuilder.AppendLine($"Streaming Tex Loading #:");
			labelBuilder.AppendLine($"Streaming Tex Pending Load #:");
			labelBuilder.AppendLine();
			labelBuilder.Append($"Target Budget: {QualitySettings.streamingMipmapsMemoryBudget} MB");

			if (DateTime.Now > nextUpdate)
			{
				current = new Snapshot();
				nextUpdate = DateTime.Now.AddMilliseconds(500);
			}
			var labels = labelBuilder.ToString();

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
			if (Widgets.ButtonText(br.LeftPartPixels((columnWidth - padding - 5) / 2), "add"))
				snapshots.Add(current);
			if (snapshots.Count > 0 && Widgets.ButtonText(br.RightPartPixels((columnWidth - padding - 5) / 2), "tex"))
				Log.Error($"New textures: " + current.textureNames.Except(snapshots[0].textureNames).Join());

			r = r.ExpandedBy(-padding);
			Widgets.Label(r, labels);
			r.width = columnWidth;
			r.x += labelWidth;
			r.height -= bottomHeight;
			Widgets.Label(r, current.ToString());

			if (snapshots.Count > 0)
				for (var i = snapshots.Count - 1; i >= 0; i--)
				{
					r.x += columnWidth;
					br.x += columnWidth;
					var over = Mouse.IsOver(r);
					if (over)
					{
						var r2 = new Rect(r.x - 5, r.y - 5, r.width, r.height + 10);
						Widgets.DrawRectFast(r2, new Color(0, 0, 0, 0.2f));
					}
					var str = over ? snapshots[i].ToString(i == snapshots.Count - 1 ? current : snapshots[i + 1]) : snapshots[i].ToString();
					Widgets.Label(r, str);

					if (Widgets.ButtonText(br.LeftPartPixels((columnWidth - padding - 5) / 2), "del"))
					{
						snapshots.RemoveAt(i);
						break;
					}
				}
		}
	}
}