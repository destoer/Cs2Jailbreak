
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

// debugging commands:
// TODO: these need to be sealed by admin
public static class Debug
{
    [RequiresPermissions("@jail/debug")]
    public static void nuke(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.announce(DEBUG_PREFIX,"Slaying all players");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.slay();
        }       
    }

    [RequiresPermissions("@jail/debug")]
    public static void test_laser(CCSPlayerController? invoke, CommandInfo command)
    {
        CCSPlayerPawn? pawn = invoke.pawn();

        if(pawn != null && pawn.AbsOrigin != null)
        {
            
            Vector mid =  new Vector(pawn.AbsOrigin.X,pawn.AbsOrigin.Y,pawn.AbsOrigin.Z);

            int lines = 25;

            float step = (float)(2.0f * Math.PI) / (float)lines;
            float r = 50.0f;

            float angle_old = 0.0f;
            float angle_cur = step;


            for(int i = 1; i < (lines + 1); i++)
            {
                Vector start = new Vector((float)(mid.X + (r * Math.Cos(angle_old))),(float)(mid.Y + (r *Math.Sin(angle_old))), mid.Z + 3.0f);
                Vector end = new Vector((float)(mid.X + (r * Math.Cos(angle_cur))),(float)(mid.Y + (r * Math.Sin(angle_cur))), mid.Z + 3.0f);

                if(invoke != null)
                {
                    invoke.PrintToConsole($"{i} : {angle_old} : {angle_cur}\n");
                }

                Lib.draw_laser(start,end,30.0f,2.0f,Lib.CYAN);

                angle_old = angle_cur;
                angle_cur += step;
            }
        }
    }
    
    [RequiresPermissions("@jail/debug")]
    public static void test_strip_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        invoke.strip_weapons(true);
    }

    [RequiresPermissions("@jail/debug")]
    public static void join_ct_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke != null && invoke.is_valid())
        {
            invoke.SwitchTeam(CsTeam.CounterTerrorist);
        }
    }

    [RequiresPermissions("@jail/debug")]
    public static void hide_weapon_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke != null && invoke.is_valid())
        {
            invoke.PrintToChat("hiding weapons");
        }

        invoke.hide_weapon();
    }

    [RequiresPermissions("@jail/debug")]
    public static void is_muted_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
        {
            return;
        }

        invoke.PrintToConsole("Is muted?");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            invoke.PrintToConsole($"{player.PlayerName} : {player.VoiceFlags.HasFlag(VoiceFlags.Muted)} : {player.VoiceFlags.HasFlag(VoiceFlags.ListenAll)} : {player.VoiceFlags.HasFlag(VoiceFlags.ListenTeam)}");
        } 
    }

    // are these commands allowed or not?
    public static readonly bool enable  = true;

    public static readonly String DEBUG_PREFIX = $" {ChatColors.Green}[DEBUG]: {ChatColors.White}";    
}