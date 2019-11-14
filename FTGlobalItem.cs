using Terraria;
using Terraria.ModLoader;
using static ChensForcedTeamworkMod.FTHelper;

namespace ChensForcedTeamworkMod
{
  public class FTGlobalItem : GlobalItem
  {
    public override bool UseItem(Item item, Player player)
    {
      if (Main.myPlayer == player.whoAmI && item.potion
          && (item.healLife != 0 || item.healMana != 0))
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player pl = Main.player[i];
          if (IsValidPlayer(pl, player))
          {
            if (item.healLife != 0)
            {
              pl.HealEffect(item.healLife, false);
              pl.statLife += item.healLife;
            }
            if (item.healMana != 0)
            {
              pl.ManaEffect(item.healMana);
              pl.statMana += item.healMana;
            }

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.HealEveryone);
            packet.Write((byte)i);
            packet.Write(item.healLife);
            packet.Write(item.healMana);
            packet.Send();
          }
        }

        return true;
      }

      return base.UseItem(item, player);
    }
  }
}