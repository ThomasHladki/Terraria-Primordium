using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Projectiles
{
	public class CyberZombieLaser : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 4;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 90;
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.localAI[0] > 0f || Main.dedServ) {
				return;
			}

			Projectile.localAI[0] = 1f;
			SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.45f, Pitch = 0.15f }, Projectile.Center);
			for (int i = 0; i < 4; i++) {
				Dust spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: new Color(80, 230, 255));
				spark.noGravity = true;
				spark.scale = 0.7f;
				spark.velocity = Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.6f, 0.6f);
			}
		}
	}
}
