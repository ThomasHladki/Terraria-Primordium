using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Items.Materials
{
	public class Glowfin : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 15);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
