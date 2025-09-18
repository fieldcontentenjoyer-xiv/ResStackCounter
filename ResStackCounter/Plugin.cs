using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ResStackCounter.Windows;

namespace ResStackCounter;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    
    [PluginService] internal static IObjectTable  ObjectTable { get; private set; } = null!;

    private const string CommandName = "/ressc";

    public Configuration Config { get; init; }

    public readonly WindowSystem WindowSystem = new("ResStackCounter");
    private MainWindow MainWindow { get; init; }
    
    private ConfigurationWindow ConfigurationWindow { get; init; }

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);
        ConfigurationWindow = new ConfigurationWindow(this);
        
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigurationWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Res Stack Counter window ( add ' Settings' for settings instead)"
        });

        ClientState.TerritoryChanged += HandleTerritoryChanged;
        
        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;
        
        ClientState.TerritoryChanged -= HandleTerritoryChanged;
        
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        if (args.Contains("settings"))
        {
            ConfigurationWindow.Toggle();
        }
        else
        {
            MainWindow.Toggle();            
        }
        
    }
    
    public void ToggleMainUi() => MainWindow.Toggle();
    
    public void ToggleConfigUi() => ConfigurationWindow.Toggle();

    public void HandleTerritoryChanged(ushort newTerritoryId)
    {
        if (newTerritoryId == TerritorySouthHorn)
        {
            if (Config.AutoOpenOnEntry && !MainWindow.IsOpen)
            {
                MainWindow.Toggle();
                
            }

            if (Config.AutoFilterKnowledgeLevelOnEntry)
            {
                Config.CopyFilters(Configuration.KnowledgeLevelFilterConfig);
            }
        }
        
    }

    public const int TerritorySouthHorn = 1252;
}
