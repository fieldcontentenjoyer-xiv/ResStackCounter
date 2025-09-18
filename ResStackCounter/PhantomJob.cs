using System.Collections.Generic;

namespace ResStackCounter.Windows;

public class PhantomJob
{
    public readonly uint BuffId;
    public readonly string PhantomJobName;

    public PhantomJob(uint buffId, string phantomJobName)
    {
        BuffId = buffId;
        PhantomJobName = phantomJobName;
    }

    public static readonly List<PhantomJob> PhantomJobs =
    [
        new(4358, "Knight"),
        new(4360, "Monk"),
        new(4369, "Thief"),
        new(4362, "Samurai"),
        new(4359, "Berserker"),
        new(4361, "Ranger"),
        new(4365, "Time Mage"),
        new(4367, "Chemist"),
        new(4364, "Geomancer"),
        new(4363, "Bard"),
        new(4368, "Oracle"),
        new(4366, "Cannoneer"),
        new(4242, "Freelancer"),
    ];
}
