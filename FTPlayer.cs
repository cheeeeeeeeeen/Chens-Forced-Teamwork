using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static ChensForcedTeamworkMod.FTHelper;

namespace ChensForcedTeamworkMod
{
  public class FTPlayer : ModPlayer
  {
    public bool isLeader = false;

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
      if (damage == -707)
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player selectedPlayer = Main.player[i];
          if (IsValidPlayer(selectedPlayer, player))
          {
            damageSource.SourceCustomReason = $" failed because of {player.name}";
            selectedPlayer.KillMe(damageSource, -707, hitDirection, pvp);
          }
        }
      }
    }

    public override void clientClone(ModPlayer clientClone)
    {
      FTPlayer clone = clientClone as FTPlayer;
      clone.isLeader = isLeader;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
      ModPacket packet = mod.GetPacket();
      packet.Write((byte)PacketMessageType.SyncLeader);
      packet.Write((byte)player.whoAmI);
      packet.Write(isLeader);
      packet.Send(toWho, fromWho);
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
      if (clientPlayer is FTPlayer clone)
      {
        ModPacket packet;

        if (clone.isLeader != isLeader)
        {
          packet = mod.GetPacket();
          packet.Write((byte)PacketMessageType.ClientChanges);
          packet.Write((byte)player.whoAmI);
          packet.Write(isLeader);
          packet.Send();
        }
      }
    }

    public override void PostUpdate()
    {
      if (player.active && isLeader)
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player selectP = Main.player[i];
          if (selectP.active && !selectP.dead && Vector2.Distance(player.Center, selectP.Center) > 1000f * 16f)
          {
            selectP.Center = player.Center;
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.TeleportBack);
            packet.Write((byte)selectP.whoAmI);
            packet.WriteVector2(player.Center);
            packet.Send();
          }
        }
      }
    }

    public override void PlayerDisconnect(Player player)
    {
      FTPlayer ftPlayer = player.GetModPlayer<FTPlayer>();
      if (ftPlayer.isLeader)
      {
        ftPlayer.isLeader = false;
        if (mod is ChensForcedTeamworkMod cftm)
        {
          cftm.hasSeenLeader = false;
          cftm.SendMessage($"{player.name} ran away from their responsibilities.");
        }
      }
    }

    public override void OnEnterWorld(Player player)
    {
      int pInd = FindLeader();
      Player leadP = Main.player[pInd];

      if (pInd >= 0)
      {
        if (leadP.whoAmI == player.whoAmI)
        {
          Main.NewText($"You are the Leader, {leadP.name}. Plan well.");
        }
        else
        {
          Main.NewText($"The Leader is {leadP.name}. Stick together now.");
        }
      }
    }
  }
}