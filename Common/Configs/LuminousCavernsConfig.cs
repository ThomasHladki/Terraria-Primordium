using System;
using Terraria.ModLoader.Config;

namespace Primordium.Common.Configs
{
	public class LuminousCavernsConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public BiomeSizeSettings Small = new(1, 1, 200, 400, 75, 200);

		public BiomeSizeSettings Medium = new(1, 2, 250, 450, 15, 250);

		public BiomeSizeSettings Large = new(2, 3, 350, 500, 200, 275);
	}

	public class BiomeSizeSettings
	{
		[Range(0, 6)]
		public int MinCount;

		[Range(0, 6)]
		public int MaxCount;

		[Range(60, 700)]
		public int MinWidth;

		[Range(60, 700)]
		public int MaxWidth;

		[Range(40, 400)]
		public int MinHeight;

		[Range(40, 400)]
		public int MaxHeight;

		public BiomeSizeSettings() : this(1, 2, 120, 180, 70, 100) {
		}

		public BiomeSizeSettings(int minCount, int maxCount, int minWidth, int maxWidth, int minHeight, int maxHeight) {
			MinCount = minCount;
			MaxCount = maxCount;
			MinWidth = minWidth;
			MaxWidth = maxWidth;
			MinHeight = minHeight;
			MaxHeight = maxHeight;
		}

		public override bool Equals(object obj) {
			return obj is BiomeSizeSettings other
				&& MinCount == other.MinCount && MaxCount == other.MaxCount
				&& MinWidth == other.MinWidth && MaxWidth == other.MaxWidth
				&& MinHeight == other.MinHeight && MaxHeight == other.MaxHeight;
		}

		public override int GetHashCode() {
			return HashCode.Combine(MinCount, MaxCount, MinWidth, MaxWidth, MinHeight, MaxHeight);
		}
	}
}
