using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static ChensForcedTeamworkMod.FTHelper;

namespace ChensForcedTeamworkMod
{
  public class ChensForcedTeamworkMod : Mod
  {
    public bool hasSeenLeader;
    public int assignedLeader;
    public string assignedLeaderName;

    public ChensForcedTeamworkMod()
    {
      hasSeenLeader = false;
      assignedLeader = -1;
      assignedLeaderName = "";
    }

    public override void PostUpdateEverything()
    {
      if (Main.netMode == NetmodeID.Server)
      {
        if (!hasSeenLeader)
        {
          if (FindLeader(this) >= 0)
          {
            hasSeenLeader = true;
            return;
          }
          else
          {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
              Player p = Main.player[i];
              if (p.active)
              {
                hasSeenLeader = true;

                if (!p.GetModPlayer<FTPlayer>().isLeader)
                {
                  p.GetModPlayer<FTPlayer>().isLeader = true;
                  if (assignedLeader >= 0)
                  {
                    SendMessage($"{assignedLeaderName} ran away from their responsibilities.");
                  }
                }

                assignedLeader = i;
                assignedLeaderName = p.name;
                SendMessage($"Leader role assigned to {assignedLeaderName}.");
                return;
              }
            }
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
          byte leaderInd = reader.ReadByte();
          byte selectInd = reader.ReadByte();
          Player leaderP = Main.player[leaderInd];
          Player selectP = Main.player[selectInd];
          TeleportEffects(selectP, leaderP);
          selectP.Center = reader.ReadVector2();

          if (Main.netMode == NetmodeID.Server)
          {
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.TeleportBack);
            packet.Write((byte)leaderP.whoAmI);
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

        case (byte)PacketMessageType.UpdateServerLeader:
          if (Main.netMode == NetmodeID.Server)
          {
            bool receiveData = reader.ReadBoolean();
            if (receiveData != hasSeenLeader) hasSeenLeader = receiveData;
          }
          break;

        case (byte)PacketMessageType.RequestServerLeader:
          if (Main.netMode == NetmodeID.Server)
          {
            ModPacket packet = GetPacket();
            packet.Write((byte)PacketMessageType.RequestServerLeader);
            packet.Write(assignedLeader);
            packet.Write(assignedLeaderName);
            packet.Send(whoAmI);
          }
          else
          {
            assignedLeader = reader.ReadInt32();
            assignedLeaderName = reader.ReadString();

            if (assignedLeader == whoAmI)
            {
              Main.NewText($"You are the Leader, {assignedLeaderName}. Plan well.");
            }
            else if (assignedLeader >= 0)
            {
              Main.NewText($"The Leader is {assignedLeaderName}. Stick together now.");
            }
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