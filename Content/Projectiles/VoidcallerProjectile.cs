using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Projectiles
{
	/// <summary>
	/// Slow orb that draws nearby foes inward, then bursts once.
	/// Bosses and knockback-immune enemies only drift. Worm segments are left alone so the head stays in charge.
	/// </summary>
	public class VoidcallerProjectile : ModProjectile
	{
		private const int TravelTime = 84;
		private const int ExplodeTime = 8;
		private const int BurstSize = 112;
		private const float PullRadius = 140f;
		private const float PullAccel = 0.36f;
		private const float HeavyPullAccel = 0.02f;
		private const float MaxPullSpeed = 6f;
		private const float HeavyPullSpeed = 0.7f;

		private bool Exploding => Projectile.ai[0] >= 1f;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TravelTime + ExplodeTime;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override bool? CanHitNPC(NPC target) {
			if (!Exploding) {
				return false;
			}

			if (!Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1)) {
				return false;
			}

			return null;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 step = oldVelocity.SafeNormalize(Vector2.Zero);
			if (step != Vector2.Zero) {
				Projectile.position -= step * 8f;
			}

			BeginExplosion();
			return false;
		}

		public override void AI() {
			if (!Exploding && Projectile.timeLeft <= ExplodeTime) {
				BeginExplosion();
			}

			if (Exploding) {
				if (Projectile.width != BurstSize) {
					Vector2 center = Projectile.Center;
					Projectile.width = BurstSize;
					Projectile.height = BurstSize;
					Projectile.position = center - Projectile.Size / 2f;
				}

				Projectile.velocity = Vector2.Zero;
				Projectile.tileCollide = false;
				Lighting.AddLight(Projectile.Center, 0.7f, 0.15f, 0.95f);
				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(28f, 28f), DustID.Shadowflame, Main.rand.NextVector2Circular(1.2f, 1.2f), 100, default, 1.25f);
					dust.noGravity = true;
				}

				return;
			}

			Lighting.AddLight(Projectile.Center, 0.4f, 0.05f, 0.65f);
			Projectile.rotation += 0.12f;
			PullNearby();
			SpawnTravelDust();
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			float scale = Exploding && Projectile.timeLeft <= 4 ? 2f : 1f;
			Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
			return false;
		}

		private void BeginExplosion() {
			if (Exploding) {
				return;
			}

			Projectile.ai[0] = 1f;
			Projectile.timeLeft = ExplodeTime;
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;
			Projectile.position = Projectile.Center - new Vector2(BurstSize / 2f);
			Projectile.width = BurstSize;
			Projectile.height = BurstSize;
			Projectile.netUpdate = true;

			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

			for (int i = 0; i < 30; i++) {
				Vector2 speed = Main.rand.NextVector2Circular(5.2f, 5.2f);
				int dustType = i % 3 == 0 ? DustID.Shadowflame : DustID.PurpleTorch;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, speed, 80, default, 1.55f);
				dust.noGravity = true;
			}

			for (int i = 0; i < 8; i++) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Main.rand.NextVector2Circular(1.6f, 1.6f), 140, new Color(70, 30, 110), 1.35f);
				dust.noGravity = true;
			}
		}

		private void PullNearby() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			bool sync = Projectile.timeLeft % 12 == 0;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy(Projectile)) {
					continue;
				}

				if (npc.realLife >= 0 && npc.realLife != npc.whoAmI) {
					continue;
				}

				if (!Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1)) {
					continue;
				}

				Vector2 toOrb = Projectile.Center - npc.Center;
				float distance = toOrb.Length();
				if (distance < 10f || distance > PullRadius) {
					continue;
				}

				Vector2 direction = toOrb / distance;
				bool heavy = npc.boss || npc.knockBackResist <= 0f;
				float accel = heavy ? HeavyPullAccel : PullAccel * MathHelper.Clamp(npc.knockBackResist, 0.4f, 1f);
				float cap = heavy ? HeavyPullSpeed : MaxPullSpeed;
				float falloff = 0.55f + 0.45f * (1f - distance / PullRadius);
				npc.velocity += direction * accel * falloff;

				float toward = Vector2.Dot(npc.velocity, direction);
				if (toward > cap) {
					npc.velocity -= direction * (toward - cap);
				}

				if (sync) {
					npc.netUpdate = true;
				}
			}
		}

		private void SpawnTravelDust() {
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(0.45f, 0.45f), 90, default, 1.2f);
				dust.noGravity = true;
			}

			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, -Projectile.velocity * 0.12f, 110, default, 1.05f);
				dust.noGravity = true;
			}
		}
	}
}
