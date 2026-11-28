using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class Lumenstone : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileStone[Type] = true;
			Main.tileMerge[Type][Type] = true;
			Main.tileMerge[Type][TileID.Stone] = true;
			Main.tileMerge[TileID.Stone][Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

			DustType = DustID.Granite;
			HitSound = SoundID.Tink;
			AddMapEntry(new Color(33, 48, 66), CreateMapEntryName());
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void RandomUpdate(int i, int j) {
			LumenShards.TryGrow(i, j);
		}
	}
}
