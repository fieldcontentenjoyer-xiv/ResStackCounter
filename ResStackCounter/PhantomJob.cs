using System.Collections.Generic;
using System.Linq;

namespace ResStackCounter;

public class PhantomJob
{
    public readonly uint BuffId;
    public readonly string PhantomJobName;

    public PhantomJob(uint buffId, string phantomJobName)
    {
        BuffId = buffId;
        PhantomJobName = phantomJobName;
    }

    public static readonly Dictionary<int, PhantomJob> PhantomJobLookupTable = new()
    {
        { 4358, new PhantomJob(4358, "Knight") },
        { 4360, new PhantomJob(4360, "Monk") },
        { 4369, new PhantomJob(4369, "Thief") },
        { 5331, new PhantomJob(5331, "Dragoon") },
        { 5328, new PhantomJob(5328, "Ninja") },
        { 4362, new PhantomJob(4362, "Samurai") },
        { 4359, new PhantomJob(4359, "Berserker") },
        { 4361, new PhantomJob(4361, "Ranger") },
        { 4803, new PhantomJob(4803, "Mystic Knight") },
        { 5329, new PhantomJob(5329, "White Mage") },
        { 5330, new PhantomJob(5330, "Black Mage") },
        { 4365, new PhantomJob(4365, "Time Mage") },
        { 5332, new PhantomJob(5332, "Summoner") },
        { 5333, new PhantomJob(5333, "Blue Mage") },
        { 5334, new PhantomJob(5334, "Red Mage") },
        { 4367, new PhantomJob(4367, "Chemist") },
        { 4364, new PhantomJob(4364, "Geomancer") },
        { 4363, new PhantomJob(4363, "Bard") },
        { 4805, new PhantomJob(4805, "Dancer") },
        { 5335, new PhantomJob(5335, "Necromancer") },
        { 4368, new PhantomJob(4368, "Oracle") },
        { 4366, new PhantomJob(4366, "Cannoneer") },
        { 4804, new PhantomJob(4804, "Gladiator") },
        { 4242, new PhantomJob(4242, "Freelancer") },
    };

    public static readonly List<PhantomJob> PhantomJobs = PhantomJobLookupTable.Values.ToList();
}
