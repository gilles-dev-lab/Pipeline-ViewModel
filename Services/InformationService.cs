using Application.Dto
//le service renvoie un objet métier et pas un ViewModel
public sealed class ConverterListeResultats {

    public InformationDto GetInformation()
    {
        // Traitement...
        // ...
        return new InformationDto { Url = "foo", Title = "Bar" };
    }
} 
