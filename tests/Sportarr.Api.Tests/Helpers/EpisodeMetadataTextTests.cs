using Sportarr.Api.Helpers;
using Sportarr.Api.Models;
using FluentAssertions;

namespace Sportarr.Api.Tests.Helpers;

/// <summary>
/// The per-league spoiler options for the media-server metadata API: hub
/// write-ups of finished events usually name the winner, and the bare event
/// title hides the round and venue. Both options rewrite only what the API
/// sends, in the neutral line the hub uses for its own spoiler-free
/// overviews.
/// </summary>
public class EpisodeMetadataTextTests
{
    private const string WriteUp = "Toprak Razgatlioglu won Race 2 at Jerez.";
    private const string NeutralLine =
        "Pirelli Spanish Round - Race 2 · SBK 2026 · Round 12 · Circuito de Jerez – Ángel Nieto";

    private static Event Race() => new()
    {
        Title = "Pirelli Spanish Round - Race 2",
        Sport = "Motorsport",
        Description = WriteUp,
        Season = "2026",
        Round = "12",
        Venue = "Circuito de Jerez – Ángel Nieto",
    };

    private static League Sbk(bool neutralTitles = false, bool hideSpoilers = false) => new()
    {
        Name = "SBK",
        Sport = "Motorsport",
        NeutralEpisodeTitles = neutralTitles,
        HideEpisodeSpoilers = hideSpoilers,
    };

    [Fact]
    public void OptionsOff_ServeTheStoredHubText()
    {
        var evt = Race();

        EpisodeMetadataText.Title(evt, Sbk()).Should().Be("Pirelli Spanish Round - Race 2");
        EpisodeMetadataText.Summary(evt, Sbk()).Should().Be(WriteUp);
    }

    [Fact]
    public void UnknownLeague_ServesTheStoredHubText()
    {
        var evt = Race();

        EpisodeMetadataText.Title(evt, null).Should().Be(evt.Title);
        EpisodeMetadataText.Summary(evt, null).Should().Be(WriteUp);
    }

    [Fact]
    public void NeutralEpisodeTitles_PutsRoundAndVenueInTheTitle()
    {
        var league = Sbk(neutralTitles: true);

        EpisodeMetadataText.Title(Race(), league).Should().Be(NeutralLine);
        EpisodeMetadataText.Summary(Race(), league).Should().Be(WriteUp);
    }

    [Fact]
    public void HideEpisodeSpoilers_ReplacesTheWriteUpWithTheNeutralLine()
    {
        var league = Sbk(hideSpoilers: true);

        EpisodeMetadataText.Summary(Race(), league).Should().Be(NeutralLine);
        EpisodeMetadataText.Title(Race(), league).Should().Be("Pirelli Spanish Round - Race 2");
    }

    [Fact]
    public void HideEpisodeSpoilers_EventWithoutDescription_StillGetsTheNeutralLine()
    {
        var evt = Race();
        evt.Description = null;

        EpisodeMetadataText.Summary(evt, Sbk(hideSpoilers: true)).Should().Be(NeutralLine);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    public void MissingRound_IsLeftOut(string? round)
    {
        var evt = Race();
        evt.Round = round;

        EpisodeMetadataText.Title(evt, Sbk(neutralTitles: true))
            .Should().Be("Pirelli Spanish Round - Race 2 · SBK 2026 · Circuito de Jerez – Ángel Nieto");
    }

    [Fact]
    public void MissingVenue_IsLeftOut()
    {
        var evt = Race();
        evt.Venue = null;

        EpisodeMetadataText.Title(evt, Sbk(neutralTitles: true))
            .Should().Be("Pirelli Spanish Round - Race 2 · SBK 2026 · Round 12");
    }

    [Fact]
    public void TeamEvent_LeadsWithTheMatchup()
    {
        var game = new Event
        {
            Title = "Arsenal vs Chelsea",
            Sport = "Soccer",
            HomeTeamName = "Arsenal",
            AwayTeamName = "Chelsea",
            Season = "2025-2026",
            Round = "10",
            Venue = "Emirates Stadium",
            Description = "Arsenal won 3-1.",
        };
        var league = new League { Name = "English Premier League", Sport = "Soccer", HideEpisodeSpoilers = true };

        EpisodeMetadataText.Summary(game, league)
            .Should().Be("Arsenal vs Chelsea · English Premier League 2025-2026 · Round 10 · Emirates Stadium");
    }
}
