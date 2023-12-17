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


public class SDFriendlyFire : SDBase
{
    public override void setup()
    {
        announce("Friendly fire day started");
        announce($"Please {delay} seconds for friendly fire to be enabled");
    }

    public override void start()
    {
        announce("Friendly fire enabled");
        Lib.enable_friendly_fire();
    }

    public override void end()
    {
        announce("Friendly fire day is over");
    }

    public override void setup_player(CCSPlayerController? player)
    {
        player.event_gun_menu();
    }
}