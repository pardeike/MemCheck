using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MemCheck
{
	public static class Tools
	{
		public static string CreateExportFolder()
		{
			var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var path = Path.Combine(desktopPath, "MemCheck");
			Directory.CreateDirectory(path);
			return Directory.Exists(path) ? path : null;
		}

		public static Texture2D CreateReadableCopy(Texture2D texture)
		{
			var (w, h) = (texture.width, texture.height);
			var tmp = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			Graphics.Blit(texture, tmp);
			var previous = RenderTexture.active;
			RenderTexture.active = tmp;
			var result = new Texture2D(texture.width, texture.height);
			result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
			result.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(tmp);
			return result;
		}

		public static void SaveTexture(Texture2D texture, string path, string filename)
		{
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(Path.Combine(path, filename), bytes);
		}

		public static WeakReference<T>[] Get<T>(this UnityEngine.Object[] objects) where T : UnityEngine.Object
			=> objects.OfType<T>().Where(o => o.name?.Length > 0).Select(o => new WeakReference<T>(o)).ToArray();

		public static IEnumerable<T> Get<T>(this IEnumerable<WeakReference<T>> objects) where T : UnityEngine.Object
			=> objects.Select(o => o.TryGetTarget(out var t) ? t : null).OfType<T>().ToArray();

		public static string[] ObjectNames<T>(this WeakReference<T>[] objects) where T : UnityEngine.Object
			=> objects.Select(t => t.TryGetTarget(out var tex) ? tex.name : null).OfType<string>().ToArray();
	}
}
