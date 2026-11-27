using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Primordium.Common.Utilities
{
	/// <summary>
	/// Shared multiplayer-safe combat and targeting helpers for weapons, projectiles, and NPCs.
	/// </summary>
	public static class CombatUtilities
	{
		public static int FindClosestChasableNPC(Vector2 origin, float maxDistance, bool requireLineOfSight = false) {
			int bestIndex = -1;
			float bestDistSq = maxDistance * maxDistance;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.CanBeChasedBy()) {
					continue;
				}

				float distSq = Vector2.DistanceSquared(origin, npc.Center);
				if (distSq >= bestDistSq) {
					continue;
				}

				if (requireLineOfSight && !Collision.CanHitLine(origin, 1, 1, npc.position, npc.width, npc.height)) {
					continue;
				}

				bestDistSq = distSq;
				bestIndex = i;
			}

			return bestIndex;
		}

		public static bool CanPullNPC(NPC npc) {
			if (!npc.active || npc.boss || npc.knockBackResist <= 0f) {
				return false;
			}

			if (npc.realLife >= 0 && npc.realLife != npc.whoAmI) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Spawns a player-owned projectile only on the owning client (or single-player), preventing duplicate spawns.
		/// </summary>
		public static int SpawnOwnedProjectile(
			Player owner,
			IEntitySource source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback,
			float ai0 = 0f,
			float ai1 = 0f,
			float ai2 = 0f) {
			if (Main.myPlayer != owner.whoAmI) {
				return -1;
			}

			return Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner.whoAmI, ai0, ai1, ai2);
		}

		/// <summary>
		/// Spawns an authoritative world/NPC projectile only on server or single-player.
		/// </summary>
		public static int SpawnServerProjectile(
			IEntitySource source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback,
			float ai0 = 0f,
			float ai1 = 0f,
			float ai2 = 0f) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return -1;
			}

			return Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, ai0, ai1, ai2);
		}
	}
}
