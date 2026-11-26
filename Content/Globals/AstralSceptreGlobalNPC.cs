using Terraria;
using Terraria.ModLoader;

namespace Primordium.Content.Globals
{
	/// <summary>
	/// Per-enemy streak for the Astral Sceptre. The first hit in a window deals base damage.
	/// Later hits add 5% per existing stack, up to five stacks (+25%).
	/// </summary>
	public class AstralSceptreGlobalNPC : GlobalNPC
	{
		private const int WindowTicks = 180;
		private const int MaxStacks = 5;

		public override bool InstancePerEntity => true;

		private int stacks;
		private uint lastHitTick;
		private bool hasHit;

		public void ApplyAstralHit(ref NPC.HitModifiers modifiers) {
			uint now = Main.GameUpdateCount;
			bool consecutive = hasHit && now - lastHitTick <= WindowTicks;

			if (consecutive) {
				int applied = stacks > MaxStacks ? MaxStacks : stacks;
				modifiers.SourceDamage *= 1f + 0.05f * applied;
				if (stacks < MaxStacks) {
					stacks++;
				}
			}
			else {
				stacks = 1;
			}

			hasHit = true;
			lastHitTick = now;
		}
	}
}
