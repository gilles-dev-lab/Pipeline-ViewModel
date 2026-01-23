public interface IBuildContextToVmConverter<TViewModel>
    where TViewModel : class
{
    TViewModel Convert(BuildContext ctx);
}
