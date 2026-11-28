using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Projectiles
{
	public class GloomlightGlobule : ModProjectile
	{
		private const int GlobuleSize = 32;
		private const int PulseSize = 120;
		private const int PulseTicks = 6;
		private const float PulseDamageScale = 0.6f;

		// ai[0] is synced: 0 while flying, 1 once it has burst. localAI[0] marks the burst as applied locally.
		private bool Bursting => Projectile.ai[0] == 1f;

		public override void SetDefaults() {
			Projectile.width = GlobuleSize;
			Projectile.height = GlobuleSize;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.ignoreWater = true;
			// Each enemy is hit once in total, so the target struck by the globule is not hit again by its own burst.
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI() {
			if (Bursting) {
				if (Projectile.localAI[0] == 0f) {
					ApplyBurst();
				}

				return;
			}

			Projectile.rotation += 0.12f * Projectile.direction;
			if (!Main.dedServ) {
				Lighting.AddLight(Projectile.Center, 0.2f, 0.55f, 0.65f);
				if (Main.rand.NextBool(3)) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.7f);
					dust.noGravity = true;
					dust.velocity *= 0.3f;
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			StartBurst();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			StartBurst();
			return false;
		}

		public override bool PreDraw(ref Color lightColor) {
			return !Bursting;
		}

		public override Color? GetAlpha(Color lightColor) {
			return new Color(200, 255, 255, 120);
		}

		private void StartBurst() {
			if (Bursting) {
				return;
			}

			Projectile.ai[0] = 1f;
			Projectile.netUpdate = true;
		}

		private void ApplyBurst() {
			Projectile.localAI[0] = 1f;
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;
			Projectile.timeLeft = PulseTicks;
			Projectile.damage = (int)(Projectile.damage * PulseDamageScale);
			Projectile.Resize(PulseSize, PulseSize);

			if (Main.dedServ) {
				return;
			}

			SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
			Lighting.AddLight(Projectile.Center, 0.5f, 1.2f, 1.4f);
			for (int i = 0; i < 16; i++) {
				Vector2 direction = (MathHelper.TwoPi * i / 16f).ToRotationVector2();
				Dust spark = Dust.NewDustDirect(Projectile.Center + direction * 10f, 0, 0, DustID.Electric);
				spark.noGravity = true;
				spark.velocity = direction * 2.6f;
				spark.scale = 1.1f;
			}
		}
	}
}
