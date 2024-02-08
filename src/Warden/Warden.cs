

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
    public Warden()
    {
        for(int p = 0; p < jailPlayers.Length; p++)
        {
            jailPlayers[p] = new JailPlayer();
        }
    }

    // Give a player warden
    public void SetWarden(int slot)
    {
        wardenSlot = slot;

        var player = Utilities.GetPlayerFromSlot(wardenSlot);

        // one last saftey check
        if(!player.IsLegal())
        {
            wardenSlot = INAVLID_SLOT;
            return;
        }

        Chat.LocalizeAnnounce(WARDEN_PREFIX,"warden.took_warden",player.PlayerName);

        player.LocalizeAnnounce(WARDEN_PREFIX,"warden.wcommand");

        wardenTimestamp = Lib.CurTimestamp();

        // change player color!
        player.SetColour(Color.FromArgb(255, 0, 0, 255));

        JailPlugin.logs.AddLocalized("warden.took_warden", player.PlayerName);
    }

    public bool IsWarden(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        return player.Slot == wardenSlot;
    }

    void RemoveWardenInternal()
    {
        wardenSlot = INAVLID_SLOT;
        wardenTimestamp = -1;
    }

    public void RemoveWarden()
    {
        var player = Utilities.GetPlayerFromSlot(wardenSlot);

        if(player.IsLegal())
        {
            player.SetColour(Player.DEFAULT_COLOUR);
            Chat.LocalizeAnnounce(WARDEN_PREFIX,"warden.removed",player.PlayerName);
            JailPlugin.logs.AddLocalized("warden.removed", player.PlayerName);
        }

        RemoveWardenInternal();
    }

    public void RemoveIfWarden(CCSPlayerController? player)
    {
        if(IsWarden(player))
        {
            RemoveWarden();
        }
    }


    // reset variables for a new round
    void PurgeRound()
    {
        RemoveLaser();

        if(Config.wardenForceRemoval)
        {
            RemoveWardenInternal();
        }

        // reset player structs
        foreach(JailPlayer jailPlayer in jailPlayers)
        {
            jailPlayer.PurgeRound();
        }
    }

    void SetWardenIfLast(bool onDeath = false)
    {
        // dont override the warden if there is no death removal
        if(!Config.wardenForceRemoval)
        {
            return;
        }

        // if there is only one ct automatically give them warden!
        var ctPlayers = Lib.GetAliveCt();

        if(ctPlayers.Count == 1)
        {
            if(onDeath)
            {
                // play sfx for last ct
                // TODO: this is too loud as there is no way to control volume..
                //Lib.PlaySound_all("sounds/vo/agents/sas/lastmanstanding03");
            }
        
            int slot = ctPlayers[0].Slot;
            SetWarden(slot);
        }
    }

    public void SetupPlayerGuns(CCSPlayerController? player)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        // strip weapons just in case
        if(Config.stripSpawnWeapons)
        {
            player.StripWeapons();
        }

        if(player.IsCt())
        {
            if(Config.ctGuns)
            {
                var jailPlayer = JailPlayerFromPlayer(player);

                player.GiveWeapon("deagle");

                if(jailPlayer != null)
                {
                    player.GiveMenuWeapon(jailPlayer.ctGun);
                }
            }

            if(Config.ctArmour)
            {  
                player.GiveArmour();
            }
        } 
    }

    // util func to get a jail player
    public JailPlayer? JailPlayerFromPlayer(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return null;
        }

        return jailPlayers[player.Slot];
    }
    
    const int INAVLID_SLOT = -3;   

    int wardenSlot = INAVLID_SLOT;
    
    public static String WARDEN_PREFIX = $" {ChatColors.Green}[WARDEN]: {ChatColors.White}";

    long wardenTimestamp = -1;

    public JailConfig Config = new JailConfig();

    public JailPlayer[] jailPlayers = new JailPlayer[64];

    // slot for player for warden colour
    int colourSlot = -1;

    bool ctHandicap = false;

    public Warday warday = new Warday();
    public Block block = new Block();
    public Mute mute = new Mute();
};