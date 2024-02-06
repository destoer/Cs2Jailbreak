// base lr class
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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;


public class SDHeadshotOnly : SDBase
{
    public override void Setup()
    {
        localize_announce("sd.headshot_start");
        localize_announce("sd.damage_enable",delay);
    }

    public override void Start()
    {
        localize_announce("sd.fight");
    }

    public override void End()
    {
        localize_announce("sd.headshot_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        player.StripWeapons(true);
        player.GiveWeapon("deagle");
        weaponRestrict = "deagle";
    }

    public override void PlayerHurt(CCSPlayerController? player,int health,int damage, int hitgroup) 
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        // dont allow damage when its not to head
        if(hitgroup != Lib.HITGROUP_HEAD)
        {
           player.RestoreHP(damage,health);
        }
    }
}