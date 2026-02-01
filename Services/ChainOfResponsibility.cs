// DTO : les valeurs par defaut
public class SeoDto {
    string Titre = "Mon titre par défaut" {get; set;};
    string H1 = "Mon h1 par défaut"{get; set;};
    string H2 = "Mon h2 par défaut"{get; set;};
    string Description = "Ma description par défaut"{get; set;};
    string Canonical = ""{get; set;};
}

// Interface
public interface ISeoEngine {
   void ApplySeo(CriteresDto criteria, SeoDto dto); 
}

// Classe abstraite : refaire implémentations  concrètes
 public abstract class SeoHandler
{
    private readonly ILogger<SeoHandler> _logger;
    private SeoHandler _successor;

    protected SeoHandler(ILogger<SeoHandler> logger)
    {
        _logger = logger;
    }

    public SeoHandler SetNext(SeoHandler successor)
    {
        _successor = successor;
        return successor;
    }

    public bool Handle(CriteresDto c, SeoDto dto)
    {
        try
        {
            _logger.LogDebug("Processing {Handler}", GetType().Name);

            if (Process(c, dto))
            {
                _logger.LogInformation("{Handler} handled the request", GetType().Name);
                return true;
            }

            _logger.LogDebug("{Handler} did not handle, passing to successor", GetType().Name);
            return _successor?.Handle(c, dto) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Handler}", GetType().Name);
            throw; // ou gestion métier
        }
    }

    protected abstract bool Process(CriteresDto c, SeoDto dto);
}




// Implémentations concrètes
public class DestinationHandler : SeoHandler
{
    private bool hasDestination(CriteresDto c) => c.zone != null || c.pays != null || c.region != null;
    private bool hasZone(CriteresDto c) => c.zone != null;
    private bool hasPays(CriteresDto c) => c.pays != null;
    private bool hasActivite(CriteresDto c) => c.activite.libelle != null;

    private readonly IDestinationSeoBuilder _builder; // TODO Implémenter l'interface
    public DestinationHandler(DestinationSeoBuilder builder) { 
        _builder = builder; 
    }

    public override bool Handle(CriteresDto c, SeoDto dto) { 
        if (!hasDestination(c)) 
            return Next?.Handle(c, dto) ?? false; 
        
        if (hasZone(c)) { 
            if (hasActivite(c)) 
                _builder.SetZoneActivite(dto); 
            else 
                _builder.SetZone(dto); 
            return true; 
        } 

        if (hasPays(c)) { 
            if (hasActivite(c)) 
                _builder.SetPaysActivite(dto, c); // cas spécifique activité 88 
            else 
                _builder.SetPays(dto); 
            return true; 
        } 

        _builder.SetDefaultDestination(dto); // si HasDestination = true et zone et pays = false
        return true; 
    }
}
public class DestinationSeoBuilder : IDestinationSeoBuilder { 
    // Les valeurs par défaut du SeoDto si criteres.hasDestination mais que règles non passées
    private string _titre = "Mon titre destination";
    private string _h1 = "Mon h1 destination";
    private string _h2 = "Mon h2 destination";
    private string _description = "Ma description destination";
    private string _canonical = "Ma canonical destination";

    public void SetZone(SeoDto dto) { 
        dto.Titre = "Titre zone"; 
        dto.H1 = "H1 zone"; 
        dto.Description = "Description zone"; 
    } 
    public void SetZoneActivite(SeoDto dto) { 
        dto.Titre = "Titre zone + activité"; 
        dto.H1 = "H1 zone + activité"; 
        dto.Description = "Description zone + activité"; 
    } 
    public void SetPays(SeoDto dto) { 
        dto.Titre = "Titre pays"; 
        dto.H1 = "H1 pays"; 
        dto.Description = "Description pays"; 
    } 
    public void SetPaysActivite(SeoDto dto, CriteresDto c) { 
        dto.Titre = $"Titre pays + activité"; 
        dto.H1 = $"H1 pays + activité"; 
        dto.Description = $"Description pays + activité {SpecificActivite(c)}"; 
    }
    public void SetDefaultDestination(SeoDto dto) { 
        dto.Titre = _titre; 
        dto.H1 = _h1; 
        dto.Description = _description; 
    }

    private string SpecificActivite(CriteresDto c) => c.codeTheme == 88 ? "trek" : "";
}


public class ActiviteHandler : SeoHandler
{
    private readonly SeoHandler _subChain;
    private bool hasActivite(CriteresDto c) => c.activite.libelle != null;
    private bool hasEnvironnement(CriteresDto c) => c.codeEnvironnement != null && c.codeEnvironnement > 0;

    public ActiviteHandler(ActiviteSeoBuilder builder) { 
        _builder = builder; 
    }

    public override bool Handle(CriteresDto c, SeoDto dto)
    {
        if (!hasActivite(c)) 
            return Next?.Handle(c, dto) ?? false; 
        
        if (hasEnvironnement(c)) { 
            _builder.SetActiviteEnvironnement(dto); 
            return true; 
        } 

        _builder.SetDefaultActivite(dto); 
        return true; 
    }
}
public class HcHandler : SeoHandler {}
public class SpecificsHandler : SeoHandler {} // DM, nouveautés, etc.


public class DefaultHandler : SeoHandler { 
    public override bool Handle(Criteria c, SeoDto dto) {
        dto.canonical = c.currentUrl; 
        return true; 
    } 
}


// Use

public class SeoEngine: ISeoEngine
{
    private readonly SeoHandler _root;


    public SeoEngine(DestinationHandler d,...)
    {
//a.SetNext(b).SetNext(c).SetNext(d);
    _root = a;
        // Règles Destination > Règles Activité > Règles par défaut
        _root = new DestinationHandler();
        _root
            .SetNext(new ActivitesHandler())
            .SetNext(new DefaultHandler());
    }

    public SeoDto ApplySeo(CriteresDto criteria, SeoDto dto)
    {
        _root.Handle(criteria, dto);

        dto.Description = ReplaceTel(dto.Description, criteria); 
        dto.Description = ReplaceNb(dto.Description, dto.Nb);

        return dto;
    }
    // Gestion des TOKENS
    private string ReplaceTel(string text, CriteresDto c) { 
        var tel = configService.GetTelFor(c.X); 
        return text.Replace("[NUMERO_TELEPHONE]", tel); 
    } 
    private string ReplaceNb(string text, int nb) { 
        return text.Replace("[X]", nb.ToString()); 
    }
}

var myService = engine.ApplySeo(criteria, dto); // injecter le SeoEngine dans mon appel du service


/// Autofac

// Module commun
public class SeoModule : Module { 
    protected override void Load(ContainerBuilder builder) { 
        builder.RegisterType<DestinationHandler>(); 
        builder.RegisterType<SeoEngine>().As<ISeoEngine>(); 
    }
}

// Module TDV
public class SiteAModule : Module { 
    protected override void Load(ContainerBuilder builder) { 
        builder.RegisterType<DestinationSeoBuilderA>() 
               .As<IDestinationSeoBuilder>() 
               .SingleInstance(); 
        }
    } 
}

