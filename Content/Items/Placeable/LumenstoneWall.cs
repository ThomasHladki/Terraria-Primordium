using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Placeable
{
	public class LumenstoneWall : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 400;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<Walls.LumenstoneWall>());
			Item.width = 24;
			Item.height = 24;
		}

		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<LumenstoneBlock>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
