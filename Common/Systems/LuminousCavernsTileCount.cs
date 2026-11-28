using System;
using Primordium.Content.Tiles.LuminousCaverns;
using Terraria.ModLoader;

namespace Primordium.Common.Systems
{
	public class LuminousCavernsTileCount : ModSystem
	{
		public int LuminousTileCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			LuminousTileCount = tileCounts[ModContent.TileType<Lumenstone>()] + tileCounts[ModContent.TileType<LuminousMoss>()];
		}
	}
}
