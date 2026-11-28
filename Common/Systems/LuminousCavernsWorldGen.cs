using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Common.Configs;
using Primordium.Content.Tiles.LuminousCaverns;
using Primordium.Content.Walls;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Primordium.Common.Systems
{
	public class LuminousCavernsWorldGen : ModSystem
	{
		private const int StructurePadding = 12;
		private const int PlacementAttempts = 500;
		private const int SpawnClearance = 140;
		private const int DungeonClearance = 220;
		private const int ShimmerClearance = 120;
		private const int BiomeEdgeClearance = 40;
		private const int EntranceReach = 30;
		private const double WallCoverage = -0.2;

		private static readonly List<BiomeArea> Areas = new();

		private sealed class BiomeArea
		{
			public Rectangle Bounds;
			public bool[,] Mask;
			public List<Point> Chambers = new();

			public bool Contains(int x, int y) {
				int lx = x - Bounds.X;
				int ly = y - Bounds.Y;
				return lx >= 0 && ly >= 0 && lx < Bounds.Width && ly < Bounds.Height && Mask[lx, ly];
			}

			public double Distance(int x, int y) {
				double dx = (x + 0.5 - Bounds.Center.X) / (Bounds.Width / 2.0);
				double dy = (y + 0.5 - Bounds.Center.Y) / (Bounds.Height / 2.0);
				return Math.Sqrt(dx * dx + dy * dy);
			}
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			// After Shimmer, so the Dungeon, the evil biome, and the Shimmer lake already exist and can be avoided.
			int terrainIndex = tasks.FindIndex(pass => pass.Name == "Shimmer");
			if (terrainIndex < 0) {
				terrainIndex = tasks.FindIndex(pass => pass.Name == "Mushroom Patches");
			}

			if (terrainIndex >= 0) {
				tasks.Insert(terrainIndex + 1, new PassLegacy("Primordium: Luminous Caverns", GenerateTerrain, 120));
			}

			int cleanupIndex = tasks.FindIndex(pass => pass.Name == "Final Cleanup");
			if (cleanupIndex >= 0) {
				tasks.Insert(cleanupIndex, new PassLegacy("Primordium: Luminous Caverns Growth", Decorate, 30));
			}
		}

		public override void PostWorldGen() {
			Areas.Clear();
		}

		private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = Language.GetTextValue("Mods.Primordium.WorldGen.LuminousCaverns");
			Areas.Clear();

			BiomeSizeSettings size = PickSize();
			int count = WorldGen.genRand.Next(Math.Min(size.MinCount, size.MaxCount), Math.Max(size.MinCount, size.MaxCount) + 1);
			for (int n = 0; n < count; n++) {
				progress.Set(n / (double)count);
				if (!TryFindSpot(size, out Rectangle bounds)) {
					Mod.Logger.Info($"Luminous Caverns: no valid spot for biome {n + 1} of {count}, skipped.");
					continue;
				}

				Areas.Add(Build(bounds));
				GenVars.structures.AddProtectedStructure(bounds, StructurePadding);
				Mod.Logger.Info($"Luminous Caverns: placed at {bounds.X},{bounds.Y} size {bounds.Width}x{bounds.Height}.");
			}
		}

		private static BiomeSizeSettings PickSize() {
			LuminousCavernsConfig config = ModContent.GetInstance<LuminousCavernsConfig>();
			if (Main.maxTilesX < 5000) {
				return config.Small;
			}

			return Main.maxTilesX < 7500 ? config.Medium : config.Large;
		}

		private static bool TryFindSpot(BiomeSizeSettings size, out Rectangle bounds) {
			bounds = default;
			int minWidth = Math.Min(size.MinWidth, size.MaxWidth);
			int maxWidth = Math.Max(size.MinWidth, size.MaxWidth);
			int minHeight = Math.Min(size.MinHeight, size.MaxHeight);
			int maxHeight = Math.Max(size.MinHeight, size.MaxHeight);
			int top = (int)Main.rockLayer + 15;
			int bottom = Main.UnderworldLayer - 50;
			int edge = (int)(Main.maxTilesX * 0.14);

			for (int attempt = 0; attempt < PlacementAttempts; attempt++) {
				int width = WorldGen.genRand.Next(minWidth, maxWidth + 1);
				int height = Math.Min(WorldGen.genRand.Next(minHeight, maxHeight + 1), bottom - top);
				if (height < 30 || Main.maxTilesX - edge - width <= edge) {
					return false;
				}

				int x = WorldGen.genRand.Next(edge, Main.maxTilesX - edge - width);
				int y = WorldGen.genRand.Next(top, bottom - height + 1);
				Rectangle candidate = new(x, y, width, height);
				if (IsValidSpot(candidate)) {
					bounds = candidate;
					return true;
				}
			}

			return false;
		}

		private static bool IsValidSpot(Rectangle area) {
			int centerX = area.Center.X;
			int centerY = area.Center.Y;
			int halfWidth = area.Width / 2;

			if (Math.Abs(centerX - Main.spawnTileX) < halfWidth + SpawnClearance) {
				return false;
			}

			(int jungleLeft, int jungleRight) = JungleSpan();
			if (area.Right > jungleLeft && area.Left < jungleRight) {
				return false;
			}

			if (GenVars.dungeonX > 0 && Math.Abs(centerX - GenVars.dungeonX) < halfWidth + DungeonClearance) {
				return false;
			}

			if (GenVars.shimmerPosition.X > 0
				&& Math.Abs(centerX - GenVars.shimmerPosition.X) < halfWidth + ShimmerClearance
				&& Math.Abs(centerY - GenVars.shimmerPosition.Y) < area.Height / 2 + ShimmerClearance) {
				return false;
			}

			Rectangle desert = GenVars.UndergroundDesertLocation;
			desert.Inflate(BiomeEdgeClearance, BiomeEdgeClearance);
			if (desert.Width > BiomeEdgeClearance * 2 && desert.Intersects(area)) {
				return false;
			}

			if (GenVars.snowOriginRight > GenVars.snowOriginLeft
				&& area.Right > GenVars.snowOriginLeft - BiomeEdgeClearance * 2
				&& area.Left < GenVars.snowOriginRight + BiomeEdgeClearance * 2) {
				return false;
			}

			foreach (BiomeArea other in Areas) {
				Rectangle spaced = other.Bounds;
				spaced.Inflate(BiomeEdgeClearance, BiomeEdgeClearance);
				if (spaced.Intersects(area)) {
					return false;
				}
			}

			return GenVars.structures.CanPlace(area, StructurePadding) && ScanTiles(area);
		}

		private static (int, int) JungleSpan() {
			int reach = (int)(Main.maxTilesX * 0.1);
			int left = GenVars.JungleX - reach;
			int right = GenVars.JungleX + reach;
			if (GenVars.jungleMinX > 0 && GenVars.jungleMaxX > GenVars.jungleMinX) {
				left = Math.Min(left, GenVars.jungleMinX - 60);
				right = Math.Max(right, GenVars.jungleMaxX + 60);
			}

			return (left, right);
		}

		// Structures, evil stone, and other biomes reject the spot outright. Mud, sand, ice, and mushroom
		// grass are allowed in small stray patches, which every cavern has.
		private static bool ScanTiles(Rectangle area) {
			int softLimit = area.Width * area.Height / 100;
			int soft = 0;
			int left = Math.Max(area.Left - 4, 10);
			int right = Math.Min(area.Right + 4, Main.maxTilesX - 10);
			int top = Math.Max(area.Top - 4, 10);
			int bottom = Math.Min(area.Bottom + 4, Main.maxTilesY - 10);

			for (int x = left; x < right; x++) {
				for (int y = top; y < bottom; y++) {
					Tile tile = Main.tile[x, y];
					if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Shimmer) {
						return false;
					}

					if (tile.WallType > 0 && (Main.wallDungeon[tile.WallType] || Main.wallHouse[tile.WallType])) {
						return false;
					}

					if (!tile.HasTile) {
						continue;
					}

					int type = tile.TileType;
					if (IsHardForbidden(type)) {
						return false;
					}

					if (IsSoftForbidden(type) && ++soft > softLimit) {
						return false;
					}
				}
			}

			return true;
		}

		private static bool IsHardForbidden(int type) {
			if (Main.tileDungeon[type] || TileID.Sets.BasicChest[type]) {
				return true;
			}

			if (type >= TileID.Count && Main.tileFrameImportant[type]) {
				return true;
			}

			return type is TileID.LihzahrdBrick or TileID.Ebonstone or TileID.Crimstone or TileID.CorruptGrass
				or TileID.CrimsonGrass or TileID.Ebonsand or TileID.Crimsand or TileID.Hive or TileID.ShimmerBlock
				or TileID.Demonite or TileID.Crimtane;
		}

		private static bool IsSoftForbidden(int type) {
			if (type >= TileID.Count) {
				return true;
			}

			return type is TileID.Mud or TileID.JungleGrass or TileID.Sand or TileID.HardenedSand or TileID.Sandstone
				or TileID.IceBlock or TileID.SnowBlock or TileID.MushroomGrass;
		}

		private static BiomeArea Build(Rectangle bounds) {
			BiomeArea area = new() { Bounds = bounds, Mask = BuildMask(bounds) };
			Convert(area);
			CarveChambers(area);
			ConnectChambers(area);
			CarveEntrances(area);
			FillPools(area);
			return area;
		}

		private static bool[,] BuildMask(Rectangle bounds) {
			bool[,] mask = new bool[bounds.Width, bounds.Height];
			double p1 = WorldGen.genRand.NextDouble() * MathHelper.TwoPi;
			double p2 = WorldGen.genRand.NextDouble() * MathHelper.TwoPi;
			double p3 = WorldGen.genRand.NextDouble() * MathHelper.TwoPi;

			for (int lx = 0; lx < bounds.Width; lx++) {
				for (int ly = 0; ly < bounds.Height; ly++) {
					double dx = (lx + 0.5 - bounds.Width / 2.0) / (bounds.Width / 2.0);
					double dy = (ly + 0.5 - bounds.Height / 2.0) / (bounds.Height / 2.0);
					double angle = Math.Atan2(dy, dx);
					double edge = 0.84 + 0.09 * Math.Sin(3 * angle + p1) + 0.05 * Math.Sin(5 * angle + p2) + 0.03 * Math.Sin(11 * angle + p3);
					mask[lx, ly] = Math.Sqrt(dx * dx + dy * dy) < edge;
				}
			}

			return mask;
		}

		private static void Convert(BiomeArea area) {
			ushort lumenstone = (ushort)ModContent.TileType<Lumenstone>();
			ushort wall = (ushort)ModContent.WallType<LumenstoneWallUnsafe>();
			double phaseX = WorldGen.genRand.NextDouble() * 100;
			double phaseY = WorldGen.genRand.NextDouble() * 100;

			for (int x = area.Bounds.Left; x < area.Bounds.Right; x++) {
				for (int y = area.Bounds.Top; y < area.Bounds.Bottom; y++) {
					if (!area.Contains(x, y)) {
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (tile.HasTile && tile.TileType is TileID.Stone or TileID.Dirt or TileID.ClayBlock or TileID.Silt) {
						tile.TileType = lumenstone;
					}

					if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava) {
						tile.LiquidType = LiquidID.Water;
					}

					// Patchy walls, so the biome's own background shows through in the open parts.
					double noise = Math.Sin(x * 0.11 + phaseX) + Math.Sin(y * 0.17 + phaseY) + Math.Sin((x + y) * 0.07);
					bool naturalWall = tile.WallType > 0 && !Main.wallHouse[tile.WallType] && !Main.wallDungeon[tile.WallType];
					if (naturalWall || (tile.WallType == 0 && noise > WallCoverage)) {
						tile.WallType = wall;
					}
				}
			}
		}

		private static void CarveChambers(BiomeArea area) {
			int target = 3 + area.Bounds.Width * area.Bounds.Height / 2200;
			int minSpacing = 14;

			for (int attempt = 0; attempt < target * 40 && area.Chambers.Count < target; attempt++) {
				int x = WorldGen.genRand.Next(area.Bounds.Left, area.Bounds.Right);
				int y = WorldGen.genRand.Next(area.Bounds.Top, area.Bounds.Bottom);
				if (area.Distance(x, y) > 0.62) {
					continue;
				}

				bool crowded = false;
				foreach (Point other in area.Chambers) {
					if (Math.Abs(other.X - x) < minSpacing && Math.Abs(other.Y - y) < minSpacing) {
						crowded = true;
						break;
					}
				}

				if (crowded) {
					continue;
				}

				area.Chambers.Add(new Point(x, y));
				double radiusX = WorldGen.genRand.Next(7, 15);
				double radiusY = WorldGen.genRand.Next(4, 8);
				CarveBlob(area, x, y, radiusX, radiusY, insideMaskOnly: true);

				int lobes = WorldGen.genRand.Next(1, 4);
				for (int k = 0; k < lobes; k++) {
					int ox = x + WorldGen.genRand.Next(-(int)radiusX, (int)radiusX + 1);
					int oy = y + WorldGen.genRand.Next(-(int)radiusY, (int)radiusY + 1);
					CarveBlob(area, ox, oy, radiusX * 0.55, radiusY * 0.7, insideMaskOnly: true);
				}
			}
		}

		private static void ConnectChambers(BiomeArea area) {
			area.Chambers.Sort((a, b) => a.X.CompareTo(b.X));
			for (int k = 0; k + 1 < area.Chambers.Count; k++) {
				CarveTunnel(area, area.Chambers[k].ToVector2(), area.Chambers[k + 1].ToVector2());
			}

			int extra = area.Chambers.Count / 3;
			for (int k = 0; k < extra; k++) {
				Point a = area.Chambers[WorldGen.genRand.Next(area.Chambers.Count)];
				Point b = area.Chambers[WorldGen.genRand.Next(area.Chambers.Count)];
				if (a != b) {
					CarveTunnel(area, a.ToVector2(), b.ToVector2());
				}
			}
		}

		private static void CarveTunnel(BiomeArea area, Vector2 from, Vector2 to) {
			// Mostly walkable tunnels, with the odd squeeze.
			float radius = WorldGen.genRand.NextBool(3) ? WorldGen.genRand.NextFloat(1.3f, 1.9f) : WorldGen.genRand.NextFloat(2.2f, 3.5f);
			Vector2 position = from;
			Vector2 heading = Vector2.Normalize(to - from + new Vector2(0.01f, 0f));

			for (int step = 0; step < 900 && Vector2.Distance(position, to) > 2f; step++) {
				Vector2 toward = Vector2.Normalize(to - position);
				heading = Vector2.Normalize(Vector2.Lerp(heading, toward, 0.2f).RotatedBy(WorldGen.genRand.NextFloat(-0.45f, 0.45f)));
				position += heading;
				float size = radius * WorldGen.genRand.NextFloat(0.85f, 1.15f);
				CarveBlob(area, (int)position.X, (int)position.Y, size, size, insideMaskOnly: true);
			}
		}

		private static void CarveEntrances(BiomeArea area) {
			if (area.Chambers.Count == 0) {
				return;
			}

			Rectangle reach = area.Bounds;
			reach.Inflate(StructurePadding - 2, StructurePadding - 2);
			int entrances = WorldGen.genRand.Next(2, 4);

			for (int k = 0; k < entrances; k++) {
				Point start = area.Chambers[WorldGen.genRand.Next(area.Chambers.Count)];
				// Spread the openings around the biome: left, right, then up or down.
				float angle = k switch {
					0 => MathHelper.Pi + WorldGen.genRand.NextFloat(-0.5f, 0.5f),
					1 => WorldGen.genRand.NextFloat(-0.5f, 0.5f),
					_ => (WorldGen.genRand.NextBool() ? -MathHelper.PiOver2 : MathHelper.PiOver2) + WorldGen.genRand.NextFloat(-0.6f, 0.6f)
				};

				Vector2 heading = angle.ToRotationVector2();
				Vector2 position = start.ToVector2();
				float radius = WorldGen.genRand.NextFloat(1.8f, 3f);
				int outside = 0;

				for (int step = 0; step < 400 && outside < EntranceReach; step++) {
					heading = Vector2.Normalize(heading.RotatedBy(WorldGen.genRand.NextFloat(-0.3f, 0.3f)) + angle.ToRotationVector2() * 0.3f);
					position += heading;
					int x = (int)position.X;
					int y = (int)position.Y;
					if (!reach.Contains(x, y)) {
						break;
					}

					bool inside = area.Contains(x, y);
					CarveBlob(area, x, y, radius, radius, insideMaskOnly: false, reach);
					if (inside) {
						continue;
					}

					outside++;
					Point ahead = (position + heading * (radius + 2f)).ToPoint();
					if (WorldGen.InWorld(ahead.X, ahead.Y, 20) && !Main.tile[ahead.X, ahead.Y].HasTile && !area.Contains(ahead.X, ahead.Y)) {
						break;
					}
				}
			}
		}

		private static void FillPools(BiomeArea area) {
			int[] depths = { 2, 3, 5, 8, 11 };
			foreach (Point chamber in area.Chambers) {
				if (WorldGen.genRand.NextFloat() > 0.65f) {
					continue;
				}

				int x = chamber.X;
				int y = chamber.Y;
				while (y < area.Bounds.Bottom && area.Contains(x, y) && !Main.tile[x, y].HasTile) {
					y++;
				}

				if (!area.Contains(x, y)) {
					continue;
				}

				int depth = depths[WorldGen.genRand.Next(depths.Length)];
				int halfWidth = WorldGen.genRand.Next(4, 13);
				for (int dx = -halfWidth; dx <= halfWidth; dx++) {
					double t = dx / (double)halfWidth;
					int columnDepth = (int)Math.Round(depth * Math.Sqrt(1 - t * t));
					for (int dy = 0; dy < columnDepth; dy++) {
						int px = x + dx;
						int py = y + dy;
						if (!area.Contains(px, py) || !area.Contains(px, py + 1)) {
							continue;
						}

						Tile tile = Main.tile[px, py];
						if (tile.HasTile && !CanCarve(tile.TileType)) {
							continue;
						}

						tile.HasTile = false;
						tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = byte.MaxValue;
					}
				}
			}
		}

		private static void CarveBlob(BiomeArea area, int centerX, int centerY, double radiusX, double radiusY, bool insideMaskOnly, Rectangle reach = default) {
			int spanX = (int)Math.Ceiling(radiusX) + 1;
			int spanY = (int)Math.Ceiling(radiusY) + 1;
			for (int x = centerX - spanX; x <= centerX + spanX; x++) {
				for (int y = centerY - spanY; y <= centerY + spanY; y++) {
					if (!WorldGen.InWorld(x, y, 20)) {
						continue;
					}

					double nx = (x - centerX) / radiusX;
					double ny = (y - centerY) / radiusY;
					if (nx * nx + ny * ny > 1 + WorldGen.genRand.NextDouble() * 0.2) {
						continue;
					}

					bool inside = area.Contains(x, y);
					if (insideMaskOnly ? !inside : !inside && !reach.Contains(x, y)) {
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (!tile.HasTile || !CanCarve(tile.TileType) || (!inside && tile.TileType >= TileID.Count)) {
						continue;
					}

					tile.HasTile = false;
					tile.Slope = Terraria.ID.SlopeType.Solid;
					tile.IsHalfBlock = false;
				}
			}
		}

		private static bool CanCarve(int type) {
			return !Main.tileFrameImportant[type] && !Main.tileDungeon[type] && !IsHardForbidden(type);
		}

		private void Decorate(GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = Language.GetTextValue("Mods.Primordium.WorldGen.LuminousCavernsGrowth");
			int lumenstone = ModContent.TileType<Lumenstone>();
			int moss = ModContent.TileType<LuminousMoss>();
			int vine = ModContent.TileType<Glowvine>();
			int blossom = ModContent.TileType<Lumenblossom>();
			int shards = ModContent.TileType<LumenShards>();

			for (int n = 0; n < Areas.Count; n++) {
				progress.Set(n / (double)Areas.Count);
				BiomeArea area = Areas[n];
				Rectangle b = area.Bounds;

				for (int x = b.Left; x < b.Right; x++) {
					for (int y = b.Top; y < b.Bottom; y++) {
						Tile tile = Main.tile[x, y];
						if (area.Contains(x, y) && tile.HasTile && tile.TileType == lumenstone && LuminousMoss.HasOpenFace(x, y) && WorldGen.genRand.NextFloat() < 0.75f) {
							tile.TileType = (ushort)moss;
						}
					}
				}

				for (int x = b.Left; x < b.Right; x++) {
					for (int y = b.Top; y < b.Bottom; y++) {
						if (!area.Contains(x, y) || !Main.tile[x, y].HasTile || Main.tile[x, y].TileType != moss) {
							continue;
						}

						if (IsOpenAir(x, y + 1) && WorldGen.genRand.NextBool(3)) {
							int length = WorldGen.genRand.Next(2, Glowvine.MaxLength - 1);
							for (int k = 1; k <= length && IsOpenAir(x, y + k); k++) {
								if (!WorldGen.PlaceTile(x, y + k, vine, mute: true)) {
									break;
								}
							}
						}

						if (IsOpenAir(x, y - 1) && WorldGen.genRand.NextBool(4)) {
							WorldGen.PlaceTile(x, y - 1, blossom, mute: true, style: WorldGen.genRand.Next(3));
						}
					}
				}

				for (int x = b.Left; x < b.Right; x++) {
					for (int y = b.Top; y < b.Bottom; y++) {
						Tile tile = Main.tile[x, y];
						if (!area.Contains(x, y) || !tile.HasTile || (tile.TileType != lumenstone && tile.TileType != moss)) {
							continue;
						}

						if (!Main.tile[x, y - 1].HasTile && !Main.tile[x, y - 2].HasTile && WorldGen.genRand.NextBool(30)) {
							WorldGen.PlaceObject(x, y - 1, shards, mute: true, style: 0, alternate: 0);
						}
						else if (!Main.tile[x, y + 1].HasTile && !Main.tile[x, y + 2].HasTile && WorldGen.genRand.NextBool(36)) {
							WorldGen.PlaceObject(x, y + 1, shards, mute: true, style: 0, alternate: LumenShards.CeilingAlternate);
						}
					}
				}
			}
		}

		private static bool IsOpenAir(int x, int y) {
			Tile tile = Main.tile[x, y];
			return !tile.HasTile && tile.LiquidAmount == 0;
		}
	}
}
