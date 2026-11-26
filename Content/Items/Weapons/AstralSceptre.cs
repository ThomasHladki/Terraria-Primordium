using Microsoft.Xna.Framework;
using Primordium.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Weapons
{
	public class AstralSceptre : ModItem
	{
		private const float TargetRange = 700f;

		public override void SetStaticDefaults() {
			Item.staff[Type] = true;
		}

		public override void SetDefaults() {
			Item.DefaultToStaff(ModContent.ProjectileType<AstralSceptreProjectile>(), 12f, 46, 24);
			Item.damage = 75;
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(gold: 5);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int target = FindTarget(Main.MouseWorld, TargetRange) + 1;

			for (int i = 0; i < 3; i++) {
				float angle = MathHelper.TwoPi * i / 3f;
				int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, target, angle);
				if (proj < 0 || proj >= Main.maxProjectiles) {
					continue;
				}

				Projectile spawned = Main.projectile[proj];
				if (spawned.ModProjectile is AstralSceptreProjectile mote) {
					mote.Initialize(Main.MouseWorld, velocity);
				}

				spawned.netUpdate = true;
			}

			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.ChlorophyteBar, 12)
				.AddIngredient(ItemID.Ectoplasm, 5)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

		private static int FindTarget(Vector2 aim, float maxDistance) {
			int found = -1;
			float closest = maxDistance;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy()) {
					continue;
				}

				float distance = Vector2.Distance(aim, npc.Center);
				if (distance < closest) {
					closest = distance;
					found = i;
				}
			}

			return found;
		}
	}
}
