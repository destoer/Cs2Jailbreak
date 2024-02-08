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

public partial class Warden
{
    public static String TEAM_PREFIX = $" {ChatColors.Green}[TEAM]: {ChatColors.White}";
    
    public bool JoinTeam(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.IsLegal())
        {
            invoke.PlaySound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        if(command.ArgCount < 2)
        {
            invoke.Announce(TEAM_PREFIX,$"Invalid team swap args");
            invoke.PlaySound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        CCSPlayerPawn? pawn = invoke.Pawn(); 


        if(!Int32.TryParse(command.ArgByIndex(1),out int team))
        {
            invoke.PlaySound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        switch(team)
        {
            case Player.TEAM_CT:
            {
                if(Config.ctSwapOnly)
                {
                    invoke.Announce(TEAM_PREFIX,$"Sorry guards must be swapped to CT by admin");
                    invoke.PlaySound("sounds/ui/counter_beep.vsnd");
                    return false;
                }

                int CtCount = Lib.CtCount();
                int TCount = Lib.TCount();

                // check CT aint full 
                // i.e at a suitable raito or either team is empty
                if((CtCount * Config.balGuards) > TCount && CtCount != 0 && TCount != 0)
                {
                    invoke.Announce(TEAM_PREFIX,$"Sorry, CT has too many players {Config.balGuards}:1 ratio maximum");
                    invoke.PlaySound("sounds/ui/counter_beep.vsnd");
                    return false;
                }

                return true;         
            }

            case Player.TEAM_T:
            {
                return true;
            }

            case Player.TEAM_SPEC:
            {
                return true;
            }

            default:
            {
                invoke.Announce(TEAM_PREFIX,$"Invalid team swap team");
                invoke.PlaySound("sounds/ui/counter_beep.vsnd");
                return false;
            }
        }
    }

    [RequiresPermissions("@css/generic")]
    public void SwapGuardCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.IsLegal())
        {
            return;
        }

        if(command.ArgCount != 2)
        {
            invoke.Localize("warden.swap_guard_desc");
            return;
        }

        var target = command.GetArgTargetResult(1);

        foreach(CCSPlayerController player in target)
        {
            if(player.IsLegal())
            {
                invoke.Localize("warden.guard_swapped",player.PlayerName);
                player.SwitchTeam(CsTeam.CounterTerrorist);
            }
        }
    }
}