using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Content.Projectiles;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Primordium.Content.NPCs
{
	public class CyberZombie : ModNPC
	{
		private const int LaserInterval = 600;
		private const float LaserRange = 20f * 16f;
		private const float LaserSpeed = 6f;
		private const int ClassicContactDamage = 14;
		private const int ClassicLaserDamage = 8;

		private int laserTimer;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;

			NPCID.Sets.NPCBestiaryDrawModifiers draw = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, draw);
		}

		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = ClassicContactDamage;
			NPC.defense = 6;
			NPC.lifeMax = 113;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.Zombie;
			laserTimer = 0;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax * balance);
		}

		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("Mods.Primordium.NPCs.CyberZombie.Bestiary")
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (Main.hardMode) {
				return 0f;
			}

			// About one sighting on a quiet night, and one or two on a night spent fighting.
			return SpawnCondition.OverworldNightMonster.Chance * 0.05f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// false keeps the shared global drops from being added a second time.
			foreach (IItemDropRule rule in Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false)) {
				npcLoot.Add(rule);
			}

			npcLoot.Add(ItemDropRule.Common(ItemID.IronBar, 3, 1, 3));
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			if (laserTimer < LaserInterval) {
				laserTimer++;
				return;
			}

			if (!NPC.HasValidTarget) {
				return;
			}

			Player target = Main.player[NPC.target];
			Vector2 eye = NPC.Center + new Vector2(NPC.direction * 4f, -10f);
			if (target.Distance(eye) > LaserRange || !Collision.CanHit(eye, 1, 1, target.position, target.width, target.height)) {
				return;
			}

			Vector2 aim = target.Center - eye;
			if (aim.LengthSquared() < 1f) {
				return;
			}

			int damage = System.Math.Max(1, NPC.damage * ClassicLaserDamage / ClassicContactDamage);
			Projectile.NewProjectile(
				NPC.GetSource_FromAI(),
				eye,
				Vector2.Normalize(aim) * LaserSpeed,
				ModContent.ProjectileType<CyberZombieLaser>(),
				damage,
				1f,
				Main.myPlayer);
			laserTimer = 0;
		}
	}
}
