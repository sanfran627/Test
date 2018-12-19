using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Test
{
  #region ClientAPIController

  [Route("api")]
  public class ClientAPIController : Controller
  {
    MyDbContext _db;
    IPasswordService _pwd;
    IRandomNameService _names;
    ITokenService _tok;
    AuthenticatedUser _user = null;

    public ClientAPIController(MyDbContext db, IRandomNameService names, IPasswordService pwd, ITokenService tok)
    {
      _db = db;
      _names = names;
      _pwd = pwd;
      _tok = tok;
    }

    #region User Wrapper

    new AuthenticatedUser User
    {
      get
      {
        if (_user == null)
        {
          if (this.HttpContext.User == null) return null;
          if (this.HttpContext.User is AuthenticatedUser) return this.HttpContext.User as AuthenticatedUser;

          var claim = this.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
          if (claim == null) return null;

          try
          {
            Guid userId = Guid.Parse(claim.Value);
            var u = this._db.User.FirstOrDefault(c => c.UserId == userId);
            if (u == null) return null;
            _user = new AuthenticatedUser(u);
          }
          catch (Exception ex)
          {
            return null;
          }
        }
        return _user;
      }
    }

    #endregion

    [EnableCors("CorsPolicy")]
    [Route("signin")]
    [HttpPost]
    //[HttpPost("signin")]
    [AllowAnonymous]
    public async Task<ResponseModel> Signin([FromBody] RequestModel request)
    {
      // note: Error messages should be extracted from the UI, stored in config and loaded as needed from a central source into a static ConcurrentDictionary, etc.. Don't have time for that now...
      string
        e1 = "Please provide a valid username and password",
        e2 = "Invalid credentials. Please try again.";

      if (request == null
        || request.Action != ActionType.Signin
        || request.Signin == null
        || string.IsNullOrWhiteSpace(request.Signin.Username)
        || string.IsNullOrWhiteSpace(request.Signin.Password))
        return new ResponseModel { Code = ResponseCode.InvalidRequest, Message = e1 };

      //ignore casing for now
      var user = _db.User.FirstOrDefault(c => c.Username == request.Signin.Username.ToLower());

      // validate the password. If this was a live site, there would be a strike count then a lockout after n# of strikes... alert, email, etc.
      if (!(await _pwd.Verify(request.Signin.Password, user.Password)))
        return new ResponseModel { Code = ResponseCode.InvalidRequest, Message = e2 };

      // note: Error messages should be extracted from the UI, stored in config and loaded as needed from a central source. Don't have time for that now...
      if (user == null) return new ResponseModel { Code = ResponseCode.InvalidRequest, Message = e2 };

      var token = _tok.BuildToken(user.UserId);

      //lazy return (for now)
      return new ResponseModel
      {
        Code = ResponseCode.Ok,
        Data = new ResponseData
        {
          { ResponseType.user, new UserView(user, token) }
        }
      };
    }

    /*
     * there are a number of ways to approach this. I don't
     * like the idea of doing server-side pagination when we can use state (e.g. Redux/Vuex/etc.) that can move this information to the client;
     * however, in the absence of any clearn use cases beyond showing contacts, it makes sense to do pagination in this manner for now.
    */


    #region Contacts - CRUD

    [EnableCors("CorsPolicy")]
    [HttpGet("generate-contacts")]
    [Authorize]
    public async Task<ResponseModel> GenerateContacts(int qty)
    {
      try
      {
        await DataGenerator.GenerateContacts( this._db, this._names, this._pwd, this.User.User.UserId, qty);
        return new ResponseModel { Code = ResponseCode.Ok };
      }
      catch (Exception ex)
      {
        // log the error for immediate handling by devops..
        // have a user-friendly error message returned here..
        return new ResponseModel
        {
          Code = ResponseCode.UnhandledError,
          Message = $"Lazy Error Handling: {ex.Message}"
        };
      }
    }

    [EnableCors("CorsPolicy")]
    [HttpGet("contacts")]
    [Authorize]
    public async Task<ResponseModel> Contacts(int pos = 0, int qty = int.MaxValue)
    {
      // note: leaving this as async for now in case I can do the lookup async...

      var contacts = _db.Contact.Skip(pos).Take(qty);

      // normally I'd create a series of advanced constructors to make these calls easier rather than inlining them..
      return new ResponseModel
      {
        Code = contacts.Count() > 0 ? ResponseCode.Ok : ResponseCode.NoData,
        Data = new ResponseData
        {
          { ResponseType.contacts, contacts.Select(c => new ContactView(c) ) }
        }
      };
    }

    [EnableCors("CorsPolicy")]
    [Route("contacts/{contactId}")]
    [HttpDelete]
    [Authorize]
    public async Task<ResponseModel> DeleteContact( string contactId)
    {
      var existing = await _db.Contact.FindAsync( contactId.Decode(), this.User.User.UserId);
      if (existing == null) return new ResponseModel { Code = ResponseCode.Ok };

      // again, this is a terrible way to write code, but will have to suffice given time constraints
      try
      {
        _db.Contact.Remove(existing);
        await _db.SaveChangesAsync();
        return new ResponseModel { Code = ResponseCode.Ok };
      }
      catch (Exception ex)
      {
        // log the error for immediate handling by devops..
        // have a user-friendly error message returned here..
        return new ResponseModel
        {
          Code = ResponseCode.UnhandledError,
          Message = $"Lazy Error Handling: {ex.Message}"
        };
      }
    }

    [EnableCors("CorsPolicy")]
    [HttpPost("contacts")]
    [Authorize]
    public async Task<ResponseModel> CreateContact([FromBody] CreateContactAction action)
    {
      //validate the inbound contact. Can be done using libraries, by hand (here), etc.


      // de-duplication checks as needed

      // create
      ContactModel newContact = new ContactModel
      {
        ContactId = Guid.NewGuid(),
        UserId = this.User.User.UserId,
        Type = action.Type,
        FirstName = action.FirstName,
        LastName = action.LastName,
        Addresses = action.Addresses
      };

      // prefer to move this back to the data layer, wrap it with a clean exception wrapper, etc.
      try
      {
        _db.Contact.Add(newContact);
        await _db.SaveChangesAsync();
        return new ResponseModel
        {
          Code = ResponseCode.Ok,
          Data = new ResponseData
          {
            { ResponseType.contact, new ContactView(newContact) }
          }
        };
      }
      catch (Exception ex)
      {
        // log the error for immediate handling by devops..
        // have a user-friendly error message returned here..
        return new ResponseModel
        {
          Code = ResponseCode.UnhandledError,
          Message = $"Lazy Error Handling: {ex.Message}"
        };
      }
    }

    [EnableCors("CorsPolicy")]
    [HttpPut("contacts/{contactId}")]
    [Authorize]
    public async Task<ResponseModel> UpdateContact([FromBody] UpdateContactAction action, string contactId )
    {
      //validate the inbound contact. Can be done using libraries, by hand (here), etc.

      var existing = await _db.Contact.FindAsync(action.ContactId, this.User.User.UserId);
      if (existing == null) return new ResponseModel { Code = ResponseCode.NotFound };

      // walk through and compare the various properties between the 2 (shortcutting for now)
      if (existing.FirstName == action.FirstName && existing.LastName == action.LastName)
      {
        // if nothing's changed, return it back as-is and treat as successful..
        return new ResponseModel
        {
          Code = ResponseCode.Ok,
          Data = new ResponseData
          {
            { ResponseType.contact, new ContactView(existing) }
          }
        };
      }

      // continue on with changes

      existing.FirstName = action.FirstName;
      existing.LastName = action.LastName;
      existing.DOB = action.DOB;
      // addresses, etc.

      // again, this is a terrible way to write code, but will have to suffice given time constraints
      try
      {
        _db.Contact.Update(existing);
        await _db.SaveChangesAsync();
        return new ResponseModel
        {
          Code = ResponseCode.Ok,
          Data = new ResponseData
          {
            { ResponseType.contact, new ContactView(existing) }
          }
        };
      }
      catch (Exception ex)
      {
        // log the error for immediate handling by devops..
        // have a user-friendly error message returned here..
        return new ResponseModel
        {
          Code = ResponseCode.UnhandledError,
          Message = $"Lazy Error Handling: {ex.Message}"
        };
      }
    }

    #endregion
  }

  #endregion

  #region AuthenticatedUser

  public class AuthenticatedUser : ClaimsPrincipal
  {
    /// <summary>
    /// Yes, this is dumb, but hacking for the timeline
    /// </summary>
    public UserModel User { get; set; }

    public AuthenticatedUser(UserModel user) => this.User = user;
    public AuthenticatedUser(System.Security.Principal.IIdentity identity, UserModel user) : base(identity) { }
  }

  #endregion

}
