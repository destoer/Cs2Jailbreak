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

public class Warday
{
    void gun_callback()
    {
        // if warday is no longer active dont allow guns

        if(warday_active)
        {
            // give T guns
            foreach(CCSPlayerController player in Utilities.GetPlayers())
            {
                if(player.is_valid() && player.TeamNum == Lib.TEAM_T)
                {
                    player.event_gun_menu();
                }
            }

            Lib.announce(WARDAY_PREFIX,"Weapons live!");
        }

        warday_timer = null;
    }

    public bool start_warday(String location)
    {
        if(round_counter >= ROUND_LIMIT)
        {
            Lib.announce(WARDAY_PREFIX,$"warday will start in 20 seconds at {location}");

            // must wait again to start a warday
            round_counter = 0;

            warday_active = true;
            JailPlugin.start_event();
            

            foreach(CCSPlayerController player in Utilities.GetPlayers())
            {
                if(player.is_valid() && player.TeamNum == Lib.TEAM_CT)
                {
                    player.event_gun_menu();
                }
            }

            // start gun callback
            if(JailPlugin.global_ctx != null)
            {
                JailPlugin.global_ctx.AddTimer(20.0f,gun_callback,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
            }

            return true;
        }        

        return false;
    }


    public void round_start()
    {
        // one less round till a warday can be called
        round_counter++;

        Lib.kill_timer(ref warday_timer);

        warday_active = false;
        JailPlugin.end_event();
    }

    public void map_start()
    {
        // give a warday on map start
        round_counter = ROUND_LIMIT;
    }

    String WARDAY_PREFIX = $" {ChatColors.Green} [Warday]: {ChatColors.White}";

    bool warday_active = false;

    public int round_counter = ROUND_LIMIT;

    public const int ROUND_LIMIT = 3;

    CSTimer.Timer? warday_timer = null;
};