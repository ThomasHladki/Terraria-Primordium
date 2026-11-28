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
			// Grass selects the edge frame. tileMoss would force the host to be stone, so the host stays lumenstone.
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

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			// A side only grows moss when it faces air. Any solid block covers that side, the same as vanilla moss.
			Cover(i, j - 1, ref up);
			Cover(i, j + 1, ref down);
			Cover(i - 1, j, ref left);
			Cover(i + 1, j, ref right);
			Cover(i - 1, j - 1, ref upLeft);
			Cover(i + 1, j - 1, ref upRight);
			Cover(i - 1, j + 1, ref downLeft);
			Cover(i + 1, j + 1, ref downRight);
		}

		private static void Cover(int x, int y, ref int merge) {
			Tile tile = Framing.GetTileSafely(x, y);
			if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
				merge = ModContent.TileType<LuminousMoss>();
			}
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
			LumenShards.TryGrow(i, j);

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
