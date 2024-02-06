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

            if(Config.wardayGuns)
            {
                // give T guns
                foreach(CCSPlayerController player in Utilities.GetPlayers())
                {
                    if(player.is_valid() && player.IsT())
                    {
                        player.event_gun_menu();
                    }
                }
            }

            Entity.ForceOpen();

            Chat.localize_announce(WARDAY_PREFIX,"warday.live");
        }
    }

    public bool StartWarday(String location, int delay)
    {
        if(roundCounter >= ROUND_LIMIT)
        {
            // must wait again to start a warday
            roundCounter = 0;

            warday_active = true;
            JailPlugin.start_event();
            
            Entity.ForceClose();

            if(Config.wardayGuns)
            {
                foreach(CCSPlayerController player in Utilities.GetPlayers())
                {
                    if(player.is_valid() && player.IsCt())
                    {
                        player.event_gun_menu();
                    }
                }
            }


            countdown.start(Chat.localize("warday.location",location),delay,0,null,gun_callback);
            return true;
        }        

        return false;
    }

    public void RoundEnd()
    {
        countdown.kill();
    }


    public void RoundStart()
    {
        // one less round till a warday can be called
        roundCounter++;

        countdown.kill();

        warday_active = false;
        JailPlugin.end_event();
    }

    public void MapStart()
    {
        // give a warday on map start
        roundCounter = ROUND_LIMIT;
    }

    public JailConfig Config = new JailConfig();

    String WARDAY_PREFIX = $" {ChatColors.Green} [Warday]: {ChatColors.White}";

    bool warday_active = false;

    public int roundCounter = ROUND_LIMIT;

    public const int ROUND_LIMIT = 3;

    Countdown<int> countdown = new Countdown<int>();
};