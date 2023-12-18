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


public class SDGrenade : SDBase
{
    public override void setup()
    {
        localise_announce("sd.grenade_start");
        localise_announce("sd.damage_enable",delay);
    }

    public override void start()
    {
        localise_announce("sd.fight");
    }

    public override void end()
    {
        localise_announce("sd.grenade_end");
    }

    public override void setup_player(CCSPlayerController player)
    {
        player.strip_weapons(true);
        player.set_health(175);
        player.GiveNamedItem("weapon_hegrenade");
        weapon_restrict = "hegrenade";
    }

    public override void grenade_thrown(CCSPlayerController? player)
    {
        Lib.give_event_nade_delay(player,1.4f,"weapon_hegrenade");
    }
}