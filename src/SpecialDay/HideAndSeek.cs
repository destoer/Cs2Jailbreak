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
using System.Drawing;


public class SDHideAndSeek : SDBase
{
    public override void setup()
    {
        localise_announce("sd.hide_start");
        localise_announce("sd.t_hide",delay);
    }

    public override void start()
    {
        // unfreeze all players
        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(player == null || !player.is_valid_alive())
            {
                continue;
            }

            if(player.is_t())
            {
                player.GiveNamedItem("weapon_knife");
            }

            player.unfreeze();
        }

        localise_announce("sd.seeker_release");
    }

    public override void end()
    {
        localise_announce("sd.hide_end");
    }

    public override void setup_player(CCSPlayerController player)
    {
        // lock them in place 500 hp, gun menu
        if(player.is_ct())
        {
            player.freeze();
            player.event_gun_menu();
            player.set_health(500);
        }

        // invis
        else
        {
            player.set_colour(Color.FromArgb(0,0,0,0));
            player.strip_weapons(true);
        }
    }

    public override void cleanup_player(CCSPlayerController player)
    {
        player.unfreeze();
    }
}