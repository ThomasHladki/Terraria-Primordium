using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Materials
{
	public class LuminousGel : ModItem
	{
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 14;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = 5;
			Item.rare = ItemRarityID.White;
			Item.ResearchUnlockCount = 99;
		}

		public override void PostUpdate() {
			if (Main.dedServ) {
				return;
			}

			Lighting.AddLight(Item.Center, 0.12f, 0.28f, 0.3f);
		}
	}
}
