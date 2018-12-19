using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  public class JWTSecretKeys : Dictionary<string, string> { }

  public interface IAppSettings
  {
    JWTSecretKeys JWTSecretKeys { get; set; }
  }

  public class AppSettings : IAppSettings
  {
    public JWTSecretKeys JWTSecretKeys { get; set; }
  }
}
