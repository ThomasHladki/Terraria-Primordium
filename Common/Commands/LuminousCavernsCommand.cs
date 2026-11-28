using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Primordium.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Primordium.Common.Commands
{
	public class LuminousCavernsCommand : ModCommand
	{
		public override string Command => "luminous";

		public override CommandType Type => CommandType.Chat;

		public override string Usage => "/luminous [number]";

		public override string Description => Language.GetTextValue("Mods.Primordium.Commands.Luminous.Description");

		public override void Action(CommandCaller caller, string input, string[] args) {
			Player player = caller.Player;
			IReadOnlyList<Rectangle> spots = LuminousCavernsWorldGen.Locate();
			if (player == null || !player.active || spots.Count == 0) {
				caller.Reply(Language.GetTextValue("Mods.Primordium.Commands.Luminous.None"));
				return;
			}

			int index;
			if (args.Length == 0) {
				index = Nearest(player, spots);
			}
			else if (!int.TryParse(args[0], out int number) || number < 1 || number > spots.Count) {
				caller.Reply(Language.GetTextValue("Mods.Primordium.Commands.Luminous.Usage", spots.Count));
				return;
			}
			else {
				index = number - 1;
			}

			LuminousCavernsWorldGen.TryStand(spots[index], out Point tile);
			player.Teleport(new Vector2(tile.X * 16, (tile.Y + 1) * 16 - player.height), TeleportationStyleID.TeleportationPotion);
			caller.Reply(Language.GetTextValue("Mods.Primordium.Commands.Luminous.Arrived", index + 1, spots.Count));
		}

		private static int Nearest(Player player, IReadOnlyList<Rectangle> spots) {
			int best = 0;
			float bestDistance = float.MaxValue;
			Vector2 feet = player.Center / 16f;
			for (int i = 0; i < spots.Count; i++) {
				float dx = spots[i].Center.X - feet.X;
				float dy = spots[i].Center.Y - feet.Y;
				float distance = (dx * dx) + (dy * dy);
				if (distance < bestDistance) {
					bestDistance = distance;
					best = i;
				}
			}

			return best;
		}
	}
}
