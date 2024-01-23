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
    void remove_marker()
    {
        if(marker != null)
        {
            Lib.destroy_beam_group(marker);
            marker = null;
        }
    }

    public void ping(CCSPlayerController? player, float x, float y, float z)
    {
        JailPlayer? jail_player = jail_player_from_player(player);

        // draw marker
        if(is_warden(player) && player.is_valid() && jail_player != null)
        {
            // make sure we destroy the old marker
            // because this generates alot of ents
            remove_marker();

            //Server.PrintToChatAll($"{Lib.ent_count()}");

            marker = Lib.draw_marker(x,y,z,75.0f,60.0f,jail_player.marker_colour);
        }
    }

    void remove_laser()
    {
        if(laser_index != -1)
        {
            Lib.remove_ent(laser_index,"env_beam");
            laser_index = -1;
        }
    }

    public void laser_tick()
    {
        if(!config.warden_laser)
        {
            return;
        }

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

        CCSPlayerPawn? pawn = warden.pawn();
        CPlayer_CameraServices? camera = pawn?.CameraServices;

        JailPlayer? jail_player = jail_player_from_player(warden);

        if(pawn != null && pawn.AbsOrigin != null && camera != null && use_key && jail_player != null)
        {
            Vector eye = new Vector(pawn.AbsOrigin.X,pawn.AbsOrigin.Y,pawn.AbsOrigin.Z + camera.OldPlayerViewOffsetZ);

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

            laser_index = Lib.update_laser(laser_index,eye,end,jail_player.laser_colour);
        }

        // hide laser
        else
        {
            remove_laser();
        }
    }

    void set_laser(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        var text = option.Text;
        JailPlayer? jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.set_laser(player,text);
        }
    }

    void set_marker(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        var text = option.Text;
        JailPlayer? jail_player = jail_player_from_player(player);

        if(jail_player != null)
        {
            jail_player.set_marker(player,text);
        }
    }

    void colour_menu(CCSPlayerController? player,Action<CCSPlayerController, ChatMenuOption> callback, String name)
    {
        if(!player.is_valid())
        {
            return;
        }

        var colour_menu = new ChatMenu(name);

        foreach(var item in Lib.LASER_CONFIG_MAP)
        {
            colour_menu.AddMenuOption(item.Key, callback);
        }

        ChatMenus.OpenMenu(player, colour_menu);    
    }

    public void laser_colour_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        colour_menu(player,set_laser,"Laser colour");
    }

    public void marker_colour_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        colour_menu(player,set_marker,"Marker colour");
    }

    public static readonly float LASER_TIME = 0.1f;

    int[]? marker = null;
    int laser_index = -1;
}