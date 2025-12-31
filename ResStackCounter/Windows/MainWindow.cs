using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Status = Dalamud.Game.ClientState.Statuses.IStatus;


namespace ResStackCounter.Windows;

using PlayerDataStructure = Tuple<IBattleChara, Status?, Status, Status?, byte>;

public class MainWindow : Window, IDisposable
{
    private static readonly Vector2 IconSize = new(24, 32);

    private string ResStackSelectedFilterOptions =>
        string.Join(
            ", ",
            Configuration.ResStackCountFilter.Index().Where(tuple => tuple.Item)
                         .Select(tuple => tuple.Index.ToString()));

    private string JobTypeSelectedFilterOptions
    {
        get
        {
            List<String> result = [];
            if (Configuration.JobTypeFilter[0])
            {
                result.Add("Tanks");
            }

            if (Configuration.JobTypeFilter[1])
            {
                result.Add("Healers");
            }

            if (Configuration.JobTypeFilter[2])
            {
                result.Add("DPS");
            }

            return result.Count > 0 ? string.Join(", ", result) : "None";
        }
    }

    private string nameFilter = "";
    private readonly Plugin plugin;

    private Configuration Configuration => plugin.Config;

    public MainWindow(Plugin plugin)
        : base("Res Stack Counter", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (Plugin.ClientState.TerritoryType != Plugin.TerritorySouthHorn)
        {
            ImGui.TextUnformatted("Currently not in South Horn!");
            return;
        }

        ImGui.TextUnformatted($"Currently counted players: {Plugin.ObjectTable.PlayerObjects.Count()}");
        ImGui.Spacing();

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (child.Success)
            {
                ImGui.SetNextItemWidth(150f);
                var sortingOrder = plugin.Config.SortingOrder;
                if (ImGuiEx.Combo("##order", ref sortingOrder, SortingOrder.Options))
                {
                    plugin.Config.SortingOrder = sortingOrder;
                    plugin.Config.Save();
                }

                ImGui.Separator();
                DrawStatusTable();
            }
        }
    }

    private void DrawStatusTable()
    {
        if (ImGui.BeginTable("StatusTable", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
        {
            for (var i = 0; i < (Configuration.ShowMastery ? 5 : 4); i++)
            {
                ImGui.TableSetupColumn($"Col{i}");
            }

            DrawTableHeader();

            var playerDataList = RetrievePlayerList();
            SortList(playerDataList, Configuration.SortingOrder);
            playerDataList.ForEach(DrawPlayerRow);

            ImGui.EndTable();
        }
    }

    private void DrawTableHeader()
    {
        ImGui.TableNextColumn();
        ImGui.Text("Class/Name");
        ImGui.SetNextItemWidth(150f);
        if (ImGui.BeginCombo("##jobtype", JobTypeSelectedFilterOptions))
        {
            var tankRef = Configuration.JobTypeFilter[0];
            if (ImGui.Checkbox("Tanks", ref tankRef))
            {
                Configuration.JobTypeFilter[0] = tankRef;
                Configuration.Save();
            }

            var healRef = Configuration.JobTypeFilter[1];
            if (ImGui.Checkbox("Healers", ref healRef))
            {
                Configuration.JobTypeFilter[1] = healRef;
                Configuration.Save();
            }

            var dpsRef = Configuration.JobTypeFilter[2];
            if (ImGui.Checkbox("DPS", ref dpsRef))
            {
                Configuration.JobTypeFilter[2] = dpsRef;
                Configuration.Save();
            }

            ImGui.EndCombo();
        }

        ImGui.SetNextItemWidth(120f);
        ImGui.InputTextWithHint("##namefilter", "Name Filter...", ref nameFilter, 21);
        ImGui.TableNextColumn();
        ImGui.Text("Phantom Job");
        ImGui.SetNextItemWidth(100f);
        if (ImGui.BeginCombo("##pj", "Select..."))
        {
            var allPhantomJobs = Configuration.AllPhantomJobs;
            if (ImGuiEx.Checkbox("Toggle all", ref allPhantomJobs))
            {
                Configuration.AllPhantomJobs = allPhantomJobs;
                Configuration.Save();
            }

            ImGui.Separator();

            foreach (var phantomJob in Configuration.PhantomJobFilter)
            {
                if (ImGui.Checkbox(phantomJob.PhantomJob.PhantomJobName, ref phantomJob.Visible))
                {
                    plugin.Config.Save();
                }
            }

            ImGui.EndCombo();
        }

        if (Configuration.ShowMastery)
        {
            ImGui.TableNextColumn();
            ImGui.Text("Mastery");
        }

        ImGui.TableNextColumn();
        ImGui.Text("Res Stacks");
        ImGui.SetNextItemWidth(80f);
        if (ImGui.BeginCombo(
                "##count", ResStackSelectedFilterOptions))
        {
            for (var i = 3; i >= 0; i--)
            {
                if (ImGui.Checkbox(i.ToString(), ref Configuration.ResStackCountFilter[i]))
                {
                    plugin.Config.Save();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.TableNextColumn();
        ImGui.Text("Knowledge Level");
        var hideLevel20Players = Configuration.HideLevel20PlayersFilter;
        if (ImGui.Checkbox("Hide Level 20", ref hideLevel20Players))
        {
            Configuration.HideLevel20PlayersFilter = hideLevel20Players;
            Configuration.Save();
        }

        ImGui.TableNextRow();
    }

    private List<PlayerDataStructure> RetrievePlayerList()
    {
        List<PlayerDataStructure> playerDataList = [];
        foreach (var player in Plugin.ObjectTable.PlayerObjects)
        {
            Status? mastery = null;
            Status? phantomJob = null;
            Status? resStacks = null;

            foreach (var status in player.StatusList)
            {
                if (Configuration.ShowMastery && IsMasteryBuff(status.StatusId))
                {
                    mastery = status;
                }
                else if (IsPhantomJobBuff(status.StatusId))
                {
                    phantomJob = status;
                }
                else if (IsResRestrictedDebuff(status))
                {
                    resStacks = status;
                }
            }

            var knowledgeLevel = GetKnowledgeLevel(player);
            if (ShouldShow(phantomJob, resStacks, player, knowledgeLevel))
            {
                playerDataList.Add(new PlayerDataStructure(player, mastery, phantomJob, resStacks, knowledgeLevel));
            }
        }

        return playerDataList;
    }

    private void DrawPlayerRow(PlayerDataStructure playerData)
    {
        var player = playerData.Item1;
        var mastery = playerData.Item2;
        var phantomJob = playerData.Item3;
        var resStacks = playerData.Item4;
        var knowledgeLevel = playerData.Item5;

        ImGui.TableNextColumn();
        ImGui.Image(
            Plugin.TextureProvider.GetFromGameIcon(ClassJobIconBaseId + player.ClassJob.RowId).GetWrapOrEmpty()
                  .Handle,
            new Vector2(32, 32));
        ImGui.SameLine();
        ImGui.Text(player.Name.TextValue);
        ImGui.SameLine();

        ImGui.TableNextColumn();
        var pjIcon = Plugin.TextureProvider
                           .GetFromGameIcon(new GameIconLookup(phantomJob.GameData.Value.Icon))
                           .GetWrapOrEmpty();
        ImGui.Image(pjIcon.Handle, IconSize);

        if (Configuration.ShowMastery)
        {
            ImGui.TableNextColumn();
            var masteryIconId =
                mastery != null ? mastery.GameData.Value.Icon + mastery.Param - 1 : MissingMasteryIconId;

            var masteryIcon = Plugin.TextureProvider
                                    .GetFromGameIcon(new GameIconLookup(masteryIconId))
                                    .GetWrapOrEmpty();
            ImGui.Image(masteryIcon.Handle, IconSize);
        }

        ImGui.TableNextColumn();

        if (resStacks != null)
        {
            var resStacksIconId = resStacks.StatusId == ResurrectionDeniedBuffId
                                      ? resStacks.GameData.Value.Icon
                                      : resStacks.GameData.Value.Icon + resStacks.Param - 1;
            var resStacksIcon = Plugin.TextureProvider
                                      .GetFromGameIcon(new GameIconLookup(resStacksIconId))
                                      .GetWrapOrEmpty();
            ImGui.Image(resStacksIcon.Handle, IconSize);
        }
        else
        {
            ImGui.Text("-");
        }

        ImGui.TableNextColumn();
        ImGui.Text(knowledgeLevel.ToString());

        ImGui.TableNextRow();
    }

    private static bool IsMasteryBuff(uint buffId)
    {
        return buffId == MasteryBuffId;
    }

    private static bool IsPhantomJobBuff(uint buffId)
    {
        return PhantomJob.PhantomJobs.Any(pj => pj.BuffId == buffId);
    }

    private static bool IsResRestrictedDebuff(Status status)
    {
        return status.StatusId is ResurrectionRestrictedBuffId or ResurrectionDeniedBuffId;
    }

    private bool ShouldShow(Status? phantomJob, Status? resStacks, IBattleChara player, byte knowledgeLevel)
    {
        if (!StringExtensions.IsNullOrEmpty(nameFilter) &&
            nameFilter.Split(" ").Any(token => !player.Name.TextValue.Contains(token)))
        {
            return false;
        }

        if (phantomJob == null ||
            Configuration.PhantomJobFilter.Where(pj => pj.Visible)
                         .All(x => phantomJob.StatusId != x.PhantomJob.BuffId))
        {
            return false;
        }

        var count = resStacks?.Param ?? 0;
        if (resStacks != null &&
            !((resStacks.StatusId == ResurrectionDeniedBuffId && Configuration.ResStackCountFilter[0]) ||
              Configuration.ResStackCountFilter[count]))
        {
            return false;
        }

        if (Configuration.HideLevel20PlayersFilter && knowledgeLevel == 20)
        {
            return false;
        }

        var playerJobFilterIndex = player.ClassJob.Value.Role switch
        {
            // Tanks
            1 => 0,
            // Healers
            4 => 1,
            // Other, Including Melee/Ranged
            _ => 2
        };

        if (!Configuration.JobTypeFilter[playerJobFilterIndex])
        {
            return false;
        }

        return true;
    }

    private static void SortList(
        List<PlayerDataStructure> playerDataList, SortingOrder sortingOrder)
    {
        Comparison<PlayerDataStructure> comparison = sortingOrder.Field switch
        {
            SortingOption.Class => (a, b) =>
                ClassOrder.IndexOf(a.Item1.ClassJob.Value.JobIndex) -
                ClassOrder.IndexOf(b.Item1.ClassJob.Value.JobIndex),
            SortingOption.Name => (a, b) =>
                string.Compare(a.Item1.Name.TextValue, b.Item1.Name.TextValue, StringComparison.Ordinal),
            SortingOption.Mastery => (a, b) =>
            {
                var aVal = a.Item2?.Param ?? 0;
                var bVal = b.Item2?.Param ?? 0;
                return aVal - bVal;
            },
            SortingOption.Phantom_Job => (a, b) =>
            {
                var aVal = PhantomJob.PhantomJobs.FindIndex(0, pj => a.Item3.StatusId == pj.BuffId);
                var bVal = PhantomJob.PhantomJobs.FindIndex(0, pj => b.Item3.StatusId == pj.BuffId);
                return aVal - bVal;
            },
            SortingOption.Res_Stacks => (a, b) =>
            {
                var aVal = a.Item4?.StatusId == ResurrectionDeniedBuffId ? 0 : a.Item4?.Param ?? 0;
                var bVal = b.Item4?.StatusId == ResurrectionDeniedBuffId ? 0 : b.Item4?.Param ?? 0;
                return aVal - bVal;
            },
            SortingOption.Knowledge_level => (a, b) => a.Item5 - b.Item5,
            _ => (_, _) => 0
        };

        playerDataList.Sort(comparison);
        if (!sortingOrder.Ascending)
        {
            playerDataList.Reverse();
        }
    }

    private unsafe byte GetKnowledgeLevel(IBattleChara player)
    {
        return ((BattleChara*)player.Address)->GetForayInfo()->Level;
    }

    private const int MissingMasteryIconId = 215190;

    private const int ResurrectionDeniedBuffId = 4263;

    private const int ResurrectionRestrictedBuffId = 4262;

    private const int MasteryBuffId = 4226;

    private const uint ClassJobIconBaseId = 62100u;

    // Tanks -> Healers -> Melee -> Ranged -> Caster
    private static readonly byte[] ClassOrder =
        [1, 3, 12, 17, 6, 13, 9, 20, 4, 10, 14, 19, 21, 5, 11, 18, 7, 8, 15, 22];
}
