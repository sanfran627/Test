using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
  public enum AddressType
  {
    Home = 0,
    Work
  }

  public class AddressModel
  {
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    // if international, this wouldn't be 'state'. for now we'll do 2-digit state code for brevity
    public string State { get; set; }
    // if international is supported, use the 2- or 3- character ISO code
    public string Country { get; set; }
    // this is dependent on the country code, but for now we're using zip5
    public string Postal { get; set; }
    // for US zip codes, last4 if we want to get cute...
    // public string Postal4 { get; set; }
  }
}
