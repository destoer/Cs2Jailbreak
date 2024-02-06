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
        localize_announce("sd.ffd_start");
        localize_announce("sd.damage_enable",delay);
    }

    public override void start()
    {
        localize_announce("sd.ffd_enable");
        Lib.enable_friendly_fire();
    }

    public override void end()
    {
        localize_announce("sd.ffd_end");
    }

    public override void setup_player(CCSPlayerController? player)
    {
        player.EventGunMenu();
    }
}