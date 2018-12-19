using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test
{
  public class RandomNameService : IRandomNameService
  {
    #region variables

    static bool _loaded = false;
    static string _loadError = null;
    static object _lock = new object();
    static string[] _first = null;
    static string[] _last = null;
    const string PATH = "Test.data.names.json";

    #endregion

    #region Load

    public void Load()
    {
      if (!_loaded)
      {
        lock (_lock)
        {
          if (!_loaded)
          {
            try
            {
              // using HashSets to ensure names are unique during load. Will use arrays for fast lookup once finished
              HashSet<string> firstNames = new HashSet<string>(), lastNames = new HashSet<string>();

              // read names from the embedded resource. In an ideal world we'd record the error or at least set it as a result..
              using (var reader = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(PATH)))
              {
                string json = reader.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json))
                {
                  var names = JArray.Parse(json);
                  foreach (var item in names)
                  {
                    string f = item["first"].Value<string>();
                    string l = item["last"].Value<string>();

                    // de-duplication
                    if (!firstNames.Contains(f)) firstNames.Add(f);
                    if (!lastNames.Contains(l)) lastNames.Add(l);
                  }
                }
              }

              // no errors, we're good
              _first = firstNames.ToArray();
              _last = lastNames.ToArray();
              _loaded = true;
            }
            catch (Exception ex)
            {
              // save the error, including everything we can capture from it - no logging for now
              _loadError = ex.Message + (!string.IsNullOrWhiteSpace(ex.StackTrace) ? Environment.NewLine + ex.StackTrace : string.Empty);

              // we're only here if there was an error reading the resource OR the format was changed without forewarning..
              // clear whatever work we did complete and send up red flags.
            }
          }
        }
      }
    }

    #endregion

    private int Count(NameType type) => type == NameType.First ? _first.Length : _last.Length;

    public string GetRandomName(NameType type, int pos = -1)
    {
      // don't throw an exception - expensive - always return null
      if (!_loaded) return null;

      var d = type == NameType.First ? _first : _last;

      // grab a random number within range
      if (pos < 0 || pos > d.Length - 1)
        pos = new Random((int)DateTime.UtcNow.Ticks).Next(Count(type));

      return d[pos];
    }

    public List<(string first, string last)> GetRandomNames(int qty)
    {
      // don't throw an exception - expensive - always return null
      if (!_loaded) return null;

      List<(string first, string last)> names = new List<(string first, string last)>();

      Random r = new Random((int)DateTime.UtcNow.Ticks);
      int maxF = _first.Length,
          maxL = _last.Length;

      // build a stupid simple list of unique names
      for (var i = 0; i < qty; i++)
        names.Add((_first[r.Next(maxF)], _last[r.Next(maxL)]));

      return names;
    }
  }
}
