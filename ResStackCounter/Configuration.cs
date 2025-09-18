using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using ResStackCounter.Windows;

namespace ResStackCounter;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool ShowMastery { get; set; } = true;

    public bool AutoOpenOnEntry { get; set; }
    
    public bool AutoFilterKnowledgeLevelOnEntry { get; set; }

    public SortingOrder SortingOrder { get; set; } = new (SortingOption.Name, false);

    // In order 0->3 Stacks remaining
    public bool[] ResStackCountFilter { get; set; } = [true, true, true, true];

    // Tanks, healers, DPS
    public bool[] JobTypeFilter { get; set; } = [true, true, true];
    
    public PhantomJobFilterToggle[] PhantomJobFilter { get; set; } =
        PhantomJob.PhantomJobs.Select(pj => new PhantomJobFilterToggle(pj, true)).ToArray();

    public bool HideLevel20PlayersFilter { get; set; }

    [JsonIgnore]
    public bool? AllPhantomJobs
    {
        get
        {
            return PhantomJobFilter.All(pj => pj.Visible) ? true
                   : PhantomJobFilter.Any(pj => pj.Visible) ? null : false;
        }
        set
        {
            var result = value ?? true;
            foreach (var phantomJobFilterToggle in PhantomJobFilter)
            {
                phantomJobFilterToggle.Visible = result;
            }
        }
    }


    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public static readonly Configuration Default = new();
    public static readonly Configuration KnowledgeLevelFilterConfig = new()
    {
        HideLevel20PlayersFilter = true
    };

    public void CopyFilters(Configuration other)
    {
        for (var i = 0; i < 4; i++)
        {
            ResStackCountFilter[i] = other.ResStackCountFilter[i];
        }
        HideLevel20PlayersFilter = other.HideLevel20PlayersFilter;
        PhantomJobFilter = other.PhantomJobFilter;
    }
}

public class PhantomJobFilterToggle(PhantomJob phantomJob, bool visible)
{
    public PhantomJob PhantomJob = phantomJob;
    public bool Visible = visible;
}

public class SortingOrder(SortingOption field, bool ascending)
{
    public readonly SortingOption Field = field;
    public readonly bool Ascending = ascending;

    public override string ToString()
    {
        return $"{Field.ToString().Replace("_", " ")} - {(Ascending ? "Asc" : "Desc")}";
    }

    public static readonly SortingOrder[] Options =
    [
        new(SortingOption.Class, true),
        new(SortingOption.Class, false),
        new(SortingOption.Name, true),
        new(SortingOption.Name, false),
        new(SortingOption.Phantom_Job, true),
        new(SortingOption.Phantom_Job, false),
        new(SortingOption.Mastery, true),
        new(SortingOption.Mastery, false),
        new(SortingOption.Res_Stacks, true),
        new(SortingOption.Res_Stacks, false),
        new(SortingOption.Knowledge_level, true),
        new(SortingOption.Knowledge_level, false),
    ];
}

public enum SortingOption
{
    // ReSharper disable InconsistentNaming
    Class,
    Name,
    Phantom_Job,
    Mastery,
    Res_Stacks,
    Knowledge_level
    // ReSharper enable InconsistentNaming
}

public enum JobTypeFilterOption
{
    Tanks, Healers, DPS
}
