using Microsoft.Xna.Framework;
using Primordium.Content.Items.Placeable;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class LuminousMoss : ModTile
	{
		// Random updates hit each tile rarely, so these are per-update odds, not per-second odds.
		private const int SpreadChance = 6;
		private const int VineChance = 12;
		private const int BlossomChance = 18;

		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<Lumenstone>();

			int lumenstone = ModContent.TileType<Lumenstone>();
			Main.tileMerge[Type][lumenstone] = true;
			Main.tileMerge[lumenstone][Type] = true;

			RegisterItemDrop(ModContent.ItemType<LumenstoneBlock>());
			DustType = DustID.BlueMoss;
			HitSound = SoundID.Tink;
			AddMapEntry(new Color(40, 150, 150), CreateMapEntryName());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.02f;
			g = 0.14f;
			b = 0.16f;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			// Like grass on dirt: a failed pickaxe hit strips the moss and leaves the stone.
			if (fail && !effectOnly) {
				Main.tile[i, j].TileType = (ushort)ModContent.TileType<Lumenstone>();
				WorldGen.SquareTileFrame(i, j);
			}
		}

		public override void RandomUpdate(int i, int j) {
			if (WorldGen.genRand.NextBool(SpreadChance)) {
				TrySpread(i + WorldGen.genRand.Next(-1, 2), j + WorldGen.genRand.Next(-1, 2));
			}

			Tile below = Framing.GetTileSafely(i, j + 1);
			if (!below.HasTile && below.LiquidAmount == 0 && WorldGen.genRand.NextBool(VineChance)) {
				TryGrow(i, j + 1, ModContent.TileType<Glowvine>(), 0);
			}

			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && above.LiquidAmount == 0 && WorldGen.genRand.NextBool(BlossomChance)) {
				TryGrow(i, j - 1, ModContent.TileType<Lumenblossom>(), WorldGen.genRand.Next(3));
			}
		}

		private void TrySpread(int x, int y) {
			if (!WorldGen.InWorld(x, y, 2)) {
				return;
			}

			Tile tile = Main.tile[x, y];
			if (!tile.HasTile || tile.TileType != ModContent.TileType<Lumenstone>() || !HasOpenFace(x, y)) {
				return;
			}

			tile.TileType = Type;
			WorldGen.SquareTileFrame(x, y);
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendTileSquare(-1, x, y);
			}
		}

		private static void TryGrow(int x, int y, int type, int style) {
			if (!WorldGen.InWorld(x, y, 2)) {
				return;
			}

			if (WorldGen.PlaceTile(x, y, type, mute: true, style: style) && Main.netMode == NetmodeID.Server) {
				NetMessage.SendTileSquare(-1, x, y);
			}
		}

		public static bool HasOpenFace(int x, int y) {
			return !WorldGen.SolidTile(x - 1, y) || !WorldGen.SolidTile(x + 1, y)
				|| !WorldGen.SolidTile(x, y - 1) || !WorldGen.SolidTile(x, y + 1);
		}
	}
}
