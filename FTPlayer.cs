using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static ChensForcedTeamworkMod.FTHelper;

namespace ChensForcedTeamworkMod
{
  public class FTPlayer : ModPlayer
  {
    public bool isLeader = false;
    public bool failedTeamwork = false;

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
      if (failedTeamwork) failedTeamwork = false;
      else
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player selectedPlayer = Main.player[i];
          if (IsValidPlayer(selectedPlayer, player))
          {
            PlayerDeathReason failedMessage = new PlayerDeathReason
            {
              SourceCustomReason = $"{selectedPlayer.name} failed because of {player.name}."
            };
            selectedPlayer.GetModPlayer<FTPlayer>().failedTeamwork = true;
            selectedPlayer.KillMe(failedMessage, selectedPlayer.statLifeMax2, hitDirection, pvp);
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
          if (selectP.active && !selectP.dead && Vector2.Distance(player.Center, selectP.Center) > 3000f)
          {
            TeleportEffects(selectP, player);
            selectP.Center = player.Center;

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.TeleportBack);
            packet.Write((byte)player.whoAmI);
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
        ModPacket packet = mod.GetPacket();
        packet.Write((byte)PacketMessageType.UpdateServerLeader);
        packet.Write(false);
        packet.Send();
      }
    }

    public override void OnEnterWorld(Player player)
    {
      if (Main.netMode == NetmodeID.MultiplayerClient)
      {
        ModPacket packet = mod.GetPacket();
        packet.Write((byte)PacketMessageType.RequestServerLeader);
        packet.Send();
      }
    }
  }
}