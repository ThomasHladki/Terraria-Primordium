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
			r = 0.2f;
			g = 0.62f;
			b = 0.78f;
		}
	}
}
