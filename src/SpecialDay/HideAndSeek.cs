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
    public override void Setup()
    {
        localize_announce("sd.hide_start");
        localize_announce("sd.t_hide",delay);
    }

    public override void Start()
    {
        // unfreeze all players
        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(!player.is_valid_alive())
            {
                continue;
            }

            if(player.IsT())
            {
                player.GiveWeapon("knife");
            }

            player.UnFreeze();
        }

        localize_announce("sd.seeker_release");
    }

    public override void End()
    {
        localize_announce("sd.hide_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        // lock them in place 500 hp, gun menu
        if(player.IsCt())
        {
            player.Freeze();
            player.EventGunMenu();
            player.SetHealth(500);
        }

        // invis
        else
        {
            player.SetColour(Color.FromArgb(0,0,0,0));
            player.StripWeapons(true);
        }
    }

    public override void CleanupPlayer(CCSPlayerController player)
    {
        player.UnFreeze();
    }
}