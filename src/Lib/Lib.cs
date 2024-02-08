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


public static class Lib
{
    static public bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    static public void InvokePlayerMenu(CCSPlayerController? invoke, String name,
        Action<CCSPlayerController, ChatMenuOption> callback, Func<CCSPlayerController?,bool> filter)
    {
        if(!invoke.IsLegal())
        {
            return;
        }

        var menu = new ChatMenu(name);

        foreach(var player in Lib.GetPlayers())
        {
            if(filter(player))
            {
                menu.AddMenuOption(player.PlayerName, callback);
            }
        }

        MenuManager.OpenChatMenu(invoke, menu); 
    }

    public static void ColourMenu(CCSPlayerController? player,Action<CCSPlayerController, ChatMenuOption> callback, String name)
    {
        if(!player.IsLegal())
        {
            return;
        }

        var colourMenu = new ChatMenu(name);

        foreach(var item in Lib.COLOUR_CONFIG_MAP)
        {
            colourMenu.AddMenuOption(item.Key, callback);
        }

        MenuManager.OpenChatMenu(player, colourMenu);    
    }

    static public void PlaySoundAll(String sound)
    {
        foreach(CCSPlayerController? player in Lib.GetPlayers())
        {
            player.PlaySound(sound);
        }
    }

    static public void MuteT()
    {
        foreach(CCSPlayerController player in Lib.GetPlayers())
        {
            if(player.IsT())
            {
                player.Mute();
            }
        }
    }

    static public void KillTimer(ref CSTimer.Timer? timer)
    {
        if(timer != null)
        {
            timer.Kill();
            timer = null;
        }
    }

    static public void UnMuteAll()
    {
        foreach(CCSPlayerController player in Lib.GetPlayers())
        {
            player.UnMute();
        }
    }

    static public long CurTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeSeconds();
    }


    static public void EnableFriendlyFire()
    {
        if(ff != null)
        {
            ff.SetValue(true);
        }
    }

    static public void DisableFriendlyFire()
    {
        if(ff != null)
        {
            ff.SetValue(false);
        }
    }

    static public void SwapAllT()
    {
        foreach(var player in GetAlivePlayers())
        {
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    static public void RespawnPlayers()
    {
        // 1up all dead players
        foreach(CCSPlayerController player in Lib.GetPlayers())
        {
            if(!player.IsLegalAlive())
            {
                player.Respawn();
            }
        }
    }

    static public List<CCSPlayerController> GetAlivePlayers()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.IsLegalAlive());      
    }

    static public List<CCSPlayerController> GetPlayers()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.IsLegal() && player.IsConnected());      
    }

    static public List<CCSPlayerController> GetAliveCt()
    {
        List<CCSPlayerController> players = Lib.GetPlayers();
        return players.FindAll(player => player.IsLegalAlive() && player.IsCt());
    }

    static public int CtCount()
    {
        List<CCSPlayerController> players = Lib.GetPlayers();
        return players.FindAll(player => player.IsLegal() && player.IsCt()).Count;        
    }

    static public int TCount()
    {
        List<CCSPlayerController> players = Lib.GetPlayers();
        return players.FindAll(player => player.IsLegal() && player.IsT()).Count;        
    }

    static public int AliveCtCount()
    {
        return GetAliveCt().Count;
    }

    static public List<CCSPlayerController> GetAliveT()
    {
        List<CCSPlayerController> players = Lib.GetPlayers();
        return players.FindAll(player => player.IsLegalAlive() && player.IsT());;
    }

    static public int AliveTCount()
    {
        return GetAliveT().Count;
    }

    static public bool BlockEnabled()
    {
        if(blockCvar != null)
        {
            return blockCvar.GetPrimitiveValue<int>() == 1;
        }

        return true;
    }

    static public void BlockAll()
    {
        if(blockCvar != null)
        {
            blockCvar.SetValue(1);
        }
    }

    static public void UnBlockAll()
    {
        if(blockCvar != null)
        {
            blockCvar.SetValue(0);
        }
    }

    
    static public void SetCvarStr(String name, String value)
    {
        // why doesn't this work lol
        
        ConVar? cvar = ConVar.Find(name);

        if(cvar != null)
        {
            cvar.StringValue = value;
        }
    }

    static public bool IsActiveTeam(int team)
    {
        return (team == Player.TEAM_T || team == Player.TEAM_CT);
    }

    // TODO: just go with a simple print for now
    static public void log(String str)
    {
        Console.WriteLine($"[JAILBREAK]: {str}");
    }


    public static readonly Color CYAN = Color.FromArgb(255, 153, 255, 255);
    public static readonly Color RED = Color.FromArgb(255, 255, 0, 0);
    public static readonly Color INVIS = Color.FromArgb(0, 255, 255, 255);
    public static readonly Color GREEN = Color.FromArgb(255,0, 191, 0);

    public static readonly Dictionary<string,Color> COLOUR_CONFIG_MAP = new Dictionary<string,Color>()
    {
        {"Cyan",Lib.CYAN}, // cyan
        {"Pink",Color.FromArgb(255,255,192,203)} , // pink
        {"Red",Lib.RED}, // red
        {"Purple",Color.FromArgb(255,118, 9, 186)}, // purple
        {"Grey",Color.FromArgb(255,66, 66, 66)}, // grey
        {"Green",GREEN}, // green
        {"Yellow",Color.FromArgb(255,255, 255, 0)} // yellow
    };

    static ConVar? blockCvar = ConVar.Find("mp_solid_teammates");
    static ConVar? ff = ConVar.Find("mp_teammates_are_enemies");

    public const int HITGROUP_HEAD = 0x1;
}