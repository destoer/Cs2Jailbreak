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


public class SDDodgeball : SDBase
{
    public override void Setup()
    {
        LocalizeAnnounce("sd.dodgeball_start");
        LocalizeAnnounce("sd.damage_enable",delay);
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.fight");
    }

    public override void End()
    {
        LocalizeAnnounce("sd.dodgeball_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        player.StripWeapons(true);
        player.GiveWeapon("flashbang");
        weaponRestrict = "flashbang";
    }

    public override void GrenadeThrown(CCSPlayerController? player)
    {
        player.GiveEventNadeDelay(1.4f,"weapon_flashbang");
    }

    public override void PlayerHurt(CCSPlayerController? player,CCSPlayerController? attacker,int damage, int health, int hitgroup)
    {
        if(player.IsLegalAlive())
        {
            player.Slay();
        }
    }

    public override void EntCreated(CEntityInstance entity)
    {
        entity.RemoveDelay(1.4f,"flashbang_projectile");
    }

}