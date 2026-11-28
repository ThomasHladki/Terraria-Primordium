using System.Collections.Generic;
using Primordium.Content.Biomes;
using Terraria;
using Terraria.ModLoader;

namespace Primordium.Common.Globals
{
	public class LuminousCavernsSpawnPool : GlobalNPC
	{
		// Pool key 0 is the whole vanilla spawn table for the spot. Modded entries are left alone.
		private const float VanillaPoolScale = 0.25f;
		private const float SpawnRateScale = 1.5f;

		private static bool Suppressed(Player player) {
			return player.InModBiome<LuminousCavernsBiome>() && !player.ZoneTowerSolar && !player.ZoneTowerVortex
				&& !player.ZoneTowerNebula && !player.ZoneTowerStardust && Main.invasionType == 0 && !Main.bloodMoon
				&& !Main.eclipse && !Main.pumpkinMoon && !Main.snowMoon;
		}

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Invasion || !Suppressed(spawnInfo.Player) || !pool.TryGetValue(0, out float vanilla)) {
				return;
			}

			pool[0] = vanilla * VanillaPoolScale;
		}

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			if (!Suppressed(player)) {
				return;
			}

			// A larger spawnRate means a longer wait between spawns.
			spawnRate = (int)(spawnRate * SpawnRateScale);
		}
	}
}
