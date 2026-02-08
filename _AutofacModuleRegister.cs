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
        // Converter VM
        // =========================
        /*builder.RegisterType<ListeResultatsConverter>()
               .As<IBuildContextToVmConverter<ListeResultatsViewModel>>()
               .InstancePerDependency();
*/
        // =========================
        // Orchestrator (configuration)
        // =========================
        // 1️⃣ Enregistrer toutes les étapes fermées
builder.RegisterAssemblyTypes(stepAssemblies)
       .AsClosedTypesOf(typeof(IStep<,>))
       .InstancePerDependency();

// 2️⃣ Enregistrer le DAG par use case
builder.RegisterType<DagBuilder<BuildParametersListeResultats, ListeResultatsViewModel>>()
       .As<IDagBuilder<BuildParametersListeResultats, ListeResultatsViewModel>>()
       .InstancePerDependency();

// 3️⃣ Enregistrer le converter (si ce n'est pas déjà fait)
builder.RegisterType<BuildContextToVmConverter<ListeResultatsViewModel>>()
       .As<IBuildContextToVmConverter<ListeResultatsViewModel>>()
       .InstancePerDependency();

// 4️⃣ Enregistrer l’orchestrator, Autofac injectera DAG + converter automatiquement
builder.RegisterType<OrchestratorListeResultats>()
       .As<IOrchestrator<BuildParametersListeResultats, ListeResultatsViewModel>>()
       .InstancePerDependency();


    }
}
