using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Walls
{
	public class LumenstoneWall : ModWall
	{
		public override void SetStaticDefaults() {
			Main.wallHouse[Type] = true;
			DustType = DustID.Granite;
			AddMapEntry(new Color(18, 27, 38));
		}
	}

	// Generated in the biome. Not a house wall, so spawns still happen against it.
	public class LumenstoneWallUnsafe : ModWall
	{
		public override string Texture => "Primordium/Content/Walls/LumenstoneWall";

		public override void SetStaticDefaults() {
			DustType = DustID.Granite;
			RegisterItemDrop(ModContent.ItemType<Items.Placeable.LumenstoneWall>());
			AddMapEntry(new Color(18, 27, 38));
		}
	}
}
