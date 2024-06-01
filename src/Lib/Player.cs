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

    static public void GiveArmour(this CCSPlayerController? player)
    {
        if(player.IsLegalAlive())
        {
            player.GiveNamedItem("item_assaultsuit");
        }
    }

    static public void Slay(this CCSPlayerController? player)
    {
        // Why does slay now count as damage, but doesn't set the kill state fast enough?
        if(player.IsLegalAlive() && player.GetHealth() > 0)
        {
            player.PlayerPawn.Value?.CommitSuicide(true, true);
            player.SetHealth(0);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool IsLegal([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player != null && player.IsValid && player.PlayerPawn.IsValid && player.PlayerPawn.Value?.IsValid == true; 
    }

    static public bool IsConnected([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.IsLegal() && player.Connected == PlayerConnectedState.PlayerConnected;
    }

    static public bool IsT([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return IsLegal(player) && player.TeamNum == TEAM_T;
    }

    static public bool IsCt([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return IsLegal(player) && player.TeamNum == TEAM_CT;
    }

    static public bool IsLegalAlive([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.IsConnected() && player.PawnIsAlive && player.PlayerPawn.Value?.LifeState == (byte)LifeState_t.LIFE_ALIVE;
    }

    static public bool IsLegalAliveT([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.IsLegalAlive() && player.IsT();
    }


    static public bool IsLegalAliveCT([NotNullWhen(true)] this CCSPlayerController? player)
    {
        return player.IsLegalAlive() && player.IsCt();
    }

    static public int SlotFromName(String name)
    {
        foreach(CCSPlayerController player in Lib.GetPlayers())
        {
            if(player.PlayerName == name)
            {
                return player.Slot;
            }
        }

        return -1;
    }

    static public CCSPlayerPawn? Pawn(this CCSPlayerController? player)
    {
        if(!player.IsLegalAlive())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;

        return pawn;
    }

    static public void SetHealth(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null)
        {
            pawn.Health = hp;
            Utilities.SetStateChanged(pawn,"CBaseEntity","m_iHealth");
        }
    }

    static public int GetHealth(this CCSPlayerController? player)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn == null)
        {
            return 0;
        }

        return pawn.Health;
    }

    static public void Freeze(this CCSPlayerController? player)
    {
        player.SetMoveType(MoveType_t.MOVETYPE_NONE);
    }

    static public void UnFreeze(this CCSPlayerController? player)
    {
        player.SetMoveType(MoveType_t.MOVETYPE_WALK);
    }

    static public void SetMoveType(this CCSPlayerController? player, MoveType_t type)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null)
        {
            pawn.MoveType = type;
        }
    }

    static public void SetGravity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null)
        {
            pawn.GravityScale = value;
        }
    }

    static public void SetVelocity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null)
        {
            pawn.VelocityModifier = value;
        }
    }


    static public void SetArmour(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null)
        {
            pawn.ArmorValue = hp;
        }
    }

    static public void StripWeapons(this CCSPlayerController? player, bool removeKnife = false)
    {
        // only care if player is valid
        if(!player.IsLegalAlive())
        {
            return;
        }

        player.RemoveWeapons();
        
        // dont remove knife its buggy
        if(!removeKnife)
        {
            player.GiveWeapon("knife");
        }
    }

    static public void SetColour(this CCSPlayerController? player, Color colour)
    {
        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn != null && player.IsLegalAlive())
        {
            pawn.RenderMode = RenderMode_t.kRenderTransColor;
            pawn.Render = colour;
            Utilities.SetStateChanged(pawn,"CBaseModelEntity","m_clrRender");
        }
    }

    static public bool IsVip(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/vip"});
    }


    static public bool IsGenericAdmin(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/generic"});
    }

    static public void PlaySound(this CCSPlayerController? player, String sound)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.ExecuteClientCommand($"play {sound}");
    }


    // NOTE: i dont think we call this in the right context
    // OnPostThink doesn't appear to be good enough?
    static public void HideWeapon(this CCSPlayerController? player)
    {
        Server.PrintToChatAll("Hide weapon uimpl");
    }

    static public void ListenAll(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.VoiceFlags |= VoiceFlags.ListenAll;
        player.VoiceFlags &= ~VoiceFlags.ListenTeam;
    }

    static public void ListenTeam(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.ListenAll;
        player.VoiceFlags |= VoiceFlags.ListenTeam;
    }

    static public void Mute(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        // admins cannot be muted by the plugin
        if(!player.IsGenericAdmin())
        {
            player.VoiceFlags |= VoiceFlags.Muted;
        }
    }

    // TODO: this needs to be hooked into the ban system that becomes used
    static public void UnMute(this CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.Muted;
    }


    public static void RestoreHP(this CCSPlayerController? player, int damage, int health)
    {
        if(!player.IsLegal())
        {
            return;
        }

        // TODO: why does this sometimes mess up?
        if(health < 100)
        {
            player.SetHealth(Math.Min(health + damage,100));
        }

        else
        {
            player.SetHealth(health + damage);
        }
    }

    public static Vector? EyeVector(this CCSPlayerController? player)
    {
        var pawn = player.Pawn();

        if(pawn == null)
        {
            return null;
        }

        QAngle eyeAngle = pawn.EyeAngles;

        // convert angles to rad 
        double pitch = (Math.PI/180) * eyeAngle.X;
        double yaw = (Math.PI/180) * eyeAngle.Y;

        // get direction vector from angles
        Vector eyeVector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)),(float)(Math.Sin(yaw) * Math.Cos(pitch)),(float)(-Math.Sin(pitch)));

        return eyeVector;
    }

    public static void RespawnCallback(int slot)
    {
        var player = Utilities.GetPlayerFromSlot(slot);

        if(player.IsLegal())
        {
            player.Respawn();
        }
    }

    static public void Nuke()
    {
        foreach(CCSPlayerController target in Lib.GetPlayers())
        {
            target.Slay();
        }  
    }

}