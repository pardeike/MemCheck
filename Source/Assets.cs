using UnityEngine;
using Verse;

namespace MemCheck
{
	[StaticConstructorOnStartup]
	public static class Assets
	{
		public static readonly Texture2D addTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);
		public static readonly Texture2D deleteTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);
		public static readonly Texture2D shareTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Paste", true);
	}
}