
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;

public interface IWardenService
{
    public bool IsWarden(CCSPlayerController? player);
    public CCSPlayerController? GetWarden();
    public void SetWarden(CCSPlayerController player);
}