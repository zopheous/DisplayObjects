using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace DisplayObjects;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;
    public float PositionRadius { get; set; } = 3.5f;
    public float TooltipPadding { get; set; } = 5f;
    public bool ShowTooltip { get; set; } = true;
    public bool MouseOverTooltip { get; set; } = true;
    public bool[] EnabledKind { get; set; } = new bool[16];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
