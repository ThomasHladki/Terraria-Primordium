using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Projectiles
{
	public class CyberZombieLaser : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 14;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 90;
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Main.dedServ) {
				return;
			}

			Lighting.AddLight(Projectile.Center, 0.95f, 0.08f, 0.05f);
			if (Projectile.localAI[0] > 0f) {
				return;
			}

			Projectile.localAI[0] = 1f;
			// Eyezor's eye laser plays this zap when the beam is fired.
			SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
			Vector2 tip = Projectile.Center + Vector2.Normalize(Projectile.velocity) * 32f;
			for (int i = 0; i < 3; i++) {
				Dust spark = Dust.NewDustDirect(tip, 0, 0, DustID.RedTorch, newColor: new Color(255, 70, 50));
				spark.noGravity = true;
				spark.scale = 1.1f;
				spark.velocity = Projectile.velocity * 0.05f;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;
			Vector2 position = Projectile.Center - Main.screenPosition;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(texture, position, null, new Color(255, 36, 24), Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(texture, position, null, new Color(255, 210, 196), Projectile.rotation, origin, new Vector2(0.35f, 1f), SpriteEffects.None, 0f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
