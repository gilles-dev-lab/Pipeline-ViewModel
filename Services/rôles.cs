
// les règles du handler 
private readonly List<Rule> _rules = new List<Rule>
{
    new Rule(
        c => hasZone(c) && hasActivite(c),
        (dto, c) => _builder.SetZoneActivite(dto)
    ),
    new Rule(
        c => hasZone(c),
        (dto, c) => _builder.SetZone(dto)
    ),
    new Rule(
        c => hasPays(c) && hasActivite(c),
        (dto, c) => _builder.SetPaysActivite(dto, c)
    ),
    new Rule(
        c => hasPays(c),
        (dto, c) => _builder.SetPays(dto)
    )
};

// dans le process()

foreach (var rule in _rules)
{
    if (!rule.When(c))
        continue;

    rule.Then(dto, c);
    return true;
}

return false;

// Classe rule
class Rule
{
    public Func<Context, bool> When { get; }
    public Action<Dto, Context> Then { get; }

    public Rule(Func<Context, bool> when, Action<Dto, Context> then)
    {
        When = when;
        Then = then;
    }
}
