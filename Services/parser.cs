using System;
using System.Collections.Generic;
using System.Web;

namespace URIParserWithHandlers
{
    // DTO exemple
    public class MyDto
    {
        public string User { get; set; }
        public string Action { get; set; }
        public int? Id { get; set; }
        public string Language { get; set; }
    }

    public class UriParserWithHandler
    {
        // Dictionnaire pour associer les keys à des méthodes de traitement
        private Dictionary<string, Action<MyDto, string, string>> _handlers;

        public UriParserWithHandler()
        {
            _handlers = new Dictionary<string, Action<MyDto, string, string>>();
            InitializeHandlers();
        }

        // Initialisation des handlers pour chaque key
        private void InitializeHandlers()
        {
            // Exemple de handler pour la clé 'user'
            _handlers["user"] = (dto, key, value) => { dto.User = value; Console.WriteLine($"Handling user: {value}"); };

            // Exemple de handler pour la clé 'action'
            _handlers["action"] = (dto, key, value) => { dto.Action = value; Console.WriteLine($"Handling action: {value}"); };

            // Exemple de handler pour la clé 'id'
            _handlers["id"] = (dto, key, value) => { dto.Id = int.TryParse(value, out var id) ? id : (int?)null; Console.WriteLine($"Handling id: {value}"); };

            // Exemple de handler pour la clé 'lang'
            _handlers["lang"] = (dto, key, value) => { dto.Language = value; Console.WriteLine($"Handling language: {value}"); };
        }

        // Méthode pour analyser l'URL et appeler les méthodes associées aux segments
        public void ParseUri(string url)
        {
            Uri uri = new Uri(url);

            // Extraction des segments (chemin de l'URL)
            string[] pathSegments = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var dto = new MyDto();

            foreach (var segment in pathSegments)
            {
                var keyValue = segment.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = keyValue[1];

                    // Si un handler est trouvé pour cette clé, l'appel de la méthode correspondante
                    if (_handlers.ContainsKey(key))
                    {
                        _handlers[key](dto, key, value);
                    }
                    else
                    {
                        Console.WriteLine($"No handler for key: {key}");
                    }
                }
            }

            // Extraction des paramètres de la query string (après le ?)
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in queryParameters)
            {
                var value = queryParameters[key];
                if (_handlers.ContainsKey(key))
                {
                    _handlers[key](dto, key, value);
                }
                else
                {
                    Console.WriteLine($"No handler for query parameter: {key}");
                }
            }

            // Affichage de l'objet DTO après traitement
            Console.WriteLine($"\nDTO Result: User = {dto.User}, Action = {dto.Action}, Id = {dto.Id}, Language = {dto.Language}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Exemple d'URL à analyser
            string url = "https://www.example.com/user=john_doe/action=edit/id=123/lang=fr";

            // Création du parser et traitement de l'URL
            var uriParser = new UriParserWithHandler();
            uriParser.ParseUri(url);
        }
    }
}
