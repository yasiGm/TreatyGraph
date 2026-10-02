using TreatyGraph.Core.Csv;

namespace TreatyGraph.Tests;

public class CsvReaderTests
{
    [Fact]
    public void Parses_quoted_fields_with_commas_and_escaped_quotes()
    {
        var rows = CsvReader.ParseDictionaries("a,b\n\"x, y\",\"say \"\"hi\"\"\"\n");

        var row = Assert.Single(rows);
        Assert.Equal("x, y", row["a"]);
        Assert.Equal("say \"hi\"", row["b"]);
    }

    [Fact]
    public void Handles_crlf_and_lone_cr_line_endings()
    {
        Assert.Equal(2, CsvReader.ParseDictionaries("a,b\r\n1,2\r\n3,4\r\n").Count);
        Assert.Equal(2, CsvReader.ParseDictionaries("a,b\r1,2\r3,4").Count);
    }

    [Fact]
    public void Skips_blank_lines_and_pads_short_rows()
    {
        var rows = CsvReader.ParseDictionaries("a,b,c\n1,2\n\n3,4,5\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(string.Empty, rows[0]["c"]);
        Assert.Equal("5", rows[1]["c"]);
    }
}
