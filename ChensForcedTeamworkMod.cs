using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChensForcedTeamworkMod
{
  public class ChensForcedTeamworkMod : Mod
  {
    public bool hasSeenLeader;

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
            SendMessage($"Leader role assigned to {p.name}.");
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

        case (byte)PacketMessageType.BroadcastMessage:
          string msg = reader.ReadString();
          Main.NewText(msg);

          if (Main.netMode == NetmodeID.Server)
          {
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.BroadcastMessage);
            packet.Write(msg);
            packet.Send(-1, whoAmI);
          }
          break;
      }
    }

    public void SendMessage(string message)
    {
      Main.NewText(message);

      ModPacket packet = GetPacket();
      packet.Write((byte)PacketMessageType.BroadcastMessage);
      packet.Write(message);
      packet.Send();
    }
  }
}