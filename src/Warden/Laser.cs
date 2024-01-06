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
        // draw marker
        if(is_warden(player) && player != null && player.is_valid())
        {
            // make sure we destroy the old marker
            // because this generates alot of ents
            remove_marker();

            //Server.PrintToChatAll($"{Lib.ent_count()}");

            marker = Lib.draw_marker(x,y,z,60.0f);
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

        if(pawn != null && pawn.AbsOrigin != null && camera != null && use_key)
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

            // make new laser
            if(laser_index == -1)
            {
                laser_index = Lib.draw_laser(eye,end,0.0f,2.0f,Lib.CYAN);
            }

            // update laser by moving
            else
            {
                CEnvBeam? laser = Utilities.GetEntityFromIndex<CEnvBeam>(laser_index);
                if(laser != null && laser.DesignerName == "env_beam")
                {
                    laser.move(eye,end);
                }
            }
        }

        // hide laser
        else
        {
            remove_laser();
        }
    }

    public static readonly float LASER_TIME = 0.1f;

    int[]? marker = null;
    int laser_index = -1;
}