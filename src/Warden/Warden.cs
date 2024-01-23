

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

        Lib.localise_announce(WARDEN_PREFIX,"warden.took_warden",player.PlayerName);

        player.localise_announce(WARDEN_PREFIX,"warden.wcommand");

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
            player.set_colour(Color.FromArgb(255, 255, 255, 255));
            Lib.localise_announce(WARDEN_PREFIX,"warden.removed",player.PlayerName);
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

    public void leave_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        remove_if_warden(player);
    }

    public void remove_marker_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(is_warden(player))
        {
            player.announce(WARDEN_PREFIX,"Marker removed");
            remove_marker();
        }
    }

    [RequiresPermissions("@css/generic")]
    public void remove_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        Lib.localise_announce(WARDEN_PREFIX,"warden.remove");
        remove_warden();
    }

    [RequiresPermissions("@css/generic")]
    public void force_open_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.force_open();
    }


    [RequiresPermissions("@css/generic")]
    public void force_close_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.force_close();
    }


    public void warday_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_restrict");
            return;
        }

        // must specify location
        if(command.ArgCount < 2)
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_usage");
            return;
        }

        // attempt the start the warday
        String location = command.ArgByIndex(1);

        // attempt to parse optional delay
        int delay = 20;

        if(command.ArgCount >= 3)
        {
            if(Int32.TryParse(command.ArgByIndex(2),out int delay_opt))
            {
                delay = delay_opt;
            }       
        }

        if(!warday.start_warday(location,delay))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_round_restrict",Warday.ROUND_LIMIT - warday.round_counter);
        }
    }

      
    public void wub_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.wub_restrict");
            return;
        }

        block.unblock_all();
    }

    public void wb_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.wb_restrict");
            return;
        }

        block.block_all();
    }

    // debug command
    [RequiresPermissions("@jail/debug")]
    public void is_rebel_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        invoke.PrintToConsole("rebels\n");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            invoke.PrintToConsole($"{jail_players[player.Slot].is_rebel} : {player.PlayerName}\n");
        }
    }

    public void warden_time_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        if(warden_slot == INAVLID_SLOT)
        {
            invoke.localise_prefix(WARDEN_PREFIX,"warden.no_warden");
            return;
        }

        long elasped_min = (Lib.cur_timestamp() - warden_timestamp) / 60;

        invoke.localise_prefix(WARDEN_PREFIX,"warden.time",elasped_min);
    }

    public void cmd_info(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.localise("warden.warden_command_desc");
        player.localise("warden.warday_command_desc");
        player.localise("warden.unwarden_command_desc");
        player.localise("warden.block_command_desc");
        player.localise("warden.unblock_command_desc");
        player.localise("warden.remove_warden_command_desc");
        player.localise("warden.laser_colour_command_desc");
        player.localise("warden.marker_colour_command_desc");
        player.localise("warden.wsd_command_desc");
        player.localise("warden.wsd_ff_command_desc");
    }

    public void take_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        // invalid player we dont care
        if(!player.is_valid())
        {
            return;
        }

        // player must be alive
        if(!player.PawnIsAlive)
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warden_req_alive");
        }        

        // check team is valid
        else if(!player.is_ct())
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warden_req_ct");
        }

        // check there is no warden
        else if(warden_slot != INAVLID_SLOT)
        {
            var warden = Utilities.GetPlayerFromSlot(warden_slot);

            player.localise_prefix(WARDEN_PREFIX,"warden.warden_taken",warden.PlayerName);
        }

        // player is valid to take warden
        else
        {
            set_warden(player.Slot);
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



    public void map_start()
    {
        setup_cvar();
        warday.map_start();
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

    void setup_cvar()
    {
        Server.ExecuteCommand("mp_force_pick_time 3000");
        Server.ExecuteCommand("mp_autoteambalance 0");

        if(config.strip_spawn_weapons)
        {
            Server.ExecuteCommand("mp_equipment_reset_rounds 1");
            Server.ExecuteCommand("mp_t_default_secondary \"\" ");
            Server.ExecuteCommand("mp_ct_default_secondary \"\" ");
        }
    }

    public void round_start()
    {
        setup_cvar();

        purge_round();

        // handle submodules
        mute.round_start();
        block.round_start();
        warday.round_start();

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.set_colour(Color.FromArgb(255, 255, 255, 255));
        }

        set_warden_if_last();
    }

    public void round_end()
    {
        mute.round_end();
        warday.round_end();
        purge_round();
    }


    public void connect(CCSPlayerController? player)
    {
        if(player != null)
        {
            jail_players[player.Slot].reset();
        }

        mute.connect(player);
    }

    public void disconnect(CCSPlayerController? player)
    {
        remove_if_warden(player);
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

                player.GiveNamedItem("weapon_deagle");

                if(jail_player != null)
                {
                    player.GiveNamedItem(Lib.gun_give_name(jail_player.ct_gun));
                }
            }

            if(config.ct_armour)
            {  
                player.GiveNamedItem("item_assaultsuit");
            }
        } 
    }

    public void voice(CCSPlayerController? player)
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        if(!config.warden_on_voice)
        {
            return;
        }

        if(warden_slot == INAVLID_SLOT && player.is_ct())
        {
            set_warden(player.Slot);
        }
    }

    public void spawn(CCSPlayerController? player)
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        setup_player_guns(player);

        mute.spawn(player);
    }   

    public void switch_team(CCSPlayerController? player,int new_team)
    {
        remove_if_warden(player);
        mute.switch_team(player,new_team);
    }

    // warden death has occured
    public void warden_death()
    {
        remove_warden();
    }

    [RequiresPermissions("@css/generic")]
    public void fire_guard_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.localise_announce(WARDEN_PREFIX,"warden.fire_guard");

        // swap every guard apart from warden to T
        List<CCSPlayerController> players = Utilities.GetPlayers();
        var valid = players.FindAll(player => player.is_valid() && player.is_ct() && !is_warden(player));

        foreach(var player in valid)
        {
            player.slay();
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    public void death(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // player is no longer on server
        if(!player.is_valid())
        {
            return;
        }

        if(config.warden_force_removal)
        {
            // handle warden death
            remove_if_warden(player);
        }

        // mute player
        mute.death(player);

        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.rebel_death(player,killer);
        }

        // if a t dies we dont need to regive the warden
        if(player.is_ct())
        {
            set_warden_if_last(true);
        }
    }

    public void ct_guns(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid_alive() || !player.is_ct()) 
        {
            return;
        }

        player.strip_weapons();

   
        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.update_player(player, "ct_gun", option.Text);
            jail_player.ct_gun = option.Text;
        }

        player.GiveNamedItem(Lib.gun_give_name(option.Text));
        player.GiveNamedItem("weapon_deagle");

        if(config.ct_armour)
        {
            player.GiveNamedItem("item_assaultsuit");
        }
    }

    public void cmd_ct_guns(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(!player.is_ct())
        {
            player.localise_announce(WARDEN_PREFIX,"warden.ct_gun_menu");
            return;
        }

        if(!config.ct_gun_menu)
        {
            player.localise_announce(WARDEN_PREFIX,"warden.gun_menu_disabled");
            return;
        }

        player.gun_menu_internal(true,ct_guns);     
    }

    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health)
    {
        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {  
            jail_player.player_hurt(player,attacker,damage, health);
        }  
    }

    public void weapon_fire(CCSPlayerController? player, String name)
    {
        // attempt to set rebel
        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.rebel_weapon_fire(player,name);
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

    public Warday warday = new Warday();
    public Block block = new Block();
    public Mute mute = new Mute();
};