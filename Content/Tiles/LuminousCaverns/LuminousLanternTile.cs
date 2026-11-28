using Microsoft.Xna.Framework;
using Primordium.Content.Items.Placeable;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class LuminousLanternTile : ModTile
	{
		private const int FrameWidth = 18;

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			AdjTiles = new int[] { TileID.HangingLanterns };

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<LuminousLantern>());
			DustType = DustID.BlueTorch;
			AddMapEntry(new Color(120, 230, 240), Language.GetText("MapObject.Lantern"));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameX < FrameWidth) {
				r = 0.3f;
				g = 0.8f;
				b = 0.95f;
			}
		}

		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			int top = j - tile.TileFrameY / 18 % 2;
			short shift = (short)(tile.TileFrameX < FrameWidth ? FrameWidth : -FrameWidth);
			for (int y = top; y < top + 2; y++) {
				Main.tile[i, y].TileFrameX += shift;
				if (Wiring.running) {
					Wiring.SkipWire(i, y);
				}
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, i, top, 1, 2);
			}
		}
	}
}
