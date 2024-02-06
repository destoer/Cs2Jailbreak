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


public class SDJuggernaut : SDBase
{
    public override void Setup()
    {
        LocalizeAnnounce("sd.juggernaut_start");
        LocalizeAnnounce("sd.damage_enable",delay);
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.ffd_enable");
        Lib.EnableFriendlyFire();
    }

    public override void End()
    {
        LocalizeAnnounce("sd.juggernaut_end");
    }

    public override void Death(CCSPlayerController? player, CCSPlayerController? attacker)
    {
        if(!player.IsLegal() || attacker == null || !attacker.IsLegalAlive())
        {
            return;
        }

        // Give attacker 100 hp
        attacker.SetHealth(attacker.GetHealth() + 100);
    }

    public override void SetupPlayer(CCSPlayerController? player)
    {
        player.EventGunMenu();
    }
}