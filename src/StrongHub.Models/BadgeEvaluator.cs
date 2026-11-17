namespace StrongHub.Models;

/// <summary>Runs the rules of a given kind. It does not know any concrete rule.</summary>
public class BadgeEvaluator
{
    private readonly List<IBadgeRule> _rules;

    public BadgeEvaluator(IEnumerable<IBadgeRule> rules) => _rules = rules.ToList();

    public IReadOnlyList<IBadgeRule> GetRules(BadgeKind kind) =>
        _rules.Where(rule => rule.Kind == kind).ToList();

    public IReadOnlyList<IBadgeRule> GetSatisfiedRules(BadgeContext context, BadgeKind kind) =>
        GetRules(kind).Where(rule => rule.IsSatisfied(context)).ToList();
}
