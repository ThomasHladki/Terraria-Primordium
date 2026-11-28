using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Primordium.Content.Biomes;
using Primordium.Content.Items.Placeable;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Primordium.Content.Tiles.LuminousCaverns
{
	public class LumenTorchTile : ModTile
	{
		// Frames at or past this X are the unlit (wired off) half of the sheet.
		private const int LitFrameLimit = 66;
		private static readonly int[] TorchAnchorTiles = { 124, 561, 574, 575, 576, 577, 578 };

		private Asset<Texture2D> flameTexture;

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.DisableSmartInteract[Type] = true;
			TileID.Sets.Torch[Type] = true;

			DustType = DustID.IceTorch;
			AdjTiles = new int[] { TileID.Torches };
			VanillaFallbackOnModDeletion = TileID.Torches;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			// Built by hand instead of copied from vanilla torches, so every alternate allows water.
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
			SetUnderwater(TileObjectData.newTile);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			SetUnderwater(TileObjectData.newAlternate);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = TorchAnchorTiles;
			TileObjectData.addAlternate(1);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			SetUnderwater(TileObjectData.newAlternate);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = TorchAnchorTiles;
			TileObjectData.addAlternate(2);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			SetUnderwater(TileObjectData.newAlternate);
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.addAlternate(0);
			TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<LumenTorch>());
			AddMapEntry(new Color(120, 230, 250), Language.GetText("ItemName.Torch"));

			if (!Main.dedServ) {
				flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
			}
		}

		private static void SetUnderwater(TileObjectData data) {
			data.WaterDeath = false;
			data.LavaDeath = true;
			data.WaterPlacement = LiquidPlacement.Allowed;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<LumenTorch>();
		}

		public override float GetTorchLuck(Player player) {
			return player.InModBiome<LuminousCavernsBiome>() ? 1f : 0f;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = Main.rand.Next(1, 3);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameX < LitFrameLimit) {
				r = 0.35f;
				g = 0.9f;
				b = 1.05f;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = WorldGen.SolidTile(i, j - 1) ? 4 : 0;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			if (flameTexture == null || tile.TileFrameX >= LitFrameLimit || !TileDrawing.IsVisible(tile)) {
				return;
			}

			int offsetY = WorldGen.SolidTile(i, j - 1) ? 4 : 0;
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
			Color color = new Color(90, 200, 220, 0);
			const int width = 20;
			const int height = 20;

			for (int k = 0; k < 7; k++) {
				float xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
				Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero;
				spriteBatch.Draw(flameTexture.Value, position, new Rectangle(tile.TileFrameX, tile.TileFrameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
			}
		}

		public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
			if (!visible || tileFrameX >= LitFrameLimit || !Main.rand.NextBool(40)) {
				return;
			}

			Dust dust = Dust.NewDustDirect(new Vector2(i * 16 + 4, j * 16), 4, 4, DustID.IceTorch, 0f, 0f, 100);
			dust.noGravity = !Main.rand.NextBool(3);
			dust.velocity *= 0.3f;
			dust.velocity.Y -= 1.5f;
		}
	}
}
