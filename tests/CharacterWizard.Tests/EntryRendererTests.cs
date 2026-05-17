using CharacterWizard.Data.EntryRendering;

namespace CharacterWizard.Tests;

public class EntryRendererTests
{
    [Fact]
    public void RenderString_returns_empty_for_null_or_empty()
    {
        Assert.Equal("", EntryRenderer.RenderString(null));
        Assert.Equal("", EntryRenderer.RenderString(""));
    }

    [Fact]
    public void RenderString_passes_through_plain_text()
    {
        Assert.Equal("Hello world.", EntryRenderer.RenderString("Hello world."));
    }

    [Fact]
    public void RenderString_escapes_html_special_chars()
    {
        Assert.Equal("&lt;script&gt;alert(1)&lt;/script&gt;",
            EntryRenderer.RenderString("<script>alert(1)</script>"));
        Assert.Equal("a &amp; b", EntryRenderer.RenderString("a & b"));
        Assert.Equal("&quot;x&quot;", EntryRenderer.RenderString("\"x\""));
    }

    [Fact]
    public void RenderString_wraps_b_tag_as_strong()
    {
        Assert.Equal("<strong>bold</strong>", EntryRenderer.RenderString("{@b bold}"));
        Assert.Equal("<strong>bold</strong>", EntryRenderer.RenderString("{@bold bold}"));
    }

    [Fact]
    public void RenderString_wraps_i_tag_as_em()
    {
        Assert.Equal("<em>it</em>", EntryRenderer.RenderString("{@i it}"));
        Assert.Equal("<em>it</em>", EntryRenderer.RenderString("{@italic it}"));
    }

    [Fact]
    public void RenderString_wraps_u_tag_as_u()
    {
        Assert.Equal("<u>uu</u>", EntryRenderer.RenderString("{@u uu}"));
        Assert.Equal("<u>uu</u>", EntryRenderer.RenderString("{@underline uu}"));
    }

    [Fact]
    public void RenderString_handles_text_around_tags()
    {
        Assert.Equal(
            "see <strong>this</strong> please",
            EntryRenderer.RenderString("see {@b this} please"));
    }

    [Fact]
    public void RenderString_escapes_html_inside_tag_args()
    {
        Assert.Equal(
            "<strong>&lt;evil&gt;</strong>",
            EntryRenderer.RenderString("{@b <evil>}"));
    }
}
