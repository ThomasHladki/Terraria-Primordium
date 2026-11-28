using Microsoft.Xna.Framework;
using Primordium.Content.Items.Materials;
using Primordium.Content.Tiles.LuminousCaverns;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Placeable
{
	public class LumenTorch : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
			ItemID.Sets.SingleUseInGamepad[Type] = true;
			ItemID.Sets.Torches[Type] = true;
			ItemID.Sets.WaterTorches[Type] = true;
		}

		public override void SetDefaults() {
			Item.DefaultToTorch(ModContent.TileType<LumenTorchTile>(), 0, true);
			// No held flame texture; the held glow comes from HoldItem instead.
			Item.flame = false;
			Item.value = 50;
		}

		public override void HoldItem(Player player) {
			if (Main.dedServ) {
				return;
			}

			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
			Lighting.AddLight(position, 0.35f, 0.9f, 1.05f);

			if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30)) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustID.IceTorch, 0f, 0f, 100);
				dust.noGravity = !Main.rand.NextBool(3);
				dust.velocity *= 0.3f;
				dust.velocity.Y -= 1.5f;
				dust.position = player.RotatedRelativePoint(dust.position);
			}
		}

		public override void PostUpdate() {
			if (!Main.dedServ) {
				Lighting.AddLight(Item.Center, 0.35f, 0.9f, 1.05f);
			}
		}

		public override void AddRecipes() {
			CreateRecipe(8)
				.AddIngredient<LuminousGel>()
				.AddIngredient<LumenShard>()
				.Register();
		}
	}
}
