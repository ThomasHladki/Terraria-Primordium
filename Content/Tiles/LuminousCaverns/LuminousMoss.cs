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
		private const int VineChance = 12;
		private const int BlossomChance = 18;

		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			// Same framing path as green moss. The host block is lumenstone, so the glow wraps every open edge
			// and blends into lumenstone where another block covers it. tileMoss would force the host to be stone.
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
			// Green moss scans the 3x3 around itself and grows onto the stone beside it. SpreadGrass has to be
			// aimed at that stone: calling it on the moss tile itself does nothing.
			int lumenstone = ModContent.TileType<Lumenstone>();
			for (int x = i - 1; x <= i + 1; x++) {
				for (int y = j - 1; y <= j + 1; y++) {
					if ((x == i && y == j) || !WorldGen.InWorld(x, y, 10)) {
						continue;
					}

					Tile neighbor = Main.tile[x, y];
					if (neighbor.HasTile && neighbor.TileType == lumenstone) {
						WorldGen.SpreadGrass(x, y, lumenstone, Type, false, neighbor.BlockColorAndCoating());
					}
				}
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

		private static void TryGrow(int x, int y, int type, int style) {
			if (!WorldGen.InWorld(x, y, 2)) {
				return;
			}

			if (WorldGen.PlaceTile(x, y, type, mute: true, style: style) && Main.netMode == NetmodeID.Server) {
				NetMessage.SendTileSquare(-1, x, y);
			}
		}
	}
}
