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
using System.Drawing;

public class SDSpectre : SDBase
{
    public override void Setup()
    {
        localize_announce("sd.spectre_start");
        localize_announce("sd.damage_enable",delay);
    }

    public override void MakeBoss(CCSPlayerController? spectre, int count)
    {
        if(spectre != null && spectre.is_valid_alive())
        {
            localize_announce($"sd.spectre",spectre.PlayerName);

            // give the spectre the HP and swap him
            spectre.SetHealth(count * 60);
            spectre.SwitchTeam(CsTeam.CounterTerrorist);
            
            SetupPlayer(spectre);
        }

        else
        {
            Chat.announce("[ERROR] ","Error picking spectre");
        }
    }

    public override bool WeaponEquip(CCSPlayerController player,String name) 
    {
        // spectre can only carry a knife
        if(is_boss(player))
        {
            return name.Contains("knife") || name.Contains("decoy");
        }

        return true;
    }

    public override void Start()
    {
        localize_announce("sd.fight");
        Lib.swap_all_t();

        (CCSPlayerController? boss, int count) = PickBoss();
        MakeBoss(boss,count);
    }

    public override void End()
    {
        localize_announce("sd.spectre_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        if(is_boss(player))
        {
            // invis and speed
            player.SetColour(Color.FromArgb(0,0,0,0));
            player.SetVelocity(2.5f);

            player.StripWeapons();
        }

        else
        {
            player.EventGunMenu();
        }
    }
}