public class ListeResultatsController
{
    private readonly IOrchestrator<BuildParametersListeResultats, ListeResultatsViewModel> _orchestrator;
    private readonly string _PathView = "/Views/Index.cshtml";
    
    public ListeResultatsController(
        IOrchestrator<BuildParametersListeResultats, 
        ListeResultatsViewModel> orchestrator
    ) => _orchestrator = orchestrator;

    public ActionResult Index(string origin) => View(
        _PathView, 
        _orchestrator.BuildVm(
            new BuildParametersListeResultats(
                origin, 
                Request.RawUrl,
                "TDV"
            )
        )
    );

    public ActionResult Filter() => {
        var vmFiltres = _orchestrator.Filter(new BuildParametersListeResultats(Request.RawUrl + "?duree=1semaine","TDV")));
        // vmFiltres = {B: Products; C: Filtres}
        //return PartialView(
        //   "_Produits",
        //vmFiltres.Products);
    }
}
