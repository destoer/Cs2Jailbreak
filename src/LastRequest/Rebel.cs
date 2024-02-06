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
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public partial class LastRequest
{
    bool can_rebel()
    {
        return Lib.AliveTCount() == 1;
    }

    public void rebel_guns(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.IsLegal())
        {
            return;
        }

        if(!can_rebel() || rebel_type != RebelType.NONE)
        {
            player.localise_prefix(LR_PREFIX,"lr.rebel_last");
            return;
        }

        player.StripWeapons();

        player.GiveMenuWeapon(option.Text);
        player.GiveWeapon("deagle");

        player.GiveArmour();
    
        player.SetHealth(Lib.AliveCtCount() * 100);

        rebel_type = RebelType.REBEL;

        Chat.localize_announce(LR_PREFIX,"lr.player_name",player.PlayerName);
    }

    public void start_rebel(CCSPlayerController? player, ChatMenuOption option)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.GunMenuInternal(false,rebel_guns);
    }

    public void start_knife_rebel(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.IsLegal())
        {
            return;
        }

        if(!can_rebel())
        {
            rebel.localise_prefix(LR_PREFIX,"rebel.last_alive");
            return;
        }

        rebel_type = RebelType.KNIFE;

        Chat.localize_announce(LR_PREFIX,"lr.knife_rebel",rebel.PlayerName);
        rebel.SetHealth(Lib.AliveCtCount() * 100);

        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(player.IsLegalAlive())
            {
                player.StripWeapons();
            }
        }
    }

    public void riot_respawn()
    {
        // riot cancelled in mean time
        if(rebel_type != RebelType.RIOT)
        {
            return;
        }


        Chat.localize_announce(LR_PREFIX,"lr.riot_active");

        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(player.IsLegal() && !player.IsLegalAlive())
            {
                Server.PrintToChatAll($"Respawn {player.PlayerName}");
                player.Respawn();
            }
        }
    }


    public void start_riot(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.IsLegal())
        {
            return;
        }

        if(!can_rebel())
        {
            rebel.localise_prefix(LR_PREFIX,"lr.rebel_last");
            return;
        }


        rebel_type = RebelType.RIOT;

        Chat.localize_announce(LR_PREFIX,"lr.riot_start");

        if(JailPlugin.globalCtx != null)
        {
            JailPlugin.globalCtx.AddTimer(15.0f,riot_respawn,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }
    }


    enum RebelType
    {
        NONE,
        REBEL,
        KNIFE,
        RIOT,
    };

    RebelType rebel_type = RebelType.NONE;

}