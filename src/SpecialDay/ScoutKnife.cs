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


public class SDScoutKnife : SDBase
{
    public override void setup()
    {
        announce("Scout knife started");
        announce($"Please {delay} seconds for damage be enabled");
    }

    public override void start()
    {
        announce("Fight!");
    }

    public override void end()
    {
        announce("Scout knife is over");
    }

    public override void setup_player(CCSPlayerController player)
    {
        player.strip_weapons();
        player.GiveNamedItem("weapon_ssg08");
        player.set_gravity(0.1f);
    }

    public override bool weapon_equip(CCSPlayerController player,String name) 
    {
        return name.Contains("knife") || name.Contains("ssg08");
    }

    public override void cleanup_player(CCSPlayerController player)
    {
        player.set_gravity(1.0f);
    }
}