using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ResStackCounter.Windows;

public class ConfigurationWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private Configuration Configuration => plugin.Config;
    
    public ConfigurationWindow(Plugin plugin)
        : base("Res Stack Counter Configuration", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.plugin = plugin;
    }
    
    public void Dispose()
    {
    }

    public override void Draw()
    {
        var showMastery = Configuration.ShowMastery;
        if (ImGui.Checkbox("Show Job Mastery level in table", ref showMastery))
        {
            plugin.Config.ShowMastery = showMastery;
            plugin.Config.Save();
        }
        
        var autoOpenOnEntry = Configuration.AutoOpenOnEntry;
        if (ImGui.Checkbox("Open automatically when entering South Horn", ref autoOpenOnEntry))
        {
            plugin.Config.AutoOpenOnEntry = autoOpenOnEntry;
            plugin.Config.Save();
        }
        
        var autoConfigureFilterOnEntry = Configuration.AutoFilterKnowledgeLevelOnEntry;
        if (ImGui.Checkbox("Automatically reset filter on entry to show sub Knowledge Level 20 players", ref autoConfigureFilterOnEntry))
        {
            plugin.Config.AutoFilterKnowledgeLevelOnEntry = autoConfigureFilterOnEntry;
            plugin.Config.Save();
        }
    }
}
