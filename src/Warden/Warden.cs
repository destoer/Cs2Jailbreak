

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
        for(int p = 0; p < jail_players.Length; p++)
        {
            jail_players[p] = new JailPlayer();
        }
    }

    // Give a player warden
    public void set_warden(int? new_slot_opt)
    {
        if(new_slot_opt == null)
        {
            return;
        }

        warden_slot = new_slot_opt.Value;

        var player = Utilities.GetPlayerFromSlot(warden_slot);

        // one last saftey check
        if(!player.is_valid())
        {
            warden_slot = INAVLID_SLOT;
            return;
        }

        Chat.localize_announce(WARDEN_PREFIX,"warden.took_warden",player.PlayerName);

        player.localize_announce(WARDEN_PREFIX,"warden.wcommand");

        warden_timestamp = Lib.cur_timestamp();

        // change player color!
        player.set_colour(Color.FromArgb(255, 0, 0, 255));

        JailPlugin.logs.AddLocalized("warden.took_warden", player.PlayerName);
    }

    public bool is_warden(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return false;
        }

        return player.Slot == warden_slot;
    }

    public void remove_warden_internal()
    {
        warden_slot = INAVLID_SLOT;
        warden_timestamp = -1;
    }

    public void remove_warden()
    {
        var player = Utilities.GetPlayerFromSlot(warden_slot);

        if(player.is_valid())
        {
            player.set_colour(Player.DEFAULT_COLOUR);
            Chat.localize_announce(WARDEN_PREFIX,"warden.removed",player.PlayerName);
            JailPlugin.logs.AddLocalized("warden.removed", player.PlayerName);
        }

        remove_warden_internal();
    }

    public void remove_if_warden(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(is_warden(player))
        {
            remove_warden();
        }
    }


    // reset variables for a new round
    void purge_round()
    {
        remove_laser();

        if(config.warden_force_removal)
        {
            remove_warden_internal();
        }

        // reset player structs
        foreach(JailPlayer jail_player in jail_players)
        {
            jail_player.purge_round();
        }
    }

    void set_warden_if_last(bool on_death = false)
    {
        // dont override the warden if there is no death removal
        if(!config.warden_force_removal)
        {
            return;
        }

        // if there is only one ct automatically give them warden!
        var ct_players = Lib.get_alive_ct();

        if(ct_players.Count == 1)
        {
            if(on_death)
            {
                // play sfx for last ct
                // TODO: this is too loud as there is no way to control volume..
                //Lib.play_sound_all("sounds/vo/agents/sas/lastmanstanding03");
            }
        
            int slot = ct_players[0].Slot;
            set_warden(slot);
        }
    }

    public void setup_player_guns(CCSPlayerController? player)
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        // strip weapons just in case
        if(config.strip_spawn_weapons)
        {
            player.strip_weapons();
        }

        if(player.is_ct())
        {
            if(config.ct_guns)
            {
                var jail_player = jail_player_from_player(player);

                player.give_weapon("deagle");

                if(jail_player != null)
                {
                    player.give_menu_weapon(jail_player.ct_gun);
                }
            }

            if(config.ct_armour)
            {  
                player.give_armour();
            }
        } 
    }

    // util func to get a jail player
    public JailPlayer? jail_player_from_player(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return null;
        }

        return jail_players[player.Slot];
    }
    
    const int INAVLID_SLOT = -3;   

    int warden_slot = INAVLID_SLOT;
    
    public static readonly String WARDEN_PREFIX = $" {ChatColors.Green}[WARDEN]: {ChatColors.White}";

    long warden_timestamp = -1;

    public JailConfig config = new JailConfig();

    public JailPlayer[] jail_players = new JailPlayer[64];

    // slot for player for waden colour
    int colour_slot = -1;

    public Warday warday = new Warday();
    public Block block = new Block();
    public Mute mute = new Mute();
};