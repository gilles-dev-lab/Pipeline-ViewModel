using System;
using System.Web;
using System.Collections.Generic;


namespace demo
{
	public class MyDto {
		public string User { get; set; }
        public string Action { get; set; }
        public int? Id { get; set; }
        public string Language { get; set; }
	}
	public class UriParserWithHandler
	{
		private Dictionary<string, Action<MyDto, string, string>> _handlers;
		public UriParserWithHandler()
		{
			_handlers = new Dictionary<string, Action<MyDto, string, string>>();
			InitializeHandlers();
		}
		private void InitializeHandlers()
		{
			_handlers["ps"] = (dto, key, value) => { dto.User = value; Console.WriteLine($"Handling pays: {value}"); };
			_handlers["action"] = (dto, key, value) => { dto.Action = value; Console.WriteLine($"Handling action: {value}"); };
			_handlers["duree"] = (dto, key, value) => { dto.Id = int.TryParse(value, out var id) ? id : (int?)null; Console.WriteLine($"Handling duree: {value}"); };
			_handlers["pays"] = (dto, key, value) => { dto.Language = value; Console.WriteLine($"Handling Pays: {value}"); };
			_handlers["dm"] = (dto, key, value) => {  dto.Action = key; Console.WriteLine($"Handling Dernieres minutes"); };
		}
		public void ParseUri(string url)
		{
			Uri uri = new Uri(url);
			string[] pathSegments = uri.Segments;
			var dto = new MyDto();
			foreach (var segment in pathSegments)
			{
				var keyValue = segment.Split('-');
				if (keyValue.Length == 2)
				{
					var key = keyValue[0];
					var value = keyValue[1].Replace("/","");
					if (_handlers.ContainsKey(key))
						_handlers[key](dto, key, value);
					else
						Console.WriteLine($"No handler for key: {key}");
				}
			}
			var queryParameters = HttpUtility.ParseQueryString(uri.Query);
			foreach (string key in queryParameters)
			{
				var value = queryParameters[key];
				if (_handlers.ContainsKey(key))
					_handlers[key](dto, key, value);
				else
					Console.WriteLine($"No handler for query parameter: {key}");
			}
			
			if(uri.PathAndQuery.ToString() == "/dernieres-minutes")
		        _handlers["dm"](dto, "dm", "value");
		    else 
			    Console.WriteLine($"\nDTO Result: {uri.AbsolutePath.Substring(1)} \nUser = {dto.User}, \nAction = {dto.Action},\n Id = {dto.Id}, \nLanguage = {dto.Language}");
			
		}
		
		private bool SpecifiqueUrl(Uri uri,MyDto dto) {
		    if(uri.ToString() == "/dernieres-minutes?duree=1")
		        _handlers["dm"](dto, "dm", "value");
		        return true;
		  return false;
		}
	}
}
//USe
//var uriParser = new UriParserWithHandler();
//uriParser.ParseUri("https://www.example.com/ps-france/action-edit/duree-123?pays=FRA");
//uriParser.ParseUri("https://www.example.com/dernieres-minutes");
