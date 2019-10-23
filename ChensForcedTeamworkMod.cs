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

  public static class FTHelper
  {
    public static bool IsValidPlayer(Player p, Player triggerer)
    {
      return p.active && !p.dead && p.whoAmI != triggerer.whoAmI;
    }
  }
}