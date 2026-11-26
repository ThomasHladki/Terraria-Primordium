using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Content.Projectiles;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Primordium.Content.NPCs
{
	public class CyberZombie : ModNPC
	{
		private const int LaserInterval = 300;
		private const float LaserRange = 20f * 16f;
		private const float LaserSpeed = 6f;
		private const int ClassicContactDamage = 14;
		private const int ClassicLaserDamage = 8;

		public static readonly SoundStyle Groan = new SoundStyle("Primordium/Sounds/CyberZombieGroan") {
			Volume = 0.8f,
			PitchVariance = 0.06f,
			MaxInstances = 2,
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
		};

		private int _laserTimer;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

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
			AnimationType = NPCID.Zombie;
			_laserTimer = 0;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax * balance);
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
			if (!Main.dedServ && Main.rand.NextBool(420)) {
				SoundEngine.PlaySound(Groan, NPC.Center);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			if (_laserTimer < LaserInterval) {
				_laserTimer++;
				return;
			}

			if (!NPC.HasValidTarget) {
				return;
			}

			Player target = Main.player[NPC.target];
			Vector2 eye = NPC.Center + new Vector2(NPC.direction * 2f, -9f);
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
			_laserTimer = 0;
		}
	}

	// Zombie AI is run with this NPC's type swapped to a zombie, and that path plays SoundID.ZombieMoan
	// without looking at soundDelay. Cancel only the moan that comes from this NPC.
	public class CyberZombieGroanSilence : ModSystem
	{
		public override void Load() {
			On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += SilenceBorrowedZombieMoan;
		}

		public override void Unload() {
			On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback -= SilenceBorrowedZombieMoan;
		}

		private static SlotId SilenceBorrowedZombieMoan(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
			if (position.HasValue && style.SoundPath.StartsWith("Terraria/Sounds/Zombie_") && GroanComesFromCyberZombie(position.Value)) {
				return SlotId.Invalid;
			}

			return orig(ref style, position, updateCallback);
		}

		private static bool GroanComesFromCyberZombie(Vector2 soundPosition) {
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.ModNPC is not CyberZombie) {
					continue;
				}

				if (Vector2.DistanceSquared(npc.position, soundPosition) < 16f || Vector2.DistanceSquared(npc.Center, soundPosition) < 16f) {
					return true;
				}
			}

			return false;
		}
	}
}
