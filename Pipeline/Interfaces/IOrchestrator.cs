public interface IOrchestrator<TParameters, TViewModel>
    where TParameters : class
    where TViewModel : class
{
    TViewModel BuildVm(TParameters parameters = null);
}
