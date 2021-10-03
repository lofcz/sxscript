using System.IO;
using NUnit.Framework;
namespace SxScriptTests;

public class SxTestRunner
{
    private static string[] GetTestCases()
    {
        return Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Programs/In", "", SearchOption.AllDirectories);
    }

    [Test, TestCaseSource(nameof(GetTestCases))]
    public void Test1(string testPath)
    {
        string outPath = testPath.Replace("/Programs/In", "/Programs/Out");
        string input = File.ReadAllText(testPath);
        string correctOutput = File.ReadAllText(outPath);

        SxScript.SxScript script = new SxScript.SxScript();
        string realOutput = script.Interpret(input);

        bool correct = correctOutput.Replace("\r\n", "\n").Trim() == realOutput.Replace("\r\n", "\n").Trim();
        if (!correct)
        {
            Assert.Fail($"Test neprošel\nnázev: {testPath}\n---------\nvstup:\n{input}\n----------\nočekávaný výstup:\n{correctOutput}\n------------\nskutečný výstup:\n{realOutput}");
            return;
        }

        Assert.Pass($"Test prošel\nnázev: {testPath}\n---------\nvstup:\n{input}\n----------\nočekávaný výstup:\n{correctOutput}\n------------\nskutečný výstup:\n{realOutput}");
    }
}