using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ChensForcedTeamworkMod
{
	public class ChensForcedTeamworkMod : Mod
	{
		public ChensForcedTeamworkMod()
		{
		}
	}

  public class FTPlayer : ModPlayer
  {
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
      for (int i = 0; i < Main.maxPlayers; i++)
      {
        Player selectedPlayer = Main.player[i];
        if (FTHelper.IsValidPlayer(selectedPlayer, player))
        {
          selectedPlayer.KillMe(damageSource, selectedPlayer.statLifeMax2, hitDirection, pvp);
        }
      }
    }
  }

  //public class FTPotion : GlobalItem
  //{
  //  public override bool UseItem(Item item, Player player)
  //  {
  //    if (item.potion && item.healLife > 0)
  //    {
  //      for (int i = 0; i < Main.maxPlayers; i++)
  //      {
  //        Player selectedPlayer = Main.player[i];
  //        if (FTHelper.IsValidPlayer(selectedPlayer, player))
  //        {
  //          selectedPlayer.statLife += item.healLife;
  //          if (Main.myPlayer == player.whoAmI)
  //          {
  //            selectedPlayer.HealEffect(item.healLife, true);
  //          }
  //          Main.PlaySound(item.UseSound, selectedPlayer.Center);
  //        }
  //      }
  //    }
  //    return base.UseItem(item, player);
  //  }
  //}

  public static class FTHelper
  {
    public static bool IsValidPlayer(Player p, Player triggerer)
    {
      return p.active && !p.dead && p.whoAmI != triggerer.whoAmI;
    }
  }
}