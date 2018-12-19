using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  public class PasswordOptions
  {
    public int RequiredLength { get; set; }
    public int RequiredUniqueChars { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireDigit { get; set; }
  }

  // note: these are async because though the current PasswordService class is local (and sync), a remote class
  //       implemented via a Microservice or Azure Function/AWS Lamdba would be async..
  public interface IPasswordService
  {
    Task<string> CreatePasswordHash(string password);
    Task<string> GenerateRandomPassword(PasswordOptions opts = null);
    Task<bool> Verify(string passwordGuess, string actualSavedHashResults);
  }
}
