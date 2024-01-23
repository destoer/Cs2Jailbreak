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
    public override void setup()
    {
        localise_announce("sd.juggernaut_start");
        localise_announce("sd.damage_enable",delay);
    }

    public override void start()
    {
        localise_announce("sd.ffd_enable");
        Lib.enable_friendly_fire();
    }

    public override void end()
    {
        localise_announce("sd.juggernaut_end");
    }

    public override void death(CCSPlayerController? player, CCSPlayerController? attacker)
    {
        if(!player.is_valid() || attacker == null || !attacker.is_valid_alive())
        {
            return;
        }

        // Give attacker 100 hp
        attacker.set_health(attacker.get_health() + 100);
    }

    public override void setup_player(CCSPlayerController? player)
    {
        player.event_gun_menu();
    }
}