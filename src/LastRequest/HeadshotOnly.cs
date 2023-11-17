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
    public LRHeadshotOnly(LastRequest manager,int lr_slot, int player_slot, String choice) : base(manager,"Headshot only",lr_slot,player_slot,choice)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        weapon_restrict = "deagle";

        player.GiveNamedItem("weapon_deagle");
    }

    public override bool take_damage(int health,int damage, int hitgroup) 
    {
        return hitgroup == Lib.HITGROUP_HEAD;
    }
}