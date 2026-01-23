/// <summary>
/// Via la classe mère OrchestratorBase :
/// - Build le contexte 
/// - Mappe le résultat pour le transformer en ListeResultatsViewModel
/// </summary>
public sealed class OrchestratorListeResultats
    : OrchestratorBase<
        BuildParametersListeResultats,
        ListeResultatsViewModel>,
      IOrchestrator<BuildParametersListeResultats, ListeResultatsViewModel>
{
    public OrchestratorListeResultats(
        IDagBuilder dag,
        IBuildContextToVmConverter<ListeResultatsViewModel> converter)
        : base(dag, converter)
    {
    }
    // Voir comment utiliser le DAG pour gérer filtres, tri..
    // public ListeResultatsViewModel Filter(BuildParametersListeResultats parameters = null) => {
    //     var ctx = Dag.BuildContext(parameters);

    //     // Exécute uniquement ce qui est nécessaire pour produire Filters
    //     dagBuilder.ExecuteUntil<Filters>(ctx);

    //     return Converter.ToListeResultatViewModel(ctx); // ToProductsVM, ToFiltersVM...
    // }
}
