
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

    public void EndSD(bool forced = false)
    {
        if(activeSD != null)
        {
            JailPlugin.EndEvent();
            activeSD.EndCommon();
            activeSD = null;

            countdown.Kill();

            // restore all players if from a cancel
            if(forced)
            {
                Chat.Announce(SPECIALDAY_PREFIX,"Special day cancelled");
            }  

            teamSave.Restore();
        }     
    }

    public void SetupSD(CCSPlayerController? invoke, ChatMenuOption option)
    {
        if(!invoke.IsLegal())
        {
            return;
        }

        if(activeSD != null)
        {
            invoke.Announce(SPECIALDAY_PREFIX,"You cannot call two SD's at once");
            return;
        }

        // invoked as warden
        // reset the round counter so they can't do it again
        if(wsdCommand)
        {
            wsdRound = 0;
        }


        String name = option.Text;

        switch(name)
        {
            case "Friendly fire":
            {
                activeSD = new SDFriendlyFire();
                type = SDType.FRIENDLY_FIRE;
                break;
            }

            case "Juggernaut":
            {
                activeSD = new SDJuggernaut();
                type = SDType.JUGGERNAUT;
                break;             
            }

            case "Tank":
            {
                activeSD = new SDTank();
                type = SDType.TANK;
                break;                          
            }

            case "Scout knife":
            {
                activeSD = new SDScoutKnife();
                type = SDType.SCOUT_KNIFE;
                break;
            }

            case "Headshot only":
            {
                activeSD = new SDHeadshotOnly();
                type = SDType.HEADSHOT_ONLY;
                break;             
            }

            case "Knife warday":
            {
                activeSD = new SDKnifeWarday();
                type = SDType.KNIFE_WARDAY;
                break;             
            }

            case "Hide and seek":
            {
                activeSD = new SDHideAndSeek();
                type = SDType.HIDE_AND_SEEK;
                break;               
            }

            case "Dodgeball":
            {
                activeSD = new SDDodgeball();
                type = SDType.DODGEBALL;
                break;             
            }

            case "Spectre":
            {
                activeSD = new SDSpectre();
                type = SDType.SPECTRE;
                break;                            
            }

            case "Grenade":
            {
                activeSD = new SDGrenade();
                type = SDType.GRENADE;
                break;             
            }

        }

        // 1up all dead players
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.IsLegal() && !player.IsLegalAlive())
            {
                player.Respawn();
            }
        }

        // call the intiail sd setup
        if(activeSD != null)
        {
            JailPlugin.StartEvent();

            activeSD.delay = delay;
            activeSD.SetupCommon();
        }

        // start the countdown for enable
        if(JailPlugin.globalCtx != null)
        {
            countdown.Start($"{name} specialday",delay,0,null,StartSD);
        }

        teamSave.Save();
    }

    public void StartSD(int unused)
    {
        if(activeSD != null)
        {
            // force ff active
            if(overrideFF)
            {
                Chat.LocalizeAnnounce(SPECIALDAY_PREFIX,"sd.ffd_enable");
                Lib.EnableFriendlyFire();
            }

            activeSD.StartCommon();
        }  
    }

    [RequiresPermissions("@css/generic")]
    public void CancelSDCmd(CCSPlayerController? player,CommandInfo command)
    {
        EndSD(true);
    }

    public void SDCmdInternal(CCSPlayerController? player,CommandInfo command)
    {
        if(!player.IsLegal())
        {
            return;
        }

        delay = 15;

        if(Int32.TryParse(command.ArgByIndex(1),out int delayOpt))
        {
            delay = delayOpt;
        }


        var sdMenu = new ChatMenu("Specialday");

        // Build the basic LR menu
        for(int s = 0; s < SD_NAME.Length - 1; s++)
        {
            sdMenu.AddMenuOption(SD_NAME[s], SetupSD);
        }
        
        MenuManager.OpenChatMenu(player, sdMenu);
    }


    [RequiresPermissions("@jail/debug")]
    public void SDRigCmd(CCSPlayerController? player,CommandInfo command)
    {
        if(!player.IsLegal())
        {
            return;
        }

        if(activeSD != null && activeSD.state == SDState.STARTED)
        {
            player.PrintToChat($"Rigged sd boss to {player.PlayerName}");
            activeSD.riggedSlot = player.Slot;
        }
    }   

    [RequiresPermissions("@css/generic")]
    public void SDCmd(CCSPlayerController? player,CommandInfo command)
    {
        overrideFF = false;
        wsdCommand = false;
        SDCmdInternal(player,command);
    }   

    [RequiresPermissions("@css/generic")]
    public void SDFFCmd(CCSPlayerController? player,CommandInfo command)
    {
        overrideFF = true;
        wsdCommand = false;
        SDCmdInternal(player,command);
    }   

    public void WardenSDCmdInternal(CCSPlayerController? player,CommandInfo command)
    {
        if(!JailPlugin.IsWarden(player))
        {
            player.Announce(SPECIALDAY_PREFIX,"You must be a warden to use this command");
            return;
        }

        // Not ready yet
        if(wsdRound < Config.wsdRound)
        {
            player.Announce(SPECIALDAY_PREFIX,$"Please wait {Config.wsdRound - wsdRound} more rounds");
            return;
        }

        // Go!
        wsdCommand = true;
        SDCmdInternal(player,command);
    }

    public void WardenSDCmd(CCSPlayerController? player,CommandInfo command)
    {
        overrideFF = false;

        WardenSDCmdInternal(player,command);
    }   

    public void WardenSDFFCmd(CCSPlayerController? player,CommandInfo command)
    {
        overrideFF = true;

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

    public int wsdRound = 0;

    // NOTE: if we cared we would make this per player
    // so we can't get weird conflicts, but its not a big deal
    bool wsdCommand = false;

    SDBase? activeSD = null;

    bool overrideFF = false;

    Countdown<int> countdown = new Countdown<int>();

    SDType type = SDType.NONE;

    public JailConfig Config = new JailConfig();

    TeamSave teamSave = new TeamSave();
};