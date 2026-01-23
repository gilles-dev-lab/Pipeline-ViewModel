public sealed class ConverterListeResultats
    : IBuildContextToVmConverter<ListeResultatsViewModel>
{
    ///<summary>
    /// on mappe le BuildContext pour le transformer en ListeResultatsViewModel
    ///</summary>
    public ListeResultatsViewModel Convert(BuildContext ctx)
    {
        var vm = new ListeResultatsViewModel();
        vm.criteres = ConverterCritere(ctx);
        // logique spécifique
        return vm;
    }
    private CritereViewModel ConverterCritere(BuildContext ctx) {
        var criteres = ctx.Get<CriteriaStep>();
        ///.....
        ///.....
        return criteres;
    }
    ///... autres méthodes privées
}