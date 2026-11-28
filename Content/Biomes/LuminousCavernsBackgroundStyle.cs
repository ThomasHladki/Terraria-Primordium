using Terraria.ModLoader;

namespace Primordium.Content.Biomes
{
	public class LuminousCavernsBackgroundStyle : ModUndergroundBackgroundStyle
	{
		public override void FillTextureArray(int[] textureSlots) {
			for (int i = 0; i < 4; i++) {
				textureSlots[i] = BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Assets/Backgrounds/LuminousCavernsUnderground{i}");
			}
		}
	}
}
