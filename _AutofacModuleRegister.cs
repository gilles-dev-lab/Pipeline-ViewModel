using Autofac;

public class ModuleListeResultats : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // =========================
        // Services métiers
        // =========================
        builder.RegisterType<ServiceAImpl>()
               .As<IServiceCritere>()
               .InstancePerRequest();

        builder.RegisterType<ServiceBImpl>()
               .As<IServiceProducts>()
               .InstancePerRequest();

        builder.RegisterType<ServiceCImpl>()
               .As<IServiceFilters>()
               .InstancePerRequest();

        // =========================
        // Steps (résolus explicitement)
        // =========================
        builder.RegisterType<CriteriaStep>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductsStep>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<FiltersStep>()
               .AsSelf()
               .InstancePerDependency();

        // =========================
        // Converter VM
        // =========================
        builder.RegisterType<ListeResultatsConverter>()
               .As<IBuildContextToVmConverter<ListeResultatsViewModel>>()
               .InstancePerDependency();

        // =========================
        // Orchestrator (configuration)
        // =========================
        builder.Register(c =>
        {
            var steps = new IStep[]
            {
                c.Resolve<CriteriaStep>(),
                c.Resolve<ProductsStep>(),
                c.Resolve<FiltersStep>()
            };

            var dag = new DagBuilder(steps);

            return new OrchestratorListeResultats(
                dag,
                c.Resolve<IBuildContextToVmConverter<ListeResultatsViewModel>>()
            );
        })
        .As<IOrchestrator<BuildParametersListeResultats, ListeResultatsViewModel>>()
        .InstancePerDependency();

    }
}
