using GeoHashDaemon;

namespace GeoHashTest;

[TestClass]
public class TestGeoHash
{
    [TestMethod]
    public async Task TestGetDowJones()
    {
        var result = await GeoHash.GetDowJonesAsync(new GDate(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day-1));
        Assert.IsNotNull(result);
        Assert.AreNotEqual("", result);
    }

    [TestMethod]
    public void TestGetHash()
    {
        var result = GeoHash.GetGeoHash(new DateTime(2025, 03, 14), 59, 17);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(AlmostEqual(59.81562, result[0], 0.0001));
        Assert.IsTrue(AlmostEqual(17.61293, result[1], 0.0001));
    }

    public static bool AlmostEqual(double a, double b, double diff)
    {
        var delta = Math.Abs(a - b);
        if (delta < diff)
            return true;
        else
            return false;
    }
}
