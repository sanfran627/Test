using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  public enum ContactType
  {
    Standard = 0,
    GoldClub = 1,
  }

  /*
   * notes:
   *
   * Without more requirements, there is need to separate out the User and the Contact explicitly.
   * Doing so requires being able to de-dup Contacts, which isn't necessary for this example.
   * If there was a need to separate Users from Contacts, I'd move the UserId out and have a separate
   * Index/Table/Data Model that mapped the 2, along with de-dup logic or a more distinct key
   */

  public class ContactModel
  {
    /// <summary>The unique Id of this contact</summary>
    public Guid ContactId { get; set; }
    /// <summary>The unique Id of the user to which this contact belongs</summary>
    public Guid UserId { get; set; }
    public ContactType Type { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }

    public Dictionary<AddressType, AddressModel> Addresses { get; set; }

    internal string AddressesString
    {
      get => this.Addresses != null
        ? string.Empty
        : Newtonsoft.Json.JsonConvert.SerializeObject(this.Addresses);

      set => this.Addresses = string.IsNullOrWhiteSpace(value)
        ? new Dictionary<AddressType, AddressModel>()
        : Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<AddressType, AddressModel>>(value);
    }
  }

  public class ContactView
  {
    public ContactView(ContactModel model)
    {
      this.ContactId = model.ContactId;
      this.Type = model.Type;
      this.FirstName = model.FirstName;
      this.LastName = model.LastName;
      this.DOB = model.DOB;
    }

    [JsonConverter(typeof(JsonIdConverter))]
    public Guid ContactId { get; set; }
    public ContactType Type { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
    /// <summary>total hack, but a base approximation to age for the purpose of being interesting</summary>
    public int Age => this.DOB != null ? Convert.ToInt32(Math.Floor(DateTime.Now.Date.Subtract(this.DOB).TotalDays / 365.25)) : 0;
  }


  public class CreateContactAction
  {
    public ContactType Type { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
    public Dictionary<AddressType, AddressModel> Addresses { get; set; }
  }

  public class UpdateContactAction
  {
    [JsonConverter(typeof(JsonIdConverter))]
    public Guid ContactId { get; set; }
    public ContactType Type { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
  }

  public class DeleteContactAction
  {
    [JsonConverter(typeof(JsonIdConverter))]
    public Guid ContactId { get; set; }
    // if we care to log why it's being deleting
    public string Reason { get; set; }
  }
}
