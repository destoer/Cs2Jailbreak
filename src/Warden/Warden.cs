

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

public class Warden
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
            Lib.localise_announce(WARDEN_PREFIX,"warden.removed",player.PlayerName);
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
        Lib.localise_announce(WARDEN_PREFIX,"warden.remove");
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
        if(player == null || !player.is_valid())
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
        if(player == null || !player.is_valid())
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

        player.localise("warden.warden_command_desc");
        player.localise("warden.warday_command_desc");
        player.localise("warden.unwarden_command_desc");
        player.localise("warden.block_command_desc");
        player.localise("warden.unblock_command_desc");
        player.localise("warden.remove_warden_command_desc");
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
        setup_cvar();
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
        Server.ExecuteCommand("mp_equipment_reset_rounds 1");
        Server.ExecuteCommand("mp_t_default_secondary \"\" ");
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

        // cvars take care of this for us now
        // player.strip_weapons();

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
    
    public void ping(CCSPlayerController? player, float x, float y, float z)
    {
        // draw marker
        if(is_warden(player) && player != null && player.is_valid())
        {
            Vector start = new Vector(x,y,z);

            // line up
            Vector end = new Vector(x,y,z + 61.0f);
            Lib.draw_laser(start,end,20.0f,2.0f,Lib.CYAN);

            // draw cross
            start = new Vector(x,y - 50.0f,z + 3.0f);
            end = new Vector(x,y + 50.0f,z + 3.0f);
            Lib.draw_laser(start,end,20.0f,2.0f,Lib.CYAN);

            start = new Vector(x - 50.0f,y,z + 3.0f);
            end = new Vector(x + 50.0f,y,z + 3.0f);
            Lib.draw_laser(start,end,20.0f,2.0f,Lib.CYAN);
        }
    }

    public void laser_tick()
    {
        if(warden_slot == INAVLID_SLOT)
        {
            return;
        }

        CCSPlayerController? warden = Utilities.GetPlayerFromSlot(warden_slot);

        if(warden == null || !warden.is_valid())
        {
            return;
        }

        bool use_key = (warden.Buttons & PlayerButtons.Use) == PlayerButtons.Use;

        if(!use_key)
        {
            return;
        }

        CCSPlayerPawn? pawn = warden.pawn();

        if(pawn != null && pawn.AbsOrigin != null)
        {
            // Ideally we would use Get Client Eye posistion
            // because this will break when we crouch etc
            Vector eye = new Vector(pawn.AbsOrigin.X,pawn.AbsOrigin.Y,pawn.AbsOrigin.Z + 61.0f);

            Vector end = new Vector(eye.X,eye.Y,eye.Z);

            QAngle eye_angle = pawn.EyeAngles;

            // convert angles to rad 
            double pitch = (Math.PI/180) * eye_angle.X;
            double yaw = (Math.PI/180) * eye_angle.Y;

            // get direction vector from angles
            Vector eye_vector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)),(float)(Math.Sin(yaw) * Math.Cos(pitch)),(float)(-Math.Sin(pitch)));

            int t = 3000;

            end.X += (t * eye_vector.X);
            end.Y += (t * eye_vector.Y);
            end.Z += (t * eye_vector.Z);

            /*
                warden.PrintToChat($"end: {end.X} {end.Y} {end.Z}");
                warden.PrintToChat($"angle: {eye_angle.X} {eye_angle.Y}");
            */

            Lib.draw_laser(eye,end,LASER_TIME + 0.05f,2.0f,Lib.CYAN);
        }
    }

    public static readonly float LASER_TIME = (float)(1.0 / 30.0);

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