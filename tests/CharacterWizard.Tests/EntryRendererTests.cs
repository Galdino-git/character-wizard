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

public class EntryRenderer_EntityRefTagsTests
{
    [Theory]
    [InlineData("@spell",   "spell",   "Fireball")]
    [InlineData("@item",    "item",    "Longsword")]
    [InlineData("@condition", "condition", "prone")]
    [InlineData("@feat",    "feat",    "Tough")]
    [InlineData("@skill",   "skill",   "Perception")]
    [InlineData("@action",  "action",  "Dash")]
    [InlineData("@sense",   "sense",   "Darkvision")]
    [InlineData("@creature","creature","Goblin")]
    [InlineData("@feature", "feature", "Rage")]
    [InlineData("@classFeature",    "classfeature",    "Action Surge")]
    [InlineData("@subclassFeature", "subclassfeature", "Eldritch Strike")]
    [InlineData("@optfeature","optfeature","Agonizing Blast")]
    public void Tag_renders_as_cw_ref_span_with_default_source(string tag, string expectedCat, string name)
    {
        var html = EntryRenderer.RenderString($"{{{tag} {name}}}");
        Assert.Contains($"data-cw-cat=\"{expectedCat}\"", html);
        Assert.Contains($"data-cw-name=\"{name}\"", html);
        Assert.Contains("class=\"cw-ref\"", html);
        Assert.Contains($">{System.Net.WebUtility.HtmlEncode(name)}<", html);
    }

    [Fact]
    public void Tag_with_explicit_source_emits_source_attribute()
    {
        var html = EntryRenderer.RenderString("{@spell Fireball|XPHB}");
        Assert.Contains("data-cw-source=\"XPHB\"", html);
        Assert.Contains(">Fireball<", html);
    }

    [Fact]
    public void Tag_with_display_alias_uses_alias_as_text()
    {
        var html = EntryRenderer.RenderString("{@spell Fireball|PHB|fb}");
        Assert.Contains("data-cw-name=\"Fireball\"", html);
        Assert.Contains("data-cw-source=\"PHB\"", html);
        Assert.Contains(">fb<", html);
    }

    [Fact]
    public void Unknown_tag_renders_display_text_fallback()
    {
        Assert.Equal("Foo", EntryRenderer.RenderString("{@xyz Foo|PHB|Foo}"));
        Assert.Equal("Foo", EntryRenderer.RenderString("{@nonsense Foo}"));
    }

    [Fact]
    public void Entity_tag_escapes_quotes_in_attributes()
    {
        var html = EntryRenderer.RenderString("{@spell Bad\"Name}");
        Assert.DoesNotContain("Bad\"Name\"", html); // never a raw injection
        Assert.Contains("data-cw-name=\"Bad&quot;Name\"", html);
    }
}

public class EntryRenderer_FormattingTagsTests
{
    [Fact]
    public void Damage_renders_dice_formula_with_class()
    {
        var html = EntryRenderer.RenderString("{@damage 8d6}");
        Assert.Contains("class=\"cw-roll\"", html);
        Assert.Contains(">8d6<", html);
    }

    [Fact]
    public void Damage_with_type_keeps_type_in_text()
    {
        var html = EntryRenderer.RenderString("{@damage 8d6 fire}");
        Assert.Contains(">8d6 fire<", html);
    }

    [Fact]
    public void Dice_tag_renders_formula()
    {
        var html = EntryRenderer.RenderString("{@dice 1d4+1}");
        Assert.Contains("class=\"cw-roll\"", html);
        Assert.Contains(">1d4+1<", html);
    }

    [Fact]
    public void Hit_tag_renders_signed_modifier()
    {
        Assert.Contains("+5", EntryRenderer.RenderString("{@hit 5}"));
        Assert.Contains("&minus;2", EntryRenderer.RenderString("{@hit -2}"));
    }

    [Fact]
    public void Dc_tag_renders_DC_prefix()
    {
        var html = EntryRenderer.RenderString("{@dc 15}");
        Assert.Contains("DC 15", html);
    }

    [Fact]
    public void Highlight_tag_renders_mark()
    {
        Assert.Equal("<mark>important</mark>", EntryRenderer.RenderString("{@h important}"));
    }

    [Fact]
    public void Note_tag_renders_em()
    {
        var html = EntryRenderer.RenderString("{@note minor remark}");
        Assert.Contains("<em", html);
        Assert.Contains("minor remark", html);
    }

    [Fact]
    public void Atk_tag_translates_known_codes()
    {
        // 5etools uses "mw" = melee weapon, "rw" = ranged weapon, "ms" = melee spell, "rs" = ranged spell.
        Assert.Contains("Melee Weapon Attack", EntryRenderer.RenderString("{@atk mw}"));
        Assert.Contains("Ranged Spell Attack", EntryRenderer.RenderString("{@atk rs}"));
    }

    [Fact]
    public void Scaledamage_renders_base_formula()
    {
        // 5etools format: "{@scaledamage 8d6|3-9|1d6}" = base 8d6, scales from levels 3-9, +1d6 per level
        var html = EntryRenderer.RenderString("{@scaledamage 8d6|3-9|1d6}");
        Assert.Contains(">8d6<", html);
    }
}
