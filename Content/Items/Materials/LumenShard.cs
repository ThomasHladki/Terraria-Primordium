using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Materials
{
	public class LumenShard : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}

		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 40);
			Item.rare = ItemRarityID.Blue;
		}

		public override void PostUpdate() {
			if (!Main.dedServ) {
				Lighting.AddLight(Item.Center, 0.15f, 0.45f, 0.55f);
			}
		}
	}
}
