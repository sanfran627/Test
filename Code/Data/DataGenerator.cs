using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  public static class DataGenerator
  {

    #region Typically this logic wouldn't live here. It'd live a level above to keep data access "dumb"

    public static async Task GenerateUsers(MyDbContext db, IRandomNameService namesSvc, IPasswordService pwdSvc, int qty)
    {
      // this is an incredibly slow process using entity framework.
      // ideally this would use a Table Value Parameter in batches of 1,000 writes,
      // not something this slow...
      var names = namesSvc.GetRandomNames(qty);

      foreach (var name in names)
      {
        var g = Guid.NewGuid();

        db.User.Add(new UserModel
        {
          UserId = g,
          //generate a fully unique, but still reasonably-recognizable username
          Username = PrepUsername($"{name.first.ToLower()}.{name.last.ToLower()}@{g.ToString().Replace("-", "").ToLower()}.co"),
          FirstName = name.first,
          LastName = name.last,
          Password = await pwdSvc.GenerateRandomPassword(),
          Title = string.Empty,
          Type = UserType.Standard,
        });
      }
      await db.SaveChangesAsync();
    }

    public static async Task GenerateContacts(MyDbContext db, IRandomNameService namesSvc, IPasswordService pwdSvc, Guid userId, int qty)
    {
      // this is an incredibly slow process using entity framework.
      // ideally this would use a Table Value Parameter in batches of 1,000 writes,
      // not something this slow...
      var names = namesSvc.GetRandomNames(qty);
      List<ContactModel> contacts = new List<ContactModel>();
      foreach (var name in names)
      {
        var g = Guid.NewGuid();
        contacts.Add(new ContactModel
        {
          ContactId = g,
          UserId = userId,
          FirstName = name.first,
          LastName = name.last,
          DOB = new DateTime(1990, 01, 01),
          Type = ContactType.Standard
        });

      }
      db.Contact.AddRange(contacts);
      await db.SaveChangesAsync();
    }

    private static string PrepUsername(string username) => username != null ? username.ToLower() : string.Empty;

    #endregion
  }
}
