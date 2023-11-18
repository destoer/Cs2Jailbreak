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

public class LRGunToss : LRBase
{
    public LRGunToss(LastRequest manager,int lr_slot, int player_slot, String choice) : base(manager,"Gun toss",lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        weapon_restrict = "deagle";
        player.GiveNamedItem("weapon_deagle");

        // empty ammo so players dont shoot eachother
        var deagle = Lib.find_weapon(player,"weapon_deagle");

        if(deagle != null)
        {
            deagle.set_ammo(0,0);
        }         
    }
}