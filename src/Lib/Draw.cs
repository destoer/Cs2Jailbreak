
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;


class Line
{
    public void move(Vector start, Vector end)
    {
        if(laser_index == -1)
        {
            laser_index = Entity.draw_laser(start,end,2.0f,colour);
        }

        else
        {
            Entity.move_laser_by_index(laser_index,start,end);
        }
    }

    public void destroy()
    {
        Entity.remove(laser_index,"env_beam");
        laser_index = -1;
    }

    public void destroy_delay(float life)
    {
        CBaseEntity? laser = Utilities.GetEntityFromIndex<CBaseEntity>(laser_index);
        laser.remove_delay(life,"env_beam");
    }

    int laser_index = -1;
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

    static Vector angle_on_circle(float angle,float r, Vector mid)
    {
        // {r * cos(x),r * sin(x)} + mid
        // NOTE: we offset Z so it doesn't clip into the ground
        return new Vector((float)(mid.X + (r * Math.Cos(angle))),(float)(mid.Y + (r * Math.Sin(angle))), mid.Z + 6.0f);
    }

    public void draw(float life, float radius,float X, float Y, float Z)
    {
        Vector mid =  new Vector(X,Y,Z);

        // draw piecewise approx by stepping angle
        // and joining points with a dot to dot
        float step = (float)(2.0f * Math.PI) / (float)lines.Count();

        float angle_old = 0.0f;
        float angle_cur = step;

        for(int l = 0; l < lines.Count(); l++)
        {
            Vector start = angle_on_circle(angle_old,radius,mid);
            Vector end = angle_on_circle(angle_cur,radius,mid);

            lines[l].move(start,end);
            lines[l].destroy_delay(life);

            angle_old = angle_cur;
            angle_cur += step;
        }
    }

    public void draw(float life, float radius,Vector vec)
    {
        draw(life,radius,vec.X,vec.Y,vec.Z);
    }

    public void destroy()
    {
        for(int l = 0; l < lines.Count(); l++)
        {
            lines[l].destroy();
        }      
    }

    Line[] lines = new Line[30];
    public Color colour = Lib.CYAN;
}