

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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public class Warden
{
    public Warden()
    {
        for(int p = 0; p < jail_players.Length; p++)
        {
            jail_players[p] = new JailPlayer();
        }
    }

    void announce(String message)
    {
        Lib.announce(WARDEN_PREFIX,message);
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
        

        announce($"{player.PlayerName} is now the warden");

        player.announce(WARDEN_PREFIX,"Type !wcommands to see a full list of warden commands");

        // change player color!
        player.set_colour(Color.FromArgb(255, 0, 0, 255));
    }

    public bool is_warden(CCSPlayerController? player)
    {
        return player.slot() == warden_slot;
    }

    public void remove_warden()
    {
        var player = Utilities.GetPlayerFromSlot(warden_slot);

        if(player.is_valid())
        {
            player.set_colour(Color.FromArgb(255, 255, 255, 255));
            announce($"{player.PlayerName} is no longer the warden");
        }

        warden_slot = INAVLID_SLOT;
    }

    public void remove_if_warden(CCSPlayerController? player)
    {
        if(!player.is_valid() || player == null)
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

    [RequiresPermissions("@css/generic")]
    public void remove_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        announce("Warden removed");
        remove_warden();
    }

    public void warday_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to call a warday");
            return;
        }

        // must specify location
        if(command.ArgCount != 2)
        {
            player.PrintToChat($"{WARDEN_PREFIX}Usage !wd <location>");
            return;
        }

        // attempt the start the warday
        String location = command.ArgByIndex(1);

        if(!warday.start_warday(location))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You cannot call a warday for another {Warday.ROUND_LIMIT - warday.round_counter} rounds");
        }
    }

      
    public void wub_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to use wub");
            return;
        }

        block.unblock_all();
    }

    public void wb_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be warden
        if(!is_warden(player))
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a warden to use wb");
            return;
        }

        block.block_all();
    }

    // debug command
    [RequiresPermissions("@jail/debug")]
    public void is_rebel_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
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

            int? slot = player.slot();

            if(slot != null)
            {
                invoke.PrintToConsole($"{jail_players[slot.Value].is_rebel} : {player.PlayerName}\n");
            }
        }
    }

    public void cmd_info(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PrintToChat("!w - take warden");
        player.PrintToChat("!wd - start a warday");
        player.PrintToChat("!uw - leave warden");
        player.PrintToChat("!wb - enable block");
        player.PrintToChat("!wub - disable block");
        player.PrintToChat("!rw - admin remove warden");
    }

    public void take_warden_cmd(CCSPlayerController? player, CommandInfo command)
    {
        // invalid player we dont care
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // player must be alive
        if(!player.PawnIsAlive)
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be alive to warden");
        }        

        // check team is valid
        else if(!player.is_ct())
        {
            player.PrintToChat($"{WARDEN_PREFIX}You must be a CT to warden");
        }

        // check there is no warden
        else if(warden_slot != INAVLID_SLOT)
        {
            var warden = Utilities.GetPlayerFromSlot(warden_slot);

            player.PrintToChat($"{WARDEN_PREFIX}{warden.PlayerName} is allready a warden");
        }

        // player is valid to take warden
        else
        {
            set_warden(player.slot());
        }
    }

    // reset variables for a new round
    void purge_round()
    {
        warden_slot = INAVLID_SLOT;

        // reset player structs
        foreach(JailPlayer jail_player in jail_players)
        {
            jail_player.purge_round();
        }
    }



    public void map_start()
    {
        warday.map_start();
    }

    void set_warden_if_last()
    {
        // if there is only one ct automatically give them warden!
        var ct_players = Lib.get_alive_ct();

        if(ct_players.Count == 1)
        {
            int? slot = ct_players[0].slot();
        
            set_warden(slot);
        }
    }

    void round_timer_callback()
    {
        start_timer = null;   
    }

    void setup_cvar()
    {
        Server.ExecuteCommand("mp_force_pick_time 3000");
        Server.ExecuteCommand("mp_autoteambalance 0");

    }

    public void round_start()
    {
        setup_cvar();

        purge_round();

        if(JailPlugin.global_ctx != null)
        {
            start_timer = JailPlugin.global_ctx.AddTimer(20.0F,round_timer_callback,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }

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
        Lib.kill_timer(ref start_timer);
        mute.round_end();
        warday.round_end();
        purge_round();
    }


    public void connect(CCSPlayerController? player)
    {
        var slot = player.slot();

        if(slot != null)
        {
            jail_players[slot.Value].reset();
        }

        mute.connect(player);
    }

    public void disconnect(CCSPlayerController? player)
    {
        remove_if_warden(player);
    }

    public void setup_player_guns(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        var jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            player.strip_weapons();

            if(player.is_ct())
            {
                if(config.ct_guns)
                {
                    player.GiveNamedItem("weapon_deagle");
                    player.GiveNamedItem("weapon_m4a1");
                }

                if(config.ct_armour)
                {  
                    player.GiveNamedItem("item_assaultsuit");
                }
            }
        }
    }

    public void voice(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        if(!config.warden_on_voice)
        {
            return;
        }

        if(warden_slot == INAVLID_SLOT && player.is_ct())
        {
            set_warden(player.slot());
        }
    }

    public void spawn(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid_alive())
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

    public void death(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // player is no longer on server
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // handle warden death
        remove_if_warden(player);

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
            set_warden_if_last();
        }
    }

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
            invoke.PrintToChat("Expected usage: !swap_guard <player name>");
            return;
        }

        var target = command.GetArgTargetResult(1);

        foreach(CCSPlayerController player in target)
        {
            if(player.is_valid())
            {
                invoke.PrintToChat($"swapped: {player.PlayerName}");
                player.SwitchTeam(CsTeam.CounterTerrorist);
            }
        }
    }


    public void ct_guns(CCSPlayerController player, ChatMenuOption option)
    {
        if(player == null || !player.is_valid_alive() || !player.is_ct()) 
        {
            return;
        }

        player.strip_weapons();


        player.GiveNamedItem("weapon_" + Lib.gun_give_name(option.Text));
        player.GiveNamedItem("weapon_deagle");

        if(config.ct_armour)
        {
            player.GiveNamedItem("item_assaultsuit");
        }
    }

    public void cmd_ct_guns(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        if(!player.is_ct())
        {
            player.announce(WARDEN_PREFIX,"You must be a ct to use the gun menu!");
            return;
        }

        if(!config.ct_gun_menu)
        {
            player.announce(WARDEN_PREFIX,"Gun menu is disabled!");
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
    JailPlayer? jail_player_from_player(CCSPlayerController? player)
    {
        if(!player.is_valid() || player == null)
        {
            return null;
        }

        var slot = player.slot();

        if(slot == null)
        {
            return null;
        }

        return jail_players[slot.Value];
    }

    const int INAVLID_SLOT = -3;   

    int warden_slot = INAVLID_SLOT;

    public static readonly String WARDEN_PREFIX = $" {ChatColors.Green}[WARDEN]: {ChatColors.White}";


    CSTimer.Timer? start_timer = null;

    public JailConfig config = new JailConfig();

    JailPlayer[] jail_players = new JailPlayer[64];

    public Warday warday = new Warday();
    public Block block = new Block();
    public Mute mute = new Mute();
};