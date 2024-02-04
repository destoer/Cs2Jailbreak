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
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using System.Drawing;
using System.Text;
using System.Diagnostics.CodeAnalysis;

public static class Player
{
    // CONST DEFS
    public const int TEAM_SPEC = 1;
    public const int TEAM_T = 2;
    public const int TEAM_CT = 3;

    public static readonly Color DEFAULT_COLOUR = Color.FromArgb(255, 255, 255, 255);

    static public void give_armour(this CCSPlayerController? player)
    {
        if(player.is_valid_alive())
        {
            player.GiveNamedItem("item_assaultsuit");
        }
    }

    static public void slay(this CCSPlayerController? player)
    {
        if(player.is_valid_alive())
        {
            player.PlayerPawn.Value?.CommitSuicide(true, true);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool is_valid([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player != null && player.IsValid && player.PlayerPawn.IsValid && player.PlayerPawn.Value?.IsValid == true; 
    }

    static public bool is_connected([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.is_valid() && (player.Connected != PlayerConnectedState.PlayerDisconnecting && player.Connected != PlayerConnectedState.PlayerDisconnected);
    }

    static public bool is_t(this CCSPlayerController? player)
    {
        return is_valid(player) && player.TeamNum == TEAM_T;
    }

    static public bool is_ct(this CCSPlayerController? player)
    {
        return is_valid(player) && player.TeamNum == TEAM_CT;
    }

    static public bool is_valid_alive([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.is_connected() && player.PawnIsAlive && player.get_health() > 0;
    }

    static public bool is_valid_alive_t([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.is_valid_alive() && player.is_t();
    }


    static public bool is_valid_alive_ct([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.is_valid_alive() && player.is_ct();
    }

    static public int slot_from_name(String name)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            if(player.PlayerName == name)
            {
                return player.Slot;
            }
        }

        return -1;
    }

    static public CCSPlayerPawn? pawn(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;

        return pawn;
    }

    static public void set_health(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.Health = hp;
            Utilities.SetStateChanged(pawn,"CBaseEntity","m_iHealth");
        }
    }

    static public int get_health(this CCSPlayerController? player)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn == null)
        {
            return 0;
        }

        return pawn.Health;
    }

    static public void freeze(this CCSPlayerController? player)
    {
        player.set_movetype(MoveType_t.MOVETYPE_NONE);
    }

    static public void unfreeze(this CCSPlayerController? player)
    {
        player.set_movetype(MoveType_t.MOVETYPE_WALK);
    }

    static public void give_event_nade_delay(this CCSPlayerController? target,float delay, String name)
    {
        if(!target.is_valid_alive())
        {
            return;
        }

        int slot = target.Slot;

        JailPlugin.global_ctx.AddTimer(delay,() => 
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

            if(player.is_valid_alive())
            {
                //Server.PrintToChatAll("give nade");
                player.strip_weapons(true);
                player.GiveNamedItem(name);
            }
        });
    }

    static public void set_movetype(this CCSPlayerController? player, MoveType_t type)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.MoveType = type;
        }
    }

    static public void set_gravity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.GravityScale = value;
        }
    }

    static public void set_velocity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.VelocityModifier = value;
        }
    }


    static public void set_armour(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.ArmorValue = hp;
        }
    }

    static public void strip_weapons(this CCSPlayerController? player, bool remove_knife = false)
    {
        // only care if player is valid
        if(!player.is_valid_alive())
        {
            return;
        }

        player.RemoveWeapons();
        
        // dont remove knife its buggy
        if(!remove_knife)
        {
            player.give_weapon("knife");
        }
    }

    static public void set_colour(this CCSPlayerController? player, Color colour)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null && player.is_valid_alive())
        {
            pawn.RenderMode = RenderMode_t.kRenderTransColor;
            pawn.Render = colour;
            Utilities.SetStateChanged(pawn,"CBaseModelEntity","m_clrRender");
        }
    }

    static public bool is_generic_admin(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/generic"});
    }

    static public void play_sound(this CCSPlayerController? player, String sound)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.ExecuteClientCommand($"play {sound}");
    }


    // NOTE: i dont think we call this in the right context
    // OnPostThink doesn't appear to be good enough?
    static public void hide_weapon(this CCSPlayerController? player)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.PrimaryAddon = 0;
            pawn.SecondaryAddon = 0;
            pawn.AddonBits = 0;
        }
    }

    static public void listen_all(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.VoiceFlags |= VoiceFlags.ListenAll;
        player.VoiceFlags &= ~VoiceFlags.ListenTeam;
    }

    static public void listen_team(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.ListenAll;
        player.VoiceFlags |= VoiceFlags.ListenTeam;
    }

    static public void mute(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        // admins cannot be muted by the plugin
        if(!player.is_generic_admin())
        {
            player.VoiceFlags |= VoiceFlags.Muted;
        }
    }

    // TODO: this needs to be hooked into the ban system that becomes used
    static public void unmute(this CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.Muted;
    }


    public static void restore_hp(this CCSPlayerController? player, int damage, int health)
    {
        if(!player.is_valid())
        {
            return;
        }

        // TODO: why does this sometimes mess up?
        if(health < 100)
        {
            player.set_health(Math.Min(health + damage,100));
        }

        else
        {
            player.set_health(health + damage);
        }
    }


    static void respawn_callback(int? slot)
    {
        if(slot != null)
        {
            var player = Utilities.GetPlayerFromSlot(slot.Value);

            if(player.is_valid())
            {
                player.Respawn();
            }
        }   
    }

    static public void respawn_delay(this CCSPlayerController? player, float delay)
    {
        if(!player.is_valid())
        {
            return;
        }

        JailPlugin.global_ctx.AddTimer(delay,() => respawn_callback(player.Slot),CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
    }

}