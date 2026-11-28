using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class Glowvine : ModTile
	{
		public const int MaxLength = 12;
		private const int GrowChance = 6;
		// Frame column 2 is the violet variant; the other two are cyan.
		private const int VioletColumn = 2;

		public override void SetStaticDefaults() {
			Main.tileCut[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;

			DustType = DustID.BlueMoss;
			HitSound = SoundID.Grass;
			AddMapEntry(new Color(70, 190, 200), CreateMapEntryName());
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			bool supported = above.HasTile && !above.IsActuated && !above.BottomSlope
				&& (above.TileType == Type || above.TileType == ModContent.TileType<LuminousMoss>());
			if (!supported) {
				WorldGen.KillTile(i, j);
				return false;
			}

			Tile tile = Main.tile[i, j];
			if (resetFrame) {
				tile.TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
			}

			Tile below = Framing.GetTileSafely(i, j + 1);
			tile.TileFrameY = (short)(below.HasTile && below.TileType == Type ? 0 : 18);
			return false;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameX / 18 == VioletColumn) {
				r = 0.32f;
				g = 0.14f;
				b = 0.48f;
			}
			else {
				r = 0.08f;
				g = 0.36f;
				b = 0.42f;
			}
		}

		public override void RandomUpdate(int i, int j) {
			Tile below = Framing.GetTileSafely(i, j + 1);
			if (below.HasTile || below.LiquidAmount > 0 || !WorldGen.genRand.NextBool(GrowChance)) {
				return;
			}

			int length = 1;
			while (length < MaxLength && Framing.GetTileSafely(i, j - length).TileType == Type && Framing.GetTileSafely(i, j - length).HasTile) {
				length++;
			}

			if (length >= MaxLength || !WorldGen.InWorld(i, j + 1, 2)) {
				return;
			}

			if (WorldGen.PlaceTile(i, j + 1, Type, mute: true) && Main.netMode == NetmodeID.Server) {
				NetMessage.SendTileSquare(-1, i, j + 1);
			}
		}
	}
}
