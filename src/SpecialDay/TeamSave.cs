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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;

public class TeamSave
{
    public void save()
    {
        count = 0;

        // iter over each active player and save the theam they are on
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            int team = player.TeamNum;

            if(Lib.is_active_team(team))
            {
                slots[count] = player.Slot;
                teams[count] = team;
                count++;
            }
        }      
    }

    public void restore()
    {
        // iter over each player and switch to recorded team
        for(int i = 0; i < count; i++)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slots[i]);

            if(!player.is_valid())
            {
                continue;
            }

            if(Lib.is_active_team(player.TeamNum))
            {
                player.SwitchTeam((CsTeam)teams[i]);
            }
        }

        count = 0;
    }

    int[] slots = new int[64];
    int[] teams = new int[64];

    int count = 0;
};