using Primordium.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Accessories
{
	public class JellyheartPendant : ModItem
	{
		// Vanilla breath is 200; this gives 75% more time underwater.
		private const int ExtraBreath = 150;

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.breathMax += ExtraBreath;
			if (!Main.dedServ) {
				Lighting.AddLight(player.Center, 0.22f, 0.55f, 0.6f);
			}
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<LuminousGel>(6)
				.AddIngredient<LumenShard>(4)
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
		}
	}
}
