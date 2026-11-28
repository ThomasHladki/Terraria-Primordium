using Terraria;
using Terraria.ModLoader;

namespace Primordium.Content.Buffs
{
	public class LuminescenceBuff : ModBuff
	{
		public override void Update(Player player, ref int buffIndex) {
			if (Main.dedServ) {
				return;
			}

			// Gentle on land, brighter in water so murky pools stay readable.
			bool inWater = player.wet && !player.lavaWet && !player.honeyWet;
			float strength = inWater ? 1.1f : 0.55f;
			Lighting.AddLight(player.Center, 0.3f * strength, 0.75f * strength, 0.8f * strength);
			if (inWater) {
				Lighting.AddLight(player.Center + player.velocity * 6f + new Microsoft.Xna.Framework.Vector2(player.direction * 48f, 0f), 0.25f, 0.6f, 0.65f);
			}
		}
	}
}
