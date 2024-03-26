using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestDbModel;

class Program
{
    static async Task<string> FetchPayload(string url)
    {
        Console.WriteLine("Getting content from " + url);

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Failed to retrieve content. Status code: {response.StatusCode}");
            }
        }
    }

    static async Task PopulateDatabase()
    {
        var characters = await FetchCharacters("https://rickandmortyapi.com/api/character");

        using (var db = new TestDbContext())
        {
            foreach (var character in characters)
            {
                db.Characters.Add(character);
            }

            db.SaveChanges();
        }
    }

    static async Task<IList<Character>> FetchCharacters(string url)
    {
        var response = await FetchPayload($"{url}?page=1");
        var element = JsonSerializer.Deserialize<dynamic>(response) ?? new JsonElement();

        var infoElement = element.GetProperty("info");
        var characterCount = infoElement.GetProperty("count").GetInt32();
        var pageCount = infoElement.GetProperty("pages").GetInt32();
        
        var characters = new List<Character>() { Capacity = characterCount };

        ParseCharacters(element.GetProperty("results"), characters);

        for (int i = 2; i <= pageCount; i++)
        {
            var payload = await FetchPayload($"{url}?page={i}");
            var json = JsonSerializer.Deserialize<dynamic>(payload) ?? new JsonElement();

            ParseCharacters(json.GetProperty("results"), characters);
        }

        return characters;
    }

    static void ParseCharacters(JsonElement element, IList<Character> characters)
    {
        var enumerator = element.EnumerateArray();

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var id = current.GetProperty("id").GetInt32();
            var name = current.GetProperty("name").GetString() ?? "";
            var image = current.GetProperty("image").GetString() ?? "";
            var species = current.GetProperty("species").GetString() ?? "";
            var status = current.GetProperty("status").GetString() ?? "";

            if (status == "Alive")
            {
                var character = new Character(id)
                {
                    Name = name,
                    Image = image,
                    Species = species
                };
                
                characters.Add(character);
            }
        }
    }

    static void ListDatabaseCharacters()
    {
        using (var db = new TestDbContext())
        {
            var query = from c in db.Characters
                        orderby c.Id
                        select c;

            foreach (var item in query)
            {
                Console.WriteLine($" Character: {item.Name} ({item.Id})");
            }
        }
    }

    static void ClearDatabase()
    {
        using (var dbContext = new TestDbContext())
        {
            dbContext.Characters.RemoveRange(dbContext.Characters.ToList());
            dbContext.SaveChanges();

            Console.WriteLine("All records cleared successfully.");
        }
    }

    static async Task Main(string[] args)
    {
        try
        {
            ClearDatabase();
            ListDatabaseCharacters();

            await PopulateDatabase();
            
            ListDatabaseCharacters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
