using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class Lumenblossom : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<LuminousMoss>() };
			TileObjectData.newTile.CoordinateHeights = new[] { 20 };
			TileObjectData.newTile.DrawYOffset = -2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);

			DustType = DustID.BlueMoss;
			HitSound = SoundID.Grass;
			AddMapEntry(new Color(120, 220, 230), CreateMapEntryName());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.1f;
			g = 0.3f;
			b = 0.38f;
		}

		public override void SetSpriteEffects(int i, int j, ref Microsoft.Xna.Framework.Graphics.SpriteEffects spriteEffects) {
			if (i % 2 == 1) {
				spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
			}
		}
	}
}
