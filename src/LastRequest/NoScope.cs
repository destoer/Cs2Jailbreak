using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;

public class LRNoScope : LRBase
{
    public LRNoScope(LastRequest manager,LastRequest.LRType type,int lr_slot, int player_slot, String choice) : base(manager,type,lr_slot,player_slot,choice)
    {

    }

    void give_weapon(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }


        player.strip_weapons(true);

        switch(choice)
        {
            case "Scout":
            {
                weapon_restrict = "ssg08";
                player.give_weapon("ssg08");
                break;
            }

            case "Awp":
            {
                weapon_restrict = "awp";
                player.give_weapon("awp");
                break;
            }
        }
    }

    public override void init_player(CCSPlayerController player)
    {
        give_weapon(player);
    }

    public override void weapon_zoom()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

        // re give the weapons so they cannot zoom
        give_weapon(player);
    }
}