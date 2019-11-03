using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChensForcedTeamworkMod
{
  public class ChensForcedTeamworkMod : Mod
  {
    private bool hasSeenLeader;

	  public ChensForcedTeamworkMod()
    {
      hasSeenLeader = false;
    }

    public override void PreUpdateEntities()
    {
      if (Main.netMode == NetmodeID.Server && !hasSeenLeader)
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player p = Main.player[i];
          if (p.active)
          {
            p.GetModPlayer<FTPlayer>().isLeader = true;
            hasSeenLeader = true;
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.SelectLeader);
            packet.Send(i);
            return;
          }
        }
      }
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
      byte mode = reader.ReadByte();
      switch (mode)
      {
        case (byte)PacketMessageType.TeleportBack:
          byte selectInd = reader.ReadByte();
          Player selectP = Main.player[selectInd];
          selectP.Center = reader.ReadVector2();

          if (Main.netMode == NetmodeID.Server)
          {
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.TeleportBack);
            packet.Write((byte)selectP.whoAmI);
            packet.WriteVector2(selectP.Center);
            packet.Send(-1, whoAmI);
          }
          break;

        case (byte)PacketMessageType.SyncLeader:
          byte pInd = reader.ReadByte();
          Player pl = Main.player[pInd];
          pl.GetModPlayer<FTPlayer>().isLeader = reader.ReadBoolean();
          break;

        case (byte)PacketMessageType.ClientChanges:
          byte clientInd = reader.ReadByte();
          Player clientP = Main.player[clientInd];
          clientP.GetModPlayer<FTPlayer>().isLeader = reader.ReadBoolean();

          if (Main.netMode == NetmodeID.Server)
          {
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.ClientChanges);
            packet.Write((byte)clientP.whoAmI);
            packet.Write(clientP.GetModPlayer<FTPlayer>().isLeader);
            packet.Send(-1, whoAmI);
          }
          break;

        case (byte)PacketMessageType.SelectLeader:
          Main.LocalPlayer.GetModPlayer<FTPlayer>().isLeader = true;
          break;
      }
    }

    internal enum PacketMessageType : byte
    {
      SyncLeader,
      TeleportBack,
      ClientChanges,
      SelectLeader
    }
  }

  public class FTPlayer : ModPlayer
  {
    public bool isLeader = false;

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

    public override void clientClone(ModPlayer clientClone)
    {
      FTPlayer clone = clientClone as FTPlayer;
      clone.isLeader = isLeader;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
      ModPacket packet = mod.GetPacket();
      packet.Write((byte)ChensForcedTeamworkMod.PacketMessageType.SyncLeader);
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
          packet.Write((byte)ChensForcedTeamworkMod.PacketMessageType.ClientChanges);
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
            selectP.Center = player.Center;
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)ChensForcedTeamworkMod.PacketMessageType.TeleportBack);
            packet.Write((byte)selectP.whoAmI);
            packet.WriteVector2(player.Center);
            packet.Send();
          }
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