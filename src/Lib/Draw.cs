
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;


class Line
{
    public void Move(Vector start, Vector end)
    {
        if(laserIndex == -1)
        {
            laserIndex = Entity.DrawLaser(start,end,2.0f,colour);
        }

        else
        {
            Entity.MoveLaserByIndex(laserIndex,start,end);
        }
    }

    public void Destroy()
    {
        if(laserIndex != -1)
        {
            Entity.Remove(laserIndex,"env_beam");
            laserIndex = -1;
        }
    }

    public void DestroyDelay(float life)
    {
        if(laserIndex != -1)
        {
            CBaseEntity? laser = Utilities.GetEntityFromIndex<CBaseEntity>(laserIndex);
            laser.RemoveDelay(life,"env_beam");
        }
    }

    int laserIndex = -1;
    public Color colour = Lib.CYAN;
}


class Circle
{
    public Circle()
    {
        for(int l = 0; l < lines.Count(); l++)
        {
            lines[l] = new Line();
        }
    }

    static Vector AngleOnCircle(float angle,float r, Vector mid)
    {
        // {r * cos(x),r * sin(x)} + mid
        // NOTE: we offset Z so it doesn't clip into the ground
        return new Vector((float)(mid.X + (r * Math.Cos(angle))),(float)(mid.Y + (r * Math.Sin(angle))), mid.Z + 6.0f);
    }

    public void Draw(float life, float radius,float X, float Y, float Z)
    {
        Vector mid =  new Vector(X,Y,Z);

        // draw piecewise approx by stepping angle
        // and joining points with a dot to dot
        float step = (float)(2.0f * Math.PI) / (float)lines.Count();

        float angleOld = 0.0f;
        float angleCur = step;

        for(int l = 0; l < lines.Count(); l++)
        {
            Vector start = AngleOnCircle(angleOld,radius,mid);
            Vector end = AngleOnCircle(angleCur,radius,mid);

            lines[l].Move(start,end);
            lines[l].DestroyDelay(life);

            angleOld = angleCur;
            angleCur += step;
        }
    }

    public void Draw(float life, float radius,Vector vec)
    {
        Draw(life,radius,vec.X,vec.Y,vec.Z);
    }

    public void Destroy()
    {
        for(int l = 0; l < lines.Count(); l++)
        {
            lines[l].Destroy();
        }      
    }

    Line[] lines = new Line[30];
    public Color colour = Lib.CYAN;
}