using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace DisplayObjects.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    public MainWindow(Plugin plugin)
        : base("DisplayObjects", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs)
    {
        Plugin = plugin;
    }

    public void Dispose() { }

    public void OnFrameworkTick(IFramework framework)
    {
        IsOpen = Plugin.Configuration.Enabled;
        Position = new Vector2(0, 0);
        Size = ImGui.GetMainViewport().Size;
    }

    public string ObjectNameString(IGameObject obj)
    {
        var name = obj.Name.TextValue;
        if (obj.ObjectKind == ObjectKind.Player)
        {
            var c = (ICharacter)obj;
            if (c.CompanyTag.TextValue.Length > 0)
                name += " <" + c.CompanyTag + ">";
        }
        return name;
    }

    public string ObjectKindString(IGameObject obj)
    {
        string str = obj.ObjectKind.ToString();
        if (obj.ObjectKind == ObjectKind.BattleNpc)
        {
            str += "->" + ((BattleNpcSubKind)obj.SubKind).ToString();
        }
        return str;
    }

    public string ObjectTooltip(IGameObject obj)
    {
        return $"Name: {ObjectNameString(obj)}\n" +
            $"Type: {ObjectKindString(obj)}\n" +
            $"Dead: {obj.IsDead}\n" +
            $"Targetable: {obj.IsTargetable}\n" +
            $"HitboxRadius: {obj.HitboxRadius:F}\n" +
            $"Position: (x:{obj.Position.X:F}, y:{obj.Position.Y:F}, z:{obj.Position.Z:F})\n" +
            $"GameObjectId: {obj.GameObjectId}\n" +
            $"EntityId: {obj.EntityId}\n" +
            $"DataId: {obj.DataId}";
    }

    public string NpcDetailedTooltip(ICharacter c)
    {
        return $"Level: {c.Level}\n" + 
            $"HP: {c.CurrentHp:N0}/{c.MaxHp:N0} (+{c.ShieldPercentage}%)\n" +
            $"MP: {c.CurrentMp:N0}/{c.MaxMp:N0}\n" +
            $"GP: {c.CurrentGp:N0}/{c.MaxGp:N0}\n" +
            $"CP: {c.CurrentCp:N0}/{c.MaxCp:N0}\n" +
            $"Status: {c.StatusFlags}";
    }

    public string BattleCharaDetailedTooltip(IBattleChara c)
    {
        string str = NpcDetailedTooltip(c);
        if (c.IsCasting)
        {
            var actionSheet = Plugin.GameData.GetExcelSheet<Lumina.Excel.Sheets.Action>();
            var row = actionSheet.GetRow(c.CastActionId);

            str += $"\nCasting {row.Name} [{c.CastActionId}]\n" +
                $"\tType: {(ActionType)c.CastActionType}\n" +
                $"\tInterruptible: {c.IsCastInterruptible}\n" +
                $"\tTime: {c.CurrentCastTime:F}/{c.TotalCastTime:F}";
        }
        return str;
    }

    public override void Draw()
    {
        Vector2 cursorPos = ImGui.GetMousePos();
        var config = Plugin.Configuration;
        foreach (IGameObject obj in Plugin.Objects)
        {
            if (!config.EnabledKind[(int)obj.ObjectKind])
                continue;

            Vector3 pos3d = obj.Position;
            Vector2 pos2d;
            bool onScreen = Plugin.GameGui.WorldToScreen(pos3d, out pos2d);
            if (!onScreen)
                continue;

            ImGui.GetWindowDrawList().AddCircleFilled(
                pos2d, config.PositionRadius, ImGui.GetColorU32(new Vector4(1f, 0f, 0f, 1f))
            );

            // Draw tooltip
            bool mouseOver = config.MouseOverTooltip && Vector2.Distance(cursorPos, pos2d) < 20;
            if (mouseOver || config.ShowTooltip)
            {
                string tooltip = "";
                string detailedTooltip = "";
                Vector2 tooltipPos;
                if (mouseOver)
                {
                    tooltip = ObjectTooltip(obj);
                    tooltipPos = cursorPos;

                    // Generate detailed tooltip from excels
                    switch (obj.ObjectKind)
                    {
                        case ObjectKind.Player:
                            IPlayerCharacter character = (IPlayerCharacter)obj;
                            detailedTooltip = BattleCharaDetailedTooltip(character);
                            break;
                        case ObjectKind.BattleNpc:
                            IBattleNpc battleNpc = (IBattleNpc)obj;
                            detailedTooltip = BattleCharaDetailedTooltip(battleNpc);
                            break;
                        case ObjectKind.EventNpc:
                            INpc npc = (INpc)obj;
                            detailedTooltip = NpcDetailedTooltip(npc);
                            break;
                        case ObjectKind.Treasure:
                            break;
                        case ObjectKind.Aetheryte:
                            var aetheryteSheet = Plugin.GameData.GetExcelSheet<Lumina.Excel.Sheets.Aetheryte>();
                            if (aetheryteSheet != null)
                            {
                                var row = aetheryteSheet.GetRow(obj.DataId);
                                var territory = row.Territory.Value;
                                var zone = territory.PlaceNameZone.Value.Name;
                                var region = territory.PlaceNameRegion.Value.Name;
                                detailedTooltip =
                                    "Zone: " + zone.ToString() + "\n" +
                                    "Region: " + region.ToString() + "\n" +
                                    "Territory: " + territory.PlaceName.Value.Name.ToString() + " [" + territory.RowId + "]\n" +
                                    "Aetheryte: " + row.PlaceName.Value.Name.ToString() + " [" + row.RowId + "]";
                            }
                            break;
                        case ObjectKind.GatheringPoint:
                            break;
                        case ObjectKind.EventObj:
                            break;
                        case ObjectKind.MountType:
                            break;
                        case ObjectKind.Companion:
                            break;
                        case ObjectKind.Retainer:
                            break;
                        case ObjectKind.Area:
                            break;
                        case ObjectKind.Housing:
                            break;
                        case ObjectKind.Cutscene:
                            break;
                        case ObjectKind.CardStand:
                            break;
                        case ObjectKind.Ornament:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    var name = obj.Name.TextValue;
                    if (name.Length == 0)
                        tooltip = $"[{ObjectKindString(obj)}]";
                    else
                        tooltip = $"{ObjectNameString(obj)}\n[{ObjectKindString(obj)}]";
                    tooltipPos = pos2d;
                }

                float padding = config.TooltipPadding;
                Vector2 tooltipSize = ImGui.CalcTextSize(tooltip);
                ImGui.GetWindowDrawList().AddRectFilled(
                    new Vector2(tooltipPos.X + padding, tooltipPos.Y + padding),
                    new Vector2(tooltipPos.X + 3 * padding + tooltipSize.X,
                        tooltipPos.Y + 3 * padding + tooltipSize.Y),
                    ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.33f)), padding
                );
                ImGui.GetWindowDrawList().AddText(
                    new Vector2(tooltipPos.X + 2 * padding, tooltipPos.Y + 2 * padding),
                    ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)),
                    tooltip
                );

                if (mouseOver && detailedTooltip.Length > 0)
                {
                    Vector2 detailedTooltipSize = ImGui.CalcTextSize(detailedTooltip);
                    ImGui.GetWindowDrawList().AddRectFilled(
                        new Vector2(tooltipPos.X + 4 * padding + tooltipSize.X, tooltipPos.Y + padding),
                        new Vector2(tooltipPos.X + 6 * padding + tooltipSize.X + detailedTooltipSize.X,
                            tooltipPos.Y + 3 * padding + detailedTooltipSize.Y),
                        ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.33f)), padding
                    );
                    ImGui.GetWindowDrawList().AddText(
                        new Vector2(tooltipPos.X + 5 * padding + tooltipSize.X, tooltipPos.Y + 2 * padding),
                        ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)),
                        detailedTooltip
                    );
                }
            }
        }
    }
}
