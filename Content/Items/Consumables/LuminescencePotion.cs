using Microsoft.Xna.Framework;
using Primordium.Content.Buffs;
using Primordium.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Consumables
{
	public class LuminescencePotion : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
			ItemID.Sets.DrinkParticleColors[Type] = new[] {
				new Color(70, 200, 210),
				new Color(30, 120, 150),
				new Color(130, 90, 190)
			};
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 30;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 2);
			Item.buffType = ModContent.BuffType<LuminescenceBuff>();
			Item.buffTime = 5 * 60 * 60;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.BottledWater)
				.AddIngredient<Glowfin>()
				.AddIngredient<LuminousGel>()
				.AddTile(TileID.Bottles)
				.Register();
		}
	}
}
