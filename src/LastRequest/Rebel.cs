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
    bool CanRebel()
    {
        return Lib.AliveTCount() == 1;
    }

    public void RebelGuns(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.IsLegal())
        {
            return;
        }

        if(!CanRebel() || rebelType != RebelType.NONE)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.rebel_last");
            return;
        }

        Weapon.GunMenuGive(player,option);
    
        player.SetHealth(Lib.AliveCtCount() * 100);

        rebelType = RebelType.REBEL;

        Chat.LocalizeAnnounce(LR_PREFIX,"lr.player_name",player.PlayerName);
    }

    public void StartRebel(CCSPlayerController? player, ChatMenuOption option)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.GunMenuInternal(false,RebelGuns);
    }

    public void StartKnifeRebel(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.IsLegal())
        {
            return;
        }

        if(!CanRebel())
        {
            rebel.LocalisePrefix(LR_PREFIX,"rebel.last_alive");
            return;
        }

        rebelType = RebelType.KNIFE;

        Chat.LocalizeAnnounce(LR_PREFIX,"lr.knife_rebel",rebel.PlayerName);
        rebel.SetHealth(Lib.AliveCtCount() * 100);

        foreach(CCSPlayerController? player in Lib.GetPlayers())
        {
            if(player.IsLegalAlive())
            {
                player.StripWeapons();
            }
        }
    }

    public void RiotRespawn()
    {
        // riot cancelled in mean time
        if(rebelType != RebelType.RIOT)
        {
            return;
        }


        Chat.LocalizeAnnounce(LR_PREFIX,"lr.riot_active");

        foreach(CCSPlayerController? player in Lib.GetPlayers())
        {
            if(!player.IsLegalAlive() && player.IsT())
            {
                player.Respawn();
            }
        }
    }


    public void StartRiot(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.IsLegal())
        {
            return;
        }

        if(!CanRebel())
        {
            rebel.LocalisePrefix(LR_PREFIX,"lr.rebel_last");
            return;
        }


        rebelType = RebelType.RIOT;

        Chat.LocalizeAnnounce(LR_PREFIX,"lr.riot_start");

        if(JailPlugin.globalCtx != null)
        {
            JailPlugin.globalCtx.AddTimer(15.0f,RiotRespawn,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }
    }


    enum RebelType
    {
        NONE,
        REBEL,
        KNIFE,
        RIOT,
    };

    RebelType rebelType = RebelType.NONE;

}