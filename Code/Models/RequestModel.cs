using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  /*
   * action classes technically live anywhere. A separate folder structure, alongside their data and outbound(view) model, etc.
   */

  public enum ActionType
  {
    Signin,
  }

  public class RequestModel
  {
    public ActionType Action { get; set; }
    public SigninAction Signin { get; set; }
  }

  // normally I'd make this more robust, but the interest of time restraints

  public class SigninAction
  {
    public string Username { get; set; }
    public string Password { get; set; }
  }
}
