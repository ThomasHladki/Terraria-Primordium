using Microsoft.Xna.Framework;
using Primordium.Content.Biomes;
using Primordium.Content.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Primordium.Common.Players
{
	public class LuminousCavernsFishingPlayer : ModPlayer
	{
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			if (attempt.inLava || attempt.inHoney || attempt.crate || npcSpawn > 0 || itemDrop == attempt.questFish) {
				return;
			}

			if (!Player.InModBiome<LuminousCavernsBiome>()) {
				return;
			}

			// Common in the biome's pools, but leaves room for the normal cavern catches.
			if ((attempt.uncommon && Main.rand.NextBool(2)) || (attempt.common && Main.rand.NextBool(3))) {
				itemDrop = ModContent.ItemType<Glowfin>();
			}
		}
	}
}
