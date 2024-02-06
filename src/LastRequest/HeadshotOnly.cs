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

public class LRHeadshotOnly : LRBase
{
    public LRHeadshotOnly(LastRequest manager,LastRequest.LRType type,int lr_slot, int player_slot, String choice) : base(manager,type,lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        weapon_restrict = "deagle";

        player.GiveWeapon("deagle");
    }

    public override void PlayerHurt(int health,int damage, int hitgroup) 
    {
        // dont allow damage when its not to head
        if(hitgroup != Lib.HITGROUP_HEAD)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
            player.restore_hp(damage,health);
        }
    }
}