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

public class Warday
{
    public bool start_warday(String location)
    {
        if(round_counter >= ROUND_LIMIT)
        {
            Lib.announce("[WARDAY]: ",$"warday will start in 20 seconds at {location}");

            // must wait again to start a warday
            round_counter = 0;

            warday_active = true;

            // TODO: this should be on a timer delay

            // TODO: this check needs to moved for an actual menu
            if(warday_active)
            {
                // give everyone guns
                foreach(CCSPlayerController player in Utilities.GetPlayers())
                {
                    player.gun_menu();
                }
            }

            return true;
        }

        return false;
    }


    public void round_start()
    {
        // one less round till a warday can be called
        round_counter++;

        warday_active = false;
    }

    public void map_start()
    {
        // give a warday on map start
        round_counter = ROUND_LIMIT;
    }

    bool warday_active = false;

    public int round_counter = ROUND_LIMIT;

    public const int ROUND_LIMIT = 3;
};