using Primordium.Content.Items.Materials;
using Primordium.Content.Tiles.LuminousCaverns;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Placeable
{
	public class LuminousLantern : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<LuminousLanternTile>());
			Item.width = 12;
			Item.height = 28;
			Item.value = Item.sellPrice(silver: 3);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<LumenstoneBlock>(6)
				.AddIngredient<LumenShard>()
				.AddIngredient<LuminousGel>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
