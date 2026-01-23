/*
Tu crées un DTO quand :

    la donnée doit être mise dans le BuildContext

    la donnée doit être consommée par un autre step

    la donnée doit être transformée en ViewModel

    la donnée doit être indépendante de la présentation

Tu crées un DTA quand :

    la donnée doit être mise dans le BuildContext

    la donnée doit être produite par un step

    la donnée doit être consommée par un autre step

    la donnée doit être transformée en ViewModel à la fin
*/
public class InformationDto
{
    public string Url { get; set; }
    public string Title { get; set; }
}
