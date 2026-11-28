using Microsoft.Xna.Framework;
using Primordium.Common.Systems;
using Primordium.Content.Items.Placeable;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Biomes
{
	public class LuminousCavernsBiome : ModBiome
	{
		// Lumenstone and moss blocks within the player's scan area. A biome is a few thousand blocks, so this
		// only trips once the player is properly inside, not when passing an edge.
		public const int TileThreshold = 300;

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

		public override int Music => MusicID.Underground;

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<LuminousCavernsBackgroundStyle>();

		public override int BiomeTorchItemType => ModContent.ItemType<LumenTorch>();

		public override string BestiaryIcon => "Primordium/Assets/Biomes/LuminousCavernsBiome_Icon";

		public override string BackgroundPath => "Primordium/Assets/Biomes/LuminousCavernsBiome_Background";

		public override string MapBackground => BackgroundPath;

		public override Color? BackgroundColor => new Color(18, 40, 62);

		public override bool IsBiomeActive(Player player) {
			return (player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight)
				&& ModContent.GetInstance<LuminousCavernsTileCount>().LuminousTileCount >= TileThreshold;
		}
	}
}
