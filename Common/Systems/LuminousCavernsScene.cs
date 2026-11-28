using Primordium.Content.Biomes;
using Terraria;
using Terraria.ModLoader;

namespace Primordium.Common.Systems
{
	public class LuminousCavernsScene : ModSystem
	{
		// Mild on purpose: plants, shards, and pools still have to read against the dark stone.
		private const float BiomeBrightness = 0.9f;
		private const float FadeStep = 0.01f;

		private float strength;

		public override void ModifyLightingBrightness(ref float scale) {
			if (Main.dedServ || Main.gameMenu) {
				strength = 0f;
				return;
			}

			bool inBiome = Main.LocalPlayer.InModBiome<LuminousCavernsBiome>();
			strength = inBiome ? System.Math.Min(1f, strength + FadeStep) : System.Math.Max(0f, strength - FadeStep);
			scale *= 1f - (1f - BiomeBrightness) * strength;
		}
	}
}
