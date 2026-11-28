using Primordium.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Weapons
{
	public class GloomlightStaff : ModItem
	{
		public override void SetStaticDefaults() {
			Item.staff[Type] = true;
		}

		public override void SetDefaults() {
			// Sits beside Vilethorn and Aqua Scepter: slower, but the pulse rewards grouped enemies.
			Item.DefaultToStaff(ModContent.ProjectileType<GloomlightGlobule>(), 6.5f, 30, 8);
			Item.damage = 21;
			Item.knockBack = 3f;
			Item.UseSound = SoundID.Item93;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 70);
		}
	}
}
