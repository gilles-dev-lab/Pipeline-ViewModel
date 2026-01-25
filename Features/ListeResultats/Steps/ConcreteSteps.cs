using System;

///<summary>
/// Les steps orchestrent l’exécution des services en fonction des dépendances du pipeline.
/// Les services encapsulent la logique métier et les accès aux données.
/// Les steps ne contiennent aucune logique métier, seulement de la coordination.
/// Un service répond à “comment faire ?”
/// Un step répond à “dans quel ordre et avec quoi ?”
// /[PrefixeController]/
//     /Services/
//         /Dtos/          ← données métier (DTO)
//         /Interfaces/    ← contrats des services (optionnel mais propre)
//         /Implementations/  ← implémentations des services
///</summary>
/// CriteriaStep: no dependency -> produces Criteria
public sealed class CriteriaStep : StepBase<Criteria>
{
    private readonly IServiceCritere _service;
    public CriteriaStep(IServiceCritere service) => _service = service;

    protected override Criteria ExecuteTyped(BuildContext ctx)
    {
// Service : Récupère les données brutes
//public List<CriteresDto> GetCriteresData() { /* ... */ } 
        return _service.GetCriteresData();
    }
}

/// ProductsStep: depends on Criteria -> produces Products
public sealed class ProductsStep : StepBase<Products>, IDependsOn<Criteria>
{
    private readonly IServiceProducts _service;
    public ProductsStep(IServiceProducts service) => _service = service;

    protected override Products ExecuteTyped(BuildContext ctx)
    {
        /// Require<>() gère l'exception si le context nécessaire n'est pas dispo : 
        /// > validation des données (pdt exécution)
        
        //var p = Require<BuildParameters>(ctx);
        //string path = p.CurrentPath;
        //string site = p.SiteCode;
        var a = Require<Criteria>(ctx);
        return _service.GetB(a);
    }
}
// public class StepB1 : StepBase<BVm>
// {
//     protected override Products ExecuteTyped(BuildContext ctx)
//     {
//         BuildParameters p;
//         if (ctx.TryGet<BuildParameters>(out p) && p != null)
//         {
//             
//             // utiliser ces params pour appeler un service ou filtrer la logique
//         }
//         //...
//     }
// }


/// FiltersStep: depends on Criteria and Products -> produces CData
public sealed class FiltersStep : StepBase<Filters>, IDependsOn<Criteria, Products>
{
    private readonly IServiceFilters _service;
    public FiltersStep(IServiceFilters service) => _service = service;

    protected override Filters ExecuteTyped(BuildContext ctx)
    {
        var a = Require<Criteria>(ctx);
        var b = Require<Products>(ctx);
        return _service.GetFilters(a, b);
    }
}