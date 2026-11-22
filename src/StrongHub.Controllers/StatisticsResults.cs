namespace StrongHub.Controllers;

/// <summary>One row of the weight and goals table.</summary>
public record StatRow(string Metric, string Start, string End, string Change, string Goal, string Progress);

/// <summary>Evolution of the member in a period: a summary line and the table rows.</summary>
public record EvolutionResult(string Summary, IReadOnlyList<StatRow> Rows);

/// <summary>One row of a visit ranking.</summary>
public record RankingRow(int Position, string Member, string Username, int Visits);

/// <summary>Both rankings of a period and a sentence about the logged-in member.</summary>
public record RankingResult(
    IReadOnlyList<RankingRow> MostVisits,
    IReadOnlyList<RankingRow> FewestVisits,
    string MySummary);
