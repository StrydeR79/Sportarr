using Sportarr.Api.Models;

namespace Sportarr.Api.Helpers;

/// <summary>
/// The episode title and summary the metadata agent API hands to
/// Plex/Jellyfin/Emby, after the league's spoiler options.
///
/// The hub overwrites Event.Title and Event.Description on every sync, and
/// for finished sessions the description is usually a write-up that names
/// the winner. Rewriting the text here, at the API edge, keeps the stored
/// hub data intact and needs no change in the media-server agents.
///
/// The neutral line is the same shape the hub synthesizes for its own
/// spoiler-free overviews ("{Title} · {League} {Season} · Round {N} ·
/// {Venue}"), so episodes look alike whichever source wrote them. Parts the
/// event does not have are left out.
/// </summary>
public static class EpisodeMetadataText
{
    private const string Separator = " · ";

    public static string Title(Event evt, League? league) =>
        league?.NeutralEpisodeTitles == true ? NeutralLine(evt, league.Name) : evt.Title;

    public static string? Summary(Event evt, League? league) =>
        league?.HideEpisodeSpoilers == true ? NeutralLine(evt, league.Name) : evt.Description;

    private static string NeutralLine(Event evt, string leagueName)
    {
        var parts = new List<string> { Subject(evt) };

        var leagueSeason = $"{leagueName} {evt.Season}".Trim();
        if (leagueSeason.Length > 0)
            parts.Add(leagueSeason);

        // The hub reports "0" for events that are not part of a round.
        if (!string.IsNullOrWhiteSpace(evt.Round) && evt.Round != "0")
            parts.Add($"Round {evt.Round}");

        if (!string.IsNullOrWhiteSpace(evt.Venue))
            parts.Add(evt.Venue);

        return string.Join(Separator, parts);
    }

    private static string Subject(Event evt) =>
        !string.IsNullOrWhiteSpace(evt.HomeTeamName) && !string.IsNullOrWhiteSpace(evt.AwayTeamName)
            ? $"{evt.HomeTeamName} vs {evt.AwayTeamName}"
            : evt.Title;
}
