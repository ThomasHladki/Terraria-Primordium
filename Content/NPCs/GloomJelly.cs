using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Content.Biomes;
using Primordium.Content.Items.Materials;
using Primordium.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Primordium.Content.NPCs
{
	public class GloomJelly : ModNPC
	{
		private const int PulseCycle = 480;
		// The flash is the warning before the shock. Long enough to read in a dark cavern.
		private const int FlashTicks = 90;
		private const float PulseRadius = 192f;
		private const float TelegraphRadius = 52f;
		private const float MaxDriftSpeed = 1.6f;
		private const float PulseKnockback = 5f;
		private const int ClassicContactDamage = 20;
		private const int ClassicPulseDamage = 12;
		private const float SpawnWeight = 0.08f;
		private const int StaffDropChance = 20;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
		}

		public override void SetDefaults() {
			NPC.width = 32;
			NPC.height = 36;
			NPC.damage = ClassicContactDamage;
			NPC.defense = 4;
			NPC.lifeMax = 48;
			NPC.HitSound = SoundID.NPCHit25;
			NPC.DeathSound = SoundID.NPCDeath28;
			// Blue Jellyfish uses 100, which drops about 1 silver.
			NPC.value = 100f;
			NPC.knockBackResist = 0.5f;
			NPC.noGravity = true;
			NPC.aiStyle = NPCAIStyleID.Flying;
			// Type is swapped to an Eater of Souls while vanilla AI runs, so the chase and wall bounce stay on.
			AIType = NPCID.EaterofSouls;
			SpawnModBiomes = new[] { ModContent.GetInstance<LuminousCavernsBiome>().Type };
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax * balance);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				new FlavorTextBestiaryInfoElement("Mods.Primordium.NPCs.GloomJelly.Bestiary")
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (Main.hardMode || spawnInfo.Water || spawnInfo.Invasion || !spawnInfo.Player.InModBiome<LuminousCavernsBiome>()) {
				return 0f;
			}

			// Weighed against the reduced cavern pool in LuminousCavernsSpawnPool: about one spawn in four.
			return SpawnWeight;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LuminousGel>(), 1, 1, 2));
			npcLoot.Add(ItemDropRule.ByCondition(Condition.DownedEowOrBoc.ToDropCondition(ShowItemDropInUI.Always), ModContent.ItemType<GloomlightStaff>(), StaffDropChance));
		}

		public override void FindFrame(int frameHeight) {
			NPC.frameCounter++;
			if (NPC.frameCounter < 8) {
				return;
			}

			NPC.frameCounter = 0;
			NPC.frame.Y += frameHeight;
			if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type]) {
				NPC.frame.Y = 0;
			}
		}

		public override void AI() {
			// Flying AI aims the top of the sprite along its velocity. The bell is the top of the frame, so it stays upright.
			NPC.rotation = 0f;

			float speed = NPC.velocity.Length();
			if (speed > MaxDriftSpeed) {
				NPC.velocity *= MaxDriftSpeed / speed;
			}

			NPC.spriteDirection = NPC.direction;

			// Eater of Souls flying AI writes ai[0]. Hornet and bee shots are the only writers of ai[1],
			// and those branches do not run for this AI type, so the pulse timer syncs with the NPC.
			float timer = NPC.ai[1];
			bool wasFlashing = timer >= PulseCycle - FlashTicks;
			timer += 1f;
			if (timer >= PulseCycle) {
				ReleasePulse();
				timer = 0f;
				NPC.netUpdate = true;
			}
			else if (!wasFlashing && timer >= PulseCycle - FlashTicks) {
				NPC.netUpdate = true;
			}

			NPC.ai[1] = timer;
			if (Main.dedServ) {
				return;
			}

			bool flashing = timer >= PulseCycle - FlashTicks;
			float boost = flashing ? 0.85f * (0.5f + 0.5f * (float)System.Math.Sin(timer * 0.45f)) : 0f;
			Lighting.AddLight(NPC.Center, 0.25f + boost, 0.7f + boost, 0.75f + boost);
			if (!flashing || (int)timer % 4 != 0) {
				return;
			}

			for (int i = 0; i < 4; i++) {
				Vector2 direction = (MathHelper.TwoPi * i / 4f + timer * 0.15f).ToRotationVector2();
				Dust spark = Dust.NewDustDirect(NPC.Center + direction * TelegraphRadius, 0, 0, DustID.Electric);
				spark.noGravity = true;
				spark.velocity = direction * 0.4f;
				spark.scale = 1.35f;
			}
		}

		private void ReleasePulse() {
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item94, NPC.Center);
				float ring = PulseRadius * 0.55f;
				for (int i = 0; i < 20; i++) {
					Vector2 direction = (MathHelper.TwoPi * i / 20f).ToRotationVector2();
					Dust spark = Dust.NewDustDirect(NPC.Center + direction * ring, 0, 0, DustID.Electric);
					spark.noGravity = true;
					spark.velocity = direction * 3.2f;
					spark.scale = 1.25f;
				}
			}

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			int damage = System.Math.Max(1, NPC.damage * ClassicPulseDamage / ClassicContactDamage);
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead || player.Distance(NPC.Center) > PulseRadius) {
					continue;
				}

				if (!Collision.CanHitLine(NPC.Center, 1, 1, player.position, player.width, player.height)) {
					continue;
				}

				int hitDirection = player.Center.X >= NPC.Center.X ? 1 : -1;
				player.Hurt(
					PlayerDeathReason.ByNPC(NPC.whoAmI),
					damage,
					hitDirection,
					pvp: false,
					quiet: false,
					cooldownCounter: -1,
					dodgeable: true,
					armorPenetration: 0f,
					scalingArmorPenetration: 0f,
					knockback: PulseKnockback);
			}
		}
	}
}
