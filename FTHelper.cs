using Terraria;

namespace ChensForcedTeamworkMod
{
  public static class FTHelper
  {
    public static bool IsValidPlayer(Player p, Player triggerer)
    {
      return p.active && !p.dead && p.whoAmI != triggerer.whoAmI;
    }

    public static int FindLeader()
    {
      for (int i = 0; i < Main.maxPlayers; i++)
      {
        FTPlayer ftp = Main.player[i].GetModPlayer<FTPlayer>();
        if (ftp.isLeader) return i;
      }

      return -1;
    }
  }

  internal enum PacketMessageType : byte
  {
    SyncLeader,
    TeleportBack,
    ClientChanges,
    SelectLeader,
    BroadcastMessage
  }
}