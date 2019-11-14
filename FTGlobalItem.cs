using Terraria;
using Terraria.ModLoader;
using static ChensForcedTeamworkMod.FTHelper;

namespace ChensForcedTeamworkMod
{
  public class FTGlobalItem : GlobalItem
  {
    public override bool UseItem(Item item, Player player)
    {
      bool canUse = base.UseItem(item, player);

      if (canUse && Main.myPlayer == player.whoAmI && item.potion
          && (item.healLife > 0 || item.healMana > 0))
      {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
          Player pl = Main.player[i];
          if (IsValidPlayer(pl, player))
          {
            pl.statLife += item.healLife;
            pl.statMana += item.healMana;
            pl.HealEffect(item.healLife, false);
            pl.ManaEffect(item.healMana);

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.HealEveryone);
            packet.Write((byte)i);
            packet.Write(item.healLife);
            packet.Write(item.healMana);
            packet.Send();
          }
        }
      }

      return canUse;
    }
  }
}