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

public class Knife : LRBase
{
    public Knife(LastRequestManager manager,int t, int ct, int slot) : base(manager,t,ct,slot)
    {

    }

    public override void init_player(CCSPlayerController player)
    {    
        // give player a knife and restrict them to it
        player.GiveNamedItem("weapon_knife");
        weapon_restrict = "knife";

        // Handle options
    }
}