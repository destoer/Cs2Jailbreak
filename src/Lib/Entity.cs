using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public static class Entity
{
    static public void Remove(int index, String name)
    {
        CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>(index);

        if(ent != null && ent.DesignerName == name)
        {
            ent.Remove();
        }
    }

    static void ForceEntInput(String name, String input)
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

    // TODO: is their a cheaper way to do this?
    static public int EntCount()
    {
        return Utilities.GetAllEntities().Count();
    }


    static public void Move(this CEnvBeam? laser,Vector start, Vector end)
    {
        if(laser == null)
        {
            return;
        }

        // set pos
        laser.Teleport(start, Lib.ANGLE_ZERO, Lib.VEC_ZERO);

        // end pos
        // NOTE: we cant just move the whole vec
        laser.EndPos.X = end.X;
        laser.EndPos.Y = end.Y;
        laser.EndPos.Z = end.Z;

        Utilities.SetStateChanged(laser,"CBeam", "m_vecEndPos");
    }

    static public void MoveLaserByIndex(int laserIndex,Vector start, Vector end)
    {
        CEnvBeam? laser = Utilities.GetEntityFromIndex<CEnvBeam>(laserIndex);
        if(laser != null && laser.DesignerName == "env_beam")
        {
            laser.Move(start,end);
        }
    }

    static public void SetColour(this CEnvBeam? laser, Color colour)
    {
        if(laser != null)
        {
            laser.Render = colour;
        }
    }


    static public int DrawLaser(Vector start, Vector end, float width, Color colour)
    {
        CEnvBeam? laser = Utilities.CreateEntityByName<CEnvBeam>("env_beam");

        if(laser == null)
        {
            return -1;
        }

        // setup looks
        laser.SetColour(colour);
        laser.Width = 2.0f;

        // circle not working?
        //laser.Flags |= 8;

        laser.Move(start,end);

        // start spawn
        laser.DispatchSpawn(); 

        return (int)laser.Index;
    }

    public static String DOOR_PREFIX =  $" {ChatColors.Green}[Door control]: {ChatColors.White}";

    public static void ForceClose()
    {
        Chat.Announce(DOOR_PREFIX,"Forcing closing all doors!");

        ForceEntInput("func_door","Close");
        ForceEntInput("func_movelinear","Close");
        ForceEntInput("func_door_rotating","Close");
        ForceEntInput("prop_door_rotating","Close");
    }

    public static void ForceOpen()
    {
        Chat.Announce(DOOR_PREFIX,"Forcing open all doors!");

        ForceEntInput("func_door","Open");
        ForceEntInput("func_movelinear","Open");
        ForceEntInput("func_door_rotating","Open");
        ForceEntInput("prop_door_rotating","Open");
        ForceEntInput("func_breakable","Break");
    }


    static CCSPlayerController? PlayerFromPawn(CCSPlayerPawn? pawn)
    {
        // pawn valid
        if(pawn == null || !pawn.IsValid)
        {
            return null;
        }

        // controller valid
        if(pawn.OriginalController == null || !pawn.OriginalController.IsValid)
        {
            return null;
        }

        // any further validity is up to the caller
        return pawn.OriginalController.Value;    
    }

    static public CCSPlayerController? Player(this CBaseEntity? ent)
    {
        if(ent != null && ent.DesignerName == "player")
        {
            var pawn = new CCSPlayerPawn(ent.Handle);

            return PlayerFromPawn(pawn);
        }

        return null;
    }

    static public CCSPlayerController? Player(this CHandle<CBaseEntity> handle)
    {
        if(handle.IsValid)
        {
            return handle.Value.Player();
        }

        return null;
    }
}