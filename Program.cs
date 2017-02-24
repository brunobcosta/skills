using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ConsoleApplication
{
    public class Program
    {

        public static void Main(string[] args)
        {

            var w = new Worker(new MongoClient().GetDatabase("geojobs"));
            Task.Run(async () =>
            {
                await w.Run();
            }).Wait();
        }
    }
    public class Worker
    {
        protected IMongoDatabase _database;

        public Dictionary<String, int> Words { get; set; }

        public Worker(IMongoDatabase database)
        {
            _database = database;
            Words = new Dictionary<string, int>();
        }

        public async Task Run()
        {
            var jobs = _database.GetCollection<dynamic>("jobs").AsQueryable();
            var bla = await jobs.ToListAsync();
            foreach (var description in bla.Select(x => x.Description as string))
            {

var split = Regex.Split(description.RemoveAccents(), @"[\p{Po}\p{Pc}\p{Pd}\p{Ps}\p{Pe}\p{Pi}\p{Pf}\p{Zl}\p{Zp}\p{Zs}\p{Cc}]");

                foreach (var w in split.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(w)||w=="\u0095") continue;
                    var key = w.Trim().ToLower();
                    int val;
                    if (Words.TryGetValue(key, out val))
                    {
                        Words[key]++;
                    }
                    else
                    {
                        Words.Add(key, 1);
                    }
                }
            }

            foreach (var item in Words.OrderByDescending(x => x.Value))
            {
                Console.WriteLine("|{0}| - {1}", item.Key, item.Value);
            }
        }
    }
    public static class StringExtentions
    {
        public static string RemoveAccents(this string text)
        {
            var sbReturn = new StringBuilder();
            foreach (char letter in text.Normalize(NormalizationForm.FormD).ToCharArray())
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

    }
}
