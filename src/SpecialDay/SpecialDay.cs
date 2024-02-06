
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
using CounterStrikeSharp.API.Modules.Admin;

public enum SDState
{
    INACTIVE,
    STARTED,
    ACTIVE
};

// TODO: this will be done after lr and warden
// are in a decent state as its just an extra, and should 
// not take too long to port from css
public partial class SpecialDay
{

    public void end_sd(bool forced = false)
    {
        if(active_sd != null)
        {
            JailPlugin.end_event();
            active_sd.end_common();
            active_sd = null;

            countdown.kill();

            // restore all players if from a cancel
            if(forced)
            {
                Chat.announce(SPECIALDAY_PREFIX,"Special day cancelled");
            }  

            team_save.restore();
        }     
    }

    public void setup_sd(CCSPlayerController? invoke, ChatMenuOption option)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        if(active_sd != null)
        {
            invoke.announce(SPECIALDAY_PREFIX,"You cannot call two SD's at once");
            return;
        }

        // invoked as warden
        // reset the round counter so they can't do it again
        if(wsd_command)
        {
            wsd_round = 0;
        }


        String name = option.Text;

        switch(name)
        {
            case "Friendly fire":
            {
                active_sd = new SDFriendlyFire();
                type = SDType.FRIENDLY_FIRE;
                break;
            }

            case "Juggernaut":
            {
                active_sd = new SDJuggernaut();
                type = SDType.JUGGERNAUT;
                break;             
            }

            case "Tank":
            {
                active_sd = new SDTank();
                type = SDType.TANK;
                break;                          
            }

            case "Scout knife":
            {
                active_sd = new SDScoutKnife();
                type = SDType.SCOUT_KNIFE;
                break;
            }

            case "Headshot only":
            {
                active_sd = new SDHeadshotOnly();
                type = SDType.HEADSHOT_ONLY;
                break;             
            }

            case "Knife warday":
            {
                active_sd = new SDKnifeWarday();
                type = SDType.KNIFE_WARDAY;
                break;             
            }

            case "Hide and seek":
            {
                active_sd = new SDHideAndSeek();
                type = SDType.HIDE_AND_SEEK;
                break;               
            }

            case "Dodgeball":
            {
                active_sd = new SDDodgeball();
                type = SDType.DODGEBALL;
                break;             
            }

            case "Spectre":
            {
                active_sd = new SDSpectre();
                type = SDType.SPECTRE;
                break;                            
            }

            case "Grenade":
            {
                active_sd = new SDGrenade();
                type = SDType.GRENADE;
                break;             
            }

        }

        // 1up all dead players
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid() && !player.is_valid_alive())
            {
                player.Respawn();
            }
        }

        // call the intiail sd setup
        if(active_sd != null)
        {
            JailPlugin.start_event();

            active_sd.delay = delay;
            active_sd.setup_common();
        }

        // start the countdown for enable
        if(JailPlugin.global_ctx != null)
        {
            countdown.start($"{name} specialday",delay,0,null,start_sd);
        }

        team_save.save();
    }

    public void start_sd(int unused)
    {
        if(active_sd != null)
        {
            // force ff active
            if(override_ff)
            {
                Chat.localize_announce(SPECIALDAY_PREFIX,"sd.ffd_enable");
                Lib.enable_friendly_fire();
            }

            active_sd.start_common();
        }  
    }

    [RequiresPermissions("@css/generic")]
    public void CancelSDCmd(CCSPlayerController? player,CommandInfo command)
    {
        end_sd(true);
    }

    public void SDCmdInternal(CCSPlayerController? player,CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        delay = 15;

        if(Int32.TryParse(command.ArgByIndex(1),out int delay_opt))
        {
            delay = delay_opt;
        }


        ChatMenu sd_menu = new ChatMenu("Specialday");

        // Build the basic LR menu
        for(int s = 0; s < SD_NAME.Length - 1; s++)
        {
            sd_menu.AddMenuOption(SD_NAME[s], setup_sd);
        }
        
        ChatMenus.OpenMenu(player, sd_menu);
    }


    [RequiresPermissions("@jail/debug")]
    public void SDRigCmd(CCSPlayerController? player,CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(active_sd != null && active_sd.state == SDState.STARTED)
        {
            player.PrintToChat($"Rigged sd boss to {player.PlayerName}");
            active_sd.rigged_slot = player.Slot;
        }
    }   

    [RequiresPermissions("@css/generic")]
    public void SDCmd(CCSPlayerController? player,CommandInfo command)
    {
        override_ff = false;
        wsd_command = false;
        SDCmdInternal(player,command);
    }   

    [RequiresPermissions("@css/generic")]
    public void SDFFCmd(CCSPlayerController? player,CommandInfo command)
    {
        override_ff = true;
        wsd_command = false;
        SDCmdInternal(player,command);
    }   

    public void WardenSDCmdInternal(CCSPlayerController? player,CommandInfo command)
    {
        if(!JailPlugin.IsWarden(player))
        {
            player.announce(SPECIALDAY_PREFIX,"You must be a warden to use this command");
            return;
        }

        // Not ready yet
        if(wsd_round < Config.wsdRound)
        {
            player.announce(SPECIALDAY_PREFIX,$"Please wait {Config.wsdRound - wsd_round} more rounds");
            return;
        }

        // Go!
        wsd_command = true;
        SDCmdInternal(player,command);
    }

    public void WardenSDCmd(CCSPlayerController? player,CommandInfo command)
    {
        override_ff = false;

        WardenSDCmdInternal(player,command);
    }   

    public void WardenSDFFCmd(CCSPlayerController? player,CommandInfo command)
    {
        override_ff = true;

        WardenSDCmdInternal(player,command);
    }   

    public enum SDType
    {
        FRIENDLY_FIRE,
        JUGGERNAUT,
        TANK,
        SPECTRE,
        DODGEBALL,
        GRENADE,
        SCOUT_KNIFE,
        HIDE_AND_SEEK,
        HEADSHOT_ONLY,
        KNIFE_WARDAY,
        NONE
    };

    public static readonly String SPECIALDAY_PREFIX = $"  {ChatColors.Green}[Special day]: {ChatColors.White}";

    static String[] SD_NAME = {
        "Friendly fire",
        "Juggernaut",
        "Tank",
        "Spectre",
        "Dodgeball",
        "Grenade",
        "Scout knife",
        "Hide and seek",
        "Headshot only",
        "Knife warday",
        "None"
    };

    int delay = 15;

    public int wsd_round = 0;

    // NOTE: if we cared we would make this per player
    // so we can't get weird conflicts, but its not a big deal
    bool wsd_command = false;

    SDBase? active_sd = null;

    bool override_ff = false;

    Countdown<int> countdown = new Countdown<int>();

    SDType type = SDType.NONE;

    public JailConfig Config = new JailConfig();

    TeamSave team_save = new TeamSave();
};