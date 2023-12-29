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


// NOTE: this is a timer wrapper, and should be owned the class
// wanting to use the timer
public class Countdown<T>
{
    public void start(String countdown_name, int countdown_delay,
        T countdown_data,Action<T,int>? countdown_print_func, Action <T> countdown_callback)
    {
        this.delay = countdown_delay;
        this.callback = countdown_callback;
        this.name = countdown_name;
        this.data = countdown_data;
        this.print_func = countdown_print_func;


        if(JailPlugin.global_ctx != null)
        {
            this.handle = JailPlugin.global_ctx.AddTimer(1.0f,countdown,CSTimer.TimerFlags.STOP_ON_MAPCHANGE | CSTimer.TimerFlags.REPEAT);
        }
    }

    public void kill()
    {
       Lib.kill_timer(ref handle);
    }

    void countdown()
    {
        delay -= 1;

        // countdown over
        if(delay <= 0)
        {
            // kill the timer
            // and then call the callback
            kill();

            if(callback != null && data != null)
            {
                callback(data);
            }
        }

        // countdown still active
        else
        {
            // custom print
            if(print_func != null && data != null)
            {
                print_func(data,delay);
            }

            // default print
            else
            {
                Lib.print_centre_all($"{name} is starting in {delay} seconds");
            }
        }
    }


    public int delay = 0;
    public Action<T>? callback = null;
    public String name = "";
    public Action<T,int>? print_func = null;
    CSTimer.Timer? handle = null;

    // callback data
    T? data = default(T);
}

    

public static class Lib
{
    // TODO: i dont think there is a builtin func for this...
    static public void print_centre_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            player.PrintToCenter(str);
        }
    }

    static public void print_console_all(String str, bool admin_only = false)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            if(admin_only && !player.is_generic_admin())
            {
                return;
            }

            player.PrintToConsole(str);
        }
    }

    static public void slay(this CCSPlayerController? player)
    {
        if(player != null && player.is_valid_alive())
        {
            player.PlayerPawn.Value?.CommitSuicide(true, true);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool is_valid(this CCSPlayerController? player)
    {
        return player != null && player.IsValid &&  player.PlayerPawn.IsValid && player.PlayerPawn.Value?.IsValid == true;
    }

    static public bool is_t(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_T;
    }

    static public bool is_ct(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_CT;
    }

    // yes i know the null check is redundant but C# is dumb
    static public bool is_valid_alive(this CCSPlayerController? player)
    {
        return player != null && player.is_valid() && player.PawnIsAlive && player.get_health() > 0;
    }

    static public CCSPlayerPawn? pawn(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
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

    static public bool is_windows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
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

    static public void give_event_nade_delay(CCSPlayerController? target,float delay, String name)
    {
        if(JailPlugin.global_ctx == null)
        {
            return;
        }

        int? slot = target.slot();

        JailPlugin.global_ctx.AddTimer(delay,() => 
        {
            if(slot != null)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot.Value);

                if(player != null && player.is_valid_alive())
                {
                    //Server.PrintToChatAll("give nade");
                    player.strip_weapons(true);
                    player.GiveNamedItem(name);
                }
            }
        });
    }

    static public void remove_ent(int index, String name)
    {
        CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>(index);

        if(ent != null && ent.DesignerName == name)
        {
            ent.Remove();
        }
    }

    static public void remove_ent_delay(CEntityInstance entity, float delay, String name)
    {
        // remove projectile
        if(entity.DesignerName == name)
        {
            int index = (int)entity.Index;

            if(JailPlugin.global_ctx != null)
            {
                JailPlugin.global_ctx.AddTimer(delay,() => 
                {
                    remove_ent(index,name);
                });
            }
        }
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
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        if(is_windows())
        {
           return; 
        }

        else
        {
            player.RemoveWeapons();
        }

        // dont remove knife its buggy
        if(!remove_knife)
        {
            player.GiveNamedItem("weapon_knife");
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
        if(player == null || !player.is_valid())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/generic"});
    }

    static Vector VEC_ZERO = new Vector(0.0f,0.0f,0.0f);
    static QAngle ANGLE_ZERO = new QAngle(0.0f,0.0f,0.0f);

    static public int draw_laser(Vector start, Vector end, float life, float width, Color color)
    {
        CEnvBeam? laser = Utilities.CreateEntityByName<CEnvBeam>("env_beam");

        if(laser == null)
        {
            return -1;
        }

        // setup looks
        laser.Render = color;
        laser.Width = 2.0f;

        // circle not working?
        //laser.Flags |= 8;

        // set pos
        laser.Teleport(start, ANGLE_ZERO, VEC_ZERO);

        // end pos
        // NOTE: we cant just move the whole vec
        laser.EndPos.X = end.X;
        laser.EndPos.Y = end.Y;
        laser.EndPos.Z = end.Z;

        // start spawn
        laser.DispatchSpawn(); 

        // create a timer to remove it
        if(life != 0.0f)
        {
            remove_ent_delay(laser,life,"env_beam");
        }

        return (int)laser.Index;
    }

    static public void play_sound(this CCSPlayerController? player, String sound)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.ExecuteClientCommand($"play {sound}");
    }

    static public CCSPlayerController? player(this CEntityInstance? instance)
    {
        if(instance == null)
        {
            return null;
        }

        // grab the pawn index
        int player_index = (int)instance.Index;

        // grab player controller from pawn
        CCSPlayerPawn? player_pawn =  Utilities.GetEntityFromIndex<CCSPlayerPawn>(player_index);

        // pawn valid
        if(player_pawn == null || !player_pawn.IsValid)
        {
            return null;
        }

        // controller valid
        if(player_pawn.OriginalController == null || !player_pawn.OriginalController.IsValid)
        {
            return null;
        }

        // any further validity is up to the caller
        return player_pawn.OriginalController.Value;
    }

    static public CCSPlayerController? player(this CHandle<CBaseEntity> handle)
    {
        if(handle.IsValid)
        {
            CBaseEntity? ent = handle.Value;

            if(ent != null)
            {
                return handle.Value.player();
            }
        }

        return null;
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
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.VoiceFlags |= VoiceFlags.ListenAll;
        player.VoiceFlags &= ~VoiceFlags.ListenTeam;
    }

    static public void listen_team(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.ListenAll;
        player.VoiceFlags |= VoiceFlags.ListenTeam;
    }

    static public void mute(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
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
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.VoiceFlags &= ~VoiceFlags.Muted;
    }

    static public void mute_t()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid() && player.is_t())
            {
                player.mute();
            }
        }
    }



    static public void kill_timer(ref CSTimer.Timer? timer)
    {
        if(timer != null)
        {
            timer.Kill();
            timer = null;
        }
    }

    static public void unmute_all()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid())
            {
                player.unmute();
            }
        }
    }

    static public bool is_valid(this CBasePlayerWeapon? weapon)
    {
        return weapon != null && weapon.IsValid;
    }

    static public CBasePlayerWeapon? find_weapon(this CCSPlayerController? player, String name)
    {
        // only care if player is alive
        if(!player.is_valid_alive())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.pawn();

        if(pawn == null)
        {
            return null;
        }

        var weapons = pawn.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return null;
        }

        foreach (var weapon_opt in weapons)
        {
            CBasePlayerWeapon? weapon = weapon_opt.Value;

            if(weapon == null)
            {
                continue;
            }
         
            if(weapon.DesignerName.Contains(name))
            {
                return weapon;
            }
        }

        return null;
    }

    static public void set_ammo(this CBasePlayerWeapon? weapon, int clip, int reserve)
    {
        if(weapon == null || !weapon.is_valid())
        {
            return;
        }

        weapon.Clip1 = clip;
        Utilities.SetStateChanged(weapon,"CBasePlayerWeapon","m_iClip1");
        weapon.ReserveAmmo[0] = reserve;
        Utilities.SetStateChanged(weapon,"CBasePlayerWeapon","m_pReserveAmmo");
    }

    public static void restore_hp(CCSPlayerController? player, int damage, int health)
    {
        if(player == null || !player.is_valid())
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

    // TODO: for now this is just a give guns
    // because menus dont work
    static public void event_gun_menu(this CCSPlayerController? player)
    {
        // Event has been cancelled in the mean time dont give any guns
        if(!JailPlugin.event_active())
        {
            return;
        }

        player.gun_menu(false);
    }

    static void give_menu_weapon(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.strip_weapons();

        player.GiveNamedItem("weapon_" + gun_give_name(option.Text));
        player.GiveNamedItem("weapon_deagle");

        player.GiveNamedItem("item_assaultsuit");
    }

    static String[] GUN_LIST =
    {	
        "ak47", "m4a1_silencer","nova",
        "p90", "m249", "mp5sd",
        "galilar", "sg556","bizon", "aug",
        "famas", "xm1014","ssg08","awp"
        
    };

    static String[] GUN_NAMES = 
    {
        "AK47","M4","M3","P90","M249","MP5",
        "FAL","SG556","BIZON","AUG",
        "FAMAS","XM1014","SCOUT","AWP"
    };

    
    public static String gun_give_name(String name)
    {
        // TODO: a linear scan shouldn't matter on a list this small
        for(int i = 0; i < GUN_NAMES.Count(); i++)
        {
            if(name == GUN_NAMES[i])
            {
                return GUN_LIST[i];
            }
        }

        return "";
    }

    static public void gun_menu_internal(this CCSPlayerController? player, bool no_awp, Action<CCSPlayerController, ChatMenuOption> callback)
    {
        // player must be alive and active!
        if(player == null || !player.is_valid_alive())
        {
            return;
        } 

    
        var gun_menu = new ChatMenu("Gun Menu");

        foreach(var weapon_name in GUN_NAMES)
        {
            if(no_awp && weapon_name == "awp")
            {
                continue;
            }

            gun_menu.AddMenuOption(weapon_name, callback);
        }

        ChatMenus.OpenMenu(player, gun_menu);
    }

    static public void gun_menu(this CCSPlayerController? player, bool no_awp)
    {
        // give bots some test guns
        if(player != null && player.is_valid_alive() && player.IsBot)
        {
            player.GiveNamedItem("weapon_ak47");
            player.GiveNamedItem("weapon_deagle");
        }

        gun_menu_internal(player,no_awp,give_menu_weapon);
    }

    // chat + centre text print
    static public void announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    static public void print_prefix(this CCSPlayerController? player, String prefix, String str)
    {
        if(player != null && player.is_valid())
        {
            player.PrintToChat(prefix + str);
        }
    }

    static public void announce(this CCSPlayerController? player,String prefix,String str)
    {
        if(player != null && player.is_valid())
        {
            player.print_prefix(prefix,str);
            player.PrintToCenter(str);
        }
    }

    static public void localise_announce(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        player.announce(prefix,localise(name,args));
    }

    static public void localise_announce(String prefix, String name, params Object[] args)
    {
        String str = localise(name,args);

        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    public static String localise(String name, params Object[] args)
    {
        return JailPlugin.localise(name,args);
    }

    static public void localise(this CCSPlayerController? player,String name, params Object[] args)
    {
        if(player != null && player.is_valid())
        {
            player.PrintToChat(localise(name,args));
        }    
    }

    static public void localise_prefix(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        if(player != null && player.is_valid())
        {
            player.PrintToChat(prefix + localise(name,args));
        }    
    }

    static public void enable_friendly_fire()
    {
        if(ff != null)
        {
            ff.SetValue(true);
        }
    }

    static public void disable_friendly_fire()
    {
        if(ff != null)
        {
            ff.SetValue(false);
        }
    }

    static public void swap_all_t()
    {
        // get valid players
        List<CCSPlayerController> players = Utilities.GetPlayers();
        var valid = players.FindAll(player => player.is_valid_alive());

        foreach(var player in valid)
        {
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    static public List<CCSPlayerController> get_alive_ct()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_ct());
    }

    static public int ct_count()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid() && player.is_ct()).Count;        
    }

    static public int t_count()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid() && player.is_t()).Count;        
    }

    static public int alive_ct_count()
    {
        return get_alive_ct().Count;
    }

    static public List<CCSPlayerController> get_alive_t()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_t());;
    }

    static public int alive_t_count()
    {
        return get_alive_t().Count;
    }

    static public bool block_enabled()
    {
        if(block_cvar != null)
        {
            return block_cvar.GetPrimitiveValue<int>() == 1;
        }

        return true;
    }

    static public void block_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(1);
        }
    }

    static public void unblock_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(0);
        }
    }

    
    static public void set_cvar_str(String name, String value)
    {
        // why doesn't this work lol
        
        ConVar? cvar = ConVar.Find(name);

        if(cvar != null)
        {
            cvar.StringValue = value;
        }
    }

    public static int? to_slot(int? user_id)
    {
        if(user_id == null)
        {
            return null;
        }

        return user_id & 0xff;
    }

    public static int? slot(this CCSPlayerController? player)
    {
        if(player == null)
        {
            return null;
        }

        return to_slot(player.UserId);
    }

    static void force_ent_input(String name, String input)
    {
        // search for door entitys and open all of them!
        var target = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(name);

        foreach(var ent in target)
        {
            if(!ent.IsValid)
            {
                continue;
            }

            ent.AcceptInput(input);
        }
    }

    static String DOOR_PREFIX =  $" {ChatColors.Green}[Door control]: {ChatColors.White}";

    public static void force_close()
    {
        announce(DOOR_PREFIX,"Forcing closing all doors!");

        force_ent_input("func_door","Close");
        force_ent_input("func_movelinear","Close");
        force_ent_input("func_door_rotating","Close");
        force_ent_input("prop_door_rotating","Close");
    }

    public static void force_open()
    {
        announce(DOOR_PREFIX,"Forcing open all doors!");

        force_ent_input("func_door","Open");
        force_ent_input("func_movelinear","Open");
        force_ent_input("func_door_rotating","Open");
        force_ent_input("prop_door_rotating","Open");
        force_ent_input("func_breakable","Break");
    }

    static public bool is_active_team(int team)
    {
        return (team == Lib.TEAM_T || team == Lib.TEAM_CT);
    }

    static void respawn_callback(int? slot)
    {
        if(slot != null)
        {
            var player = Utilities.GetPlayerFromSlot(slot.Value);

            if(player != null && player.is_valid())
            {
                player.Respawn();
            }
        }   
    }

    static public void respawn_delay(this CCSPlayerController? player, float delay)
    {
        if(JailPlugin.global_ctx != null)
        {
            JailPlugin.global_ctx.AddTimer(delay,() => respawn_callback(player.slot()),CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }
    }


    public static readonly Color CYAN = Color.FromArgb(255, 153, 255, 255);
    public static readonly Color RED = Color.FromArgb(255, 255, 0, 0);

    static ConVar? block_cvar = ConVar.Find("mp_solid_teammates");
    static ConVar? ff = ConVar.Find("mp_teammates_are_enemies");

    // CONST DEFS
    public const int TEAM_SPEC = 1;
    public const int TEAM_T = 2;
    public const int TEAM_CT = 3;

    public const int HITGROUP_HEAD = 0x1;
}