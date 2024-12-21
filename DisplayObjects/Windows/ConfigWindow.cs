using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DisplayObjects.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin)
        : base("Display objects configurations", ImGuiWindowFlags.NoCollapse)
    {
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var boolVar = Configuration.Enabled;
        if (ImGui.Checkbox("Enabled", ref boolVar))
        {
            Configuration.Enabled = boolVar;
            Configuration.Save();
        }

        boolVar = Configuration.ShowTooltip;
        if (ImGui.Checkbox("Always show tooltip", ref boolVar))
        {
            Configuration.ShowTooltip = boolVar;
            Configuration.Save();
        }

        boolVar = Configuration.MouseOverTooltip;
        if (ImGui.Checkbox("Show details when mouseover", ref boolVar))
        {
            Configuration.MouseOverTooltip = boolVar;
            Configuration.Save();
        }

        ImGui.Text("Enable for object types:");
        for (int i = 1; i < 16; i++)
        {
            ImGui.PushID(i);
            boolVar = Configuration.EnabledKind[i];
            if (ImGui.Checkbox(((ObjectKind)i).ToString(), ref boolVar))
            {
                Configuration.EnabledKind[i] = boolVar;
                Configuration.Save();
            }
            ImGui.PopID();
        }
    }
}
