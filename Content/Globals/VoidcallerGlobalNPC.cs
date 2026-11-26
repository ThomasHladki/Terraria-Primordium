using Microsoft.Xna.Framework;
using Primordium.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Globals
{
	/// <summary>
	/// Pulls foes toward a flying Voidcaller orb after their own AI has chosen a velocity.
	/// </summary>
	public class VoidcallerGlobalNPC : GlobalNPC
	{
		public override void PostAI(NPC npc) {
			if (!npc.active || !npc.CanBeChasedBy() || !VoidcallerProjectile.ReceivesPull(npc)) {
				return;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			int type = ModContent.ProjectileType<VoidcallerProjectile>();
			Vector2 pull = Vector2.Zero;
			float strongest = 0f;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile orb = Main.projectile[i];
				if (!orb.active || orb.type != type || orb.ai[0] >= 1f) {
					continue;
				}

				if (!Collision.CanHitLine(orb.Center, 1, 1, npc.Center, 1, 1)) {
					continue;
				}

				Vector2 toOrb = orb.Center - npc.Center;
				float distance = toOrb.Length();
				if (distance < 12f || distance > VoidcallerProjectile.PullRadius) {
					continue;
				}

				float closeness = 1f - distance / VoidcallerProjectile.PullRadius;
				float strength = VoidcallerProjectile.PullStrength * (0.6f + 0.5f * closeness);
				if (distance < VoidcallerProjectile.CalmRadius) {
					strength *= distance / VoidcallerProjectile.CalmRadius;
				}

				if (strength > strongest) {
					strongest = strength;
					pull = toOrb / distance * strength;
				}
			}

			if (pull == Vector2.Zero) {
				return;
			}

			Vector2 direction = pull.SafeNormalize(Vector2.UnitX);
			npc.velocity += pull;
			float toward = Vector2.Dot(npc.velocity, direction);
			float cap = VoidcallerProjectile.MaxApproach;
			if (toward > cap) {
				npc.velocity -= direction * (toward - cap);
			}

			if (Main.GameUpdateCount % 10 == 0) {
				npc.netUpdate = true;
			}
		}
	}
}
