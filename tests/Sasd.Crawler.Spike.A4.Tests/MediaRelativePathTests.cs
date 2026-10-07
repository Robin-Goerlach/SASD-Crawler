using Sasd.Crawler.Spike.A4.Media;

namespace Sasd.Crawler.Spike.A4.Tests;

public sealed class MediaRelativePathTests
{
    [Fact]
    public void ID_005_Drive_letter_is_not_part_of_stored_relative_path()
    {
        var relative = MediaRelativePath.FromAbsolutePath("E:\\", "E:\\archive\\report.pdf");
        Assert.Equal("archive/report.pdf", relative);
        Assert.Equal("F:\\archive\\report.pdf", MediaRelativePath.ToAbsolutePath("F:\\", relative));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("folder/../../outside.txt")]
    public void SEC_004_Relative_path_cannot_escape_media_root(string relative)
    {
        Assert.Throws<ArgumentException>(() => MediaRelativePath.ToAbsolutePath("E:\\", relative));
    }
}
