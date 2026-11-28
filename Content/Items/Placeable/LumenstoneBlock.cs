using Primordium.Content.Tiles.LuminousCaverns;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Placeable
{
	public class LumenstoneBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Lumenstone>());
			Item.width = 16;
			Item.height = 16;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<LumenstoneWall>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
