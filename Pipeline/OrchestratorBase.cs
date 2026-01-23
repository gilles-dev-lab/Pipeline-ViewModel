public abstract class OrchestratorBase<TParameters, TViewModel>
    where TParameters : class
    where TViewModel : class
{
    protected readonly IDagBuilder Dag;
    protected readonly IBuildContextToVmConverter<TViewModel> Converter;

    protected OrchestratorBase(
        IDagBuilder dag,
        IBuildContextToVmConverter<TViewModel> converter)
    {
        Dag = dag;
        Converter = converter;
    }

    public TViewModel BuildVm(TParameters parameters = null)
    {
        var ctx = Dag.BuildContext(parameters);
        return Converter.Convert(ctx);
    }
}