using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test
{
  public enum NameType
  {
    First,
    Last
  }

  public interface IRandomNameService
  {
    void Load();

    /// <summary>Returns a random name if successfully loaded</summary>
    /// <param name="type"></param>
    /// <returns>a random name or null if unsuccessful in loading</returns>
    string GetRandomName(NameType type, int pos = -1);

    /// <summary>Returns a List of random first/last if successfully loaded</summary>
    /// <param name="type"></param>
    /// <returns>a random name set or null if unsuccessful in loading</returns>
    List<(string first, string last )> GetRandomNames(int qty);
  }
}
