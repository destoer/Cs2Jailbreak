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


public class SDTank : SDBase
{
    public override void setup()
    {
        localize_announce("sd.tank_start");
        localize_announce("sd.damage_enable",delay);
    }

    public override void make_boss(CCSPlayerController? tank, int count)
    {
        if(tank != null && tank.is_valid_alive())
        {
            localize_announce($"sd.tank",tank.PlayerName);

            // give the tank the HP and swap him
            tank.set_health(count * 100);
            tank.set_colour(Lib.RED);
            tank.SwitchTeam(CsTeam.CounterTerrorist);
        }

        else
        {
            Chat.announce("[ERROR]: ","Error picking tank");
        }
    }

    public override void start()
    {
        localize_announce("sd.fight");
        Lib.swap_all_t();

        (CCSPlayerController? boss, int count) = pick_boss();
        make_boss(boss,count);
    }

    public override void end()
    {
        localize_announce("sd.tank_end");
    }

    public override void setup_player(CCSPlayerController player)
    {
        player.event_gun_menu();
    }
}