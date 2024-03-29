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
        LocalizeAnnounce("sd.spectre_start");
        LocalizeAnnounce("sd.damage_enable",delay);
    }

    public override void MakeBoss(CCSPlayerController? spectre, int count)
    {
        if(spectre != null && spectre.IsLegalAlive())
        {
            LocalizeAnnounce($"sd.spectre",spectre.PlayerName);

            // give the spectre the HP and swap him
            spectre.SetHealth(count * 60);
            spectre.SwitchTeam(CsTeam.CounterTerrorist);
            
            SetupPlayer(spectre);
        }

        else
        {
            Chat.Announce("[ERROR] ","Error picking spectre");
        }
    }

    public override bool WeaponEquip(CCSPlayerController player,String name) 
    {
        // spectre can only carry a knife
        if(IsBoss(player))
        {
            return name.Contains("knife") || name.Contains("decoy");
        }

        return true;
    }

    public override void Start()
    {
        LocalizeAnnounce("sd.fight");
        Lib.SwapAllT();

        (CCSPlayerController? boss, int count) = PickBoss();
        MakeBoss(boss,count);
    }

    public override void End()
    {
        LocalizeAnnounce("sd.spectre_end");
    }

    public override void SetupPlayer(CCSPlayerController player)
    {
        if(IsBoss(player))
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