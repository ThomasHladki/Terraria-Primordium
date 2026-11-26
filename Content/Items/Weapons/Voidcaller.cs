using Primordium.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Weapons
{
	public class Voidcaller : ModItem
	{
		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 32;
			Item.damage = 62;
			Item.DamageType = DamageClass.Magic;
			Item.knockBack = 6f;
			Item.mana = 15;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<VoidcallerProjectile>();
			Item.shootSpeed = 4.5f;
			Item.UseSound = SoundID.Item8;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 4);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.FallenStar, 25)
				.AddIngredient(ItemID.SoulofNight, 15)
				.AddIngredient(ItemID.SpellTome)
				.AddIngredient(ItemID.DarkShard)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
