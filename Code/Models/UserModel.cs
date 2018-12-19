using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Test
{
  public enum UserType
  {
    Standard = 0,
    Admin = 1
  }

  public class UserModel
  {
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public UserType Type { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Title { get; set; }
  }




/// <summary>
/// Version of the UserModel that is consumable by external resources (e.g. no Password)
/// </summary>
public class UserView
  {
    public UserView(UserModel model, string token)
    {
      this.UserId = model.UserId;
      this.Username = model.Username;
      this.Type = model.Type;
      this.FirstName = model.FirstName;
      this.LastName = model.LastName;
      this.Title = model.Title;
      this.Token = token;
    }

    [JsonConverter(typeof(JsonIdConverter))]
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public UserType Type { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Title { get; set; }
    public string Token { get; set; }
  }
}
