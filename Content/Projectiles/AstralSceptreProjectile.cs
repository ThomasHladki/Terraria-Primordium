using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Primordium.Content.Globals;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.Projectiles
{
	public class AstralSceptreProjectile : ModProjectile
	{
		private const int OrbitTicks = 50;
		private const float OrbitRadius = 96f;
		private const float SpinSpeed = 0.13f;
		private const float StrikeSpeed = 12f;
		private const float RetargetRange = 700f;

		private Vector2 originalVelocity;
		private Vector2 orbitCenter;
		private bool hasCursor;

		private bool Orbiting => Projectile.localAI[0] <= OrbitTicks;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 8;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = OrbitTicks + 160;
		}

		public override void OnSpawn(IEntitySource source) {
			originalVelocity = Projectile.velocity;
		}

		public void Initialize(Vector2 cursor, Vector2 shotVelocity) {
			originalVelocity = shotVelocity;
			orbitCenter = cursor;
			hasCursor = true;
			SnapToOrbit();
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(originalVelocity.X);
			writer.Write(originalVelocity.Y);
			writer.Write(orbitCenter.X);
			writer.Write(orbitCenter.Y);
			writer.Write(hasCursor);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			originalVelocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			orbitCenter = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			hasCursor = reader.ReadBoolean();
		}

		public override bool? CanHitNPC(NPC target) {
			if (Orbiting) {
				return false;
			}

			return null;
		}

		public override void AI() {
			Projectile.localAI[0]++;
			Lighting.AddLight(Projectile.Center, 0.55f, 0.15f, 0.9f);
			SpawnTrailDust();

			NPC target = ResolveTarget();
			if (Orbiting) {
				Projectile.ai[1] += SpinSpeed;
				Projectile.Center = OrbitAnchor(target) + Projectile.ai[1].ToRotationVector2() * OrbitRadius;
				Projectile.velocity = Vector2.Zero;
				Projectile.rotation = Projectile.ai[1];
				return;
			}

			if (target != null) {
				Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * StrikeSpeed;
				if (Projectile.velocity.LengthSquared() < 16f) {
					Projectile.velocity = desired * 0.45f;
				}
				else {
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.22f);
				}
			}
			else {
				Projectile.velocity = originalVelocity.SafeNormalize(Vector2.UnitX) * StrikeSpeed;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.TryGetGlobalNPC(out AstralSceptreGlobalNPC mark)) {
				mark.ApplyAstralHit(ref modifiers);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

			for (int i = 0; i < 16; i++) {
				Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
				int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.Shadowflame;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, speed, 100, default, 1.45f);
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;
			int length = Projectile.oldPos.Length;

			for (int i = length - 1; i > 0; i--) {
				if (Projectile.oldPos[i] == Vector2.Zero) {
					continue;
				}

				float fade = (length - i) / (float)length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Color color = new Color(170, 80, 255) * fade * 0.45f;
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale * (0.7f + 0.3f * fade), SpriteEffects.None, 0);
			}

			return true;
		}

		private void SnapToOrbit() {
			NPC target = ResolveTarget();
			Projectile.Center = OrbitAnchor(target) + Projectile.ai[1].ToRotationVector2() * OrbitRadius;
			Projectile.velocity = Vector2.Zero;
			Projectile.rotation = Projectile.ai[1];
		}

		private Vector2 OrbitAnchor(NPC target) {
			if (target != null) {
				return target.Center;
			}

			if (hasCursor) {
				return orbitCenter;
			}

			return Projectile.Center + originalVelocity.SafeNormalize(Vector2.UnitX) * 160f;
		}

		private NPC ResolveTarget() {
			int index = (int)Projectile.ai[0] - 1;
			if (IsValidTarget(index)) {
				return Main.npc[index];
			}

			int next = FindClosest(Projectile.Center, RetargetRange);
			int encoded = next + 1;
			if ((int)Projectile.ai[0] != encoded) {
				Projectile.ai[0] = encoded;
				Projectile.netUpdate = true;
			}

			return next >= 0 ? Main.npc[next] : null;
		}

		private static int FindClosest(Vector2 origin, float maxDistance) {
			int found = -1;
			float closest = maxDistance;

			for (int i = 0; i < Main.maxNPCs; i++) {
				if (!IsValidTarget(i)) {
					continue;
				}

				float distance = Vector2.Distance(origin, Main.npc[i].Center);
				if (distance < closest) {
					closest = distance;
					found = i;
				}
			}

			return found;
		}

		private static bool IsValidTarget(int index) {
			if (index < 0 || index >= Main.maxNPCs) {
				return false;
			}

			NPC npc = Main.npc[index];
			return npc.active && npc.CanBeChasedBy();
		}

		private void SpawnTrailDust() {
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(0.6f, 0.6f), 80, default, 1.25f);
				dust.noGravity = true;
			}

			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, Projectile.velocity * 0.05f, 100, default, 1.05f);
				dust.noGravity = true;
			}
		}
	}
}
