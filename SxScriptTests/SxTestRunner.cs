using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
namespace SxScriptTests;

public class SxTestRunner
{
    private static string[] GetTestCases()
    {
        return Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Programs/In", "", SearchOption.AllDirectories);
    }

    [Test, TestCaseSource(nameof(GetTestCases))]
    public async Task Test1(string testPath)
    {
        string outPath = testPath.Replace("/Programs/In", "/Programs/Out");
        string input = await File.ReadAllTextAsync(testPath);
        string correctOutput = await File.ReadAllTextAsync(outPath);

        if (testPath.Contains("_flaky"))
        {
            Assert.Inconclusive("Flaky test přeskočen");
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();
        SxScript.SxScript script = new SxScript.SxScript();
        string realOutput = await script.Interpret(input);
        sw.Stop();
        
        bool correct = correctOutput.Replace("\r\n", "\n").Trim() == realOutput.Replace("\r\n", "\n").Trim();
        if (!correct)
        {
            Assert.Fail($"Test neprošel\nnázev: {testPath}\n---------\nvstup:\n{input}\n----------\nočekávaný výstup:\n{correctOutput}\n------------\nskutečný výstup:\n{realOutput}");
            return;
        }

        Assert.Pass($"Test prošel\nnázev: {testPath}\nčas:\n{sw.Elapsed}\n---------\nvstup:\n{input}\n----------\nočekávaný výstup:\n{correctOutput}\n------------\nskutečný výstup:\n{realOutput}");
    }
}