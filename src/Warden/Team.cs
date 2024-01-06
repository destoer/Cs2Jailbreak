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
    static readonly String TEAM_PREFIX = $" {ChatColors.Green}[TEAM]: {ChatColors.White}";
    
    public bool join_team(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
        {
            invoke.play_sound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        if(command.ArgCount < 2)
        {
            invoke.play_sound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        CCSPlayerPawn? pawn = invoke.pawn(); 


        if(!Int32.TryParse(command.ArgByIndex(1),out int team))
        {
            invoke.play_sound("sounds/ui/counter_beep.vsnd");
            return false;
        }

        switch(team)
        {
            case Lib.TEAM_CT:
            {
                if(config.ct_swap_only)
                {
                    invoke.announce(TEAM_PREFIX,$"Sorry guards must be swapped to CT by admin");
                    invoke.play_sound("sounds/ui/counter_beep.vsnd");
                    return false;
                }

                int ct_count = Lib.ct_count();
                int t_count = Lib.t_count();

                // check CT aint full 
                // i.e at a suitable raito or either team is empty
                if((ct_count * config.bal_guards) > t_count && ct_count != 0 && t_count != 0)
                {
                    invoke.announce(TEAM_PREFIX,$"Sorry, CT has too many players {config.bal_guards}:1 ratio maximum");
                    invoke.play_sound("sounds/ui/counter_beep.vsnd");
                    return false;
                }

                return true;         
            }

            case Lib.TEAM_T:
            {
                return true;
            }

            case Lib.TEAM_SPEC:
            {
                return true;
            }

            default:
            {
                invoke.play_sound("sounds/ui/counter_beep.vsnd");
                return false;
            }
        }
    }

    [RequiresPermissions("@css/generic")]
    public void swap_guard_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
        {
            return;
        }

        if(command.ArgCount != 2)
        {
            invoke.localise("warden.swap_guard_desc");
            return;
        }

        var target = command.GetArgTargetResult(1);

        foreach(CCSPlayerController player in target)
        {
            if(player.is_valid())
            {
                invoke.localise("warden.guard_swapped",player.PlayerName);
                player.SwitchTeam(CsTeam.CounterTerrorist);
            }
        }
    }
}