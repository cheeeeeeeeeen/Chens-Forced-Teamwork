using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

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
        Player p = Main.player[i];
        if (p.active)
        {
          if (p.GetModPlayer<FTPlayer>().isLeader) return i;
        }
      }

      return -1;
    }

    public static int FindLeader(Mod mod)
    {
      ChensForcedTeamworkMod ftMod = mod as ChensForcedTeamworkMod;

      if (ftMod.assignedLeader >= 0 && Main.player[ftMod.assignedLeader].active)
      {
        return ftMod.assignedLeader;
      }
      else return -1;
    }

    public static void TeleportEffects(Player source, Player destination)
    {
      Main.PlaySound(new LegacySoundStyle(2, 6, Terraria.Audio.SoundType.Sound), source.Center);
      Main.PlaySound(new LegacySoundStyle(2, 6, Terraria.Audio.SoundType.Sound), destination.Center);

      for (int i = 0; i < 25; i++)
      {
        Dust.NewDust(source.position, source.width, source.height, 15);
        Dust.NewDust(destination.position, destination.width, destination.height, 15);
      }
    }
  }

  internal enum PacketMessageType : byte
  {
    SyncLeader,
    TeleportBack,
    ClientChanges,
    SelectLeader,
    BroadcastMessage,
    UpdateServerLeader,
    RequestServerLeader
  }
}