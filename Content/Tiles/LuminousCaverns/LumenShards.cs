using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Content.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class LumenShards : ModTile
	{
		// Frame column 0 grows from the floor, column 1 hangs from the ceiling.
		public const int CeilingAlternate = 1;

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			int[] stone = { ModContent.TileType<Lumenstone>(), ModContent.TileType<LuminousMoss>() };

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorValidTiles = stone;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaDeath = false;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, 1, 0);
			TileObjectData.addAlternate(CeilingAlternate);
			TileObjectData.addTile(Type);

			DustType = DustID.BlueCrystalShard;
			HitSound = SoundID.Shatter;
			AddMapEntry(new Color(110, 235, 245), CreateMapEntryName());
		}

		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new Item(ModContent.ItemType<LumenShard>(), Main.rand.Next(1, 3));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// The half that touches the stone stays dark. Only the tip glows, and only a little.
			Tile above = Framing.GetTileSafely(i, j - 1);
			Tile below = Framing.GetTileSafely(i, j + 1);
			bool anchored = above.HasTile && IsHost(above.TileType) || below.HasTile && IsHost(below.TileType);
			if (anchored) {
				return;
			}

			r = 0.05f;
			g = 0.14f;
			b = 0.18f;
		}

		// Hallow crystals sprout from pearlstone a few at a time. These do the same on lumenstone and luminous moss.
		public static void TryGrow(int i, int j) {
			if (j <= Main.rockLayer && !Main.remixWorld) {
				return;
			}

			if (!WorldGen.genRand.NextBool(5)) {
				return;
			}

			int direction = WorldGen.genRand.Next(4);
			int x = i + (direction == 0 ? -1 : direction == 1 ? 1 : 0);
			int y = j + (direction == 2 ? -1 : direction == 3 ? 1 : 0);
			if (!WorldGen.InWorld(x, y, 10) || Main.tile[x, y].HasTile || Main.tile[x, y].LiquidAmount > 0) {
				return;
			}

			if (CountNearby(i, j) >= 2) {
				return;
			}

			if (direction == 2 && IsOpen(x, y - 1)) {
				Place(x, y, ceiling: false);
			}
			else if (direction == 3 && IsOpen(x, y + 1)) {
				Place(x, y, ceiling: true);
			}
		}

		private static void Place(int x, int y, bool ceiling) {
			int type = ModContent.TileType<LumenShards>();
			if (ceiling) {
				WorldGen.PlaceObject(x, y, type, mute: true, style: 0, alternate: CeilingAlternate);
			}
			else {
				WorldGen.PlaceObject(x, y, type, mute: true);
			}

			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendTileSquare(-1, x - 1, y - 1, 3, 3);
			}
		}

		private static int CountNearby(int i, int j) {
			int type = ModContent.TileType<LumenShards>();
			int count = 0;
			for (int x = i - 6; x <= i + 6; x++) {
				for (int y = j - 6; y <= j + 6; y++) {
					if (!WorldGen.InWorld(x, y, 1)) {
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (tile.HasTile && tile.TileType == type && tile.TileFrameY == 0) {
						count++;
					}
				}
			}

			return count;
		}

		private static bool IsOpen(int x, int y) {
			if (!WorldGen.InWorld(x, y, 1)) {
				return false;
			}

			Tile tile = Main.tile[x, y];
			return !tile.HasTile && tile.LiquidAmount == 0;
		}

		private static bool IsHost(int type) {
			return type == ModContent.TileType<Lumenstone>() || type == ModContent.TileType<LuminousMoss>();
		}
	}
}
