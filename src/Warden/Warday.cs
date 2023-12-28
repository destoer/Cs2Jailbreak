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
    void gun_callback(int unused)
    {
        // if warday is no longer active dont allow guns

        if(warday_active)
        {

            if(config.warday_guns)
            {
                // give T guns
                foreach(CCSPlayerController player in Utilities.GetPlayers())
                {
                    if(player.is_valid() && player.TeamNum == Lib.TEAM_T)
                    {
                        player.event_gun_menu();
                    }
                }
            }

            Lib.force_open();

            Lib.localise_announce(WARDAY_PREFIX,"warday.live");
        }
    }

    public bool start_warday(String location, int delay)
    {
        if(round_counter >= ROUND_LIMIT)
        {
            // must wait again to start a warday
            round_counter = 0;

            warday_active = true;
            JailPlugin.start_event();
            

            if(config.warday_guns)
            {
                foreach(CCSPlayerController player in Utilities.GetPlayers())
                {
                    if(player.is_valid() && player.TeamNum == Lib.TEAM_CT)
                    {
                        player.event_gun_menu();
                    }
                }
            }


            countdown.start(Lib.localise("warday.location",location),delay,0,null,gun_callback);
            return true;
        }        

        return false;
    }

    public void round_end()
    {
        countdown.kill();
    }


    public void round_start()
    {
        // one less round till a warday can be called
        round_counter++;

        countdown.kill();

        warday_active = false;
        JailPlugin.end_event();
    }

    public void map_start()
    {
        // give a warday on map start
        round_counter = ROUND_LIMIT;
    }

    public JailConfig config = new JailConfig();

    String WARDAY_PREFIX = $" {ChatColors.Green} [Warday]: {ChatColors.White}";

    bool warday_active = false;

    public int round_counter = ROUND_LIMIT;

    public const int ROUND_LIMIT = 3;

    Countdown<int> countdown = new Countdown<int>();
};