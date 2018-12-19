using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Test
{
  public enum ResponseCode
  {
    Ok = 0,
    NoData = 1,
    NotFound = 2,
    InvalidCredentials = 3,
    InvalidRequest = 4,
    Maintenance = 411,
    UnhandledError = 911,
    SystemDown = 999
  }

  public enum ResponseType
  {
    raw,
    error,
    user,
    contact,
    contacts
  }

  public class ResponseData : Dictionary<ResponseType, object> { }

  public class ResponseModel
  {
    [JsonProperty("codeText")]
    public ResponseCode Code { get; set; }

    [JsonProperty("code")]
    public int CodeNum => (int)this.Code;

    public string Message { get; set; }

    public ResponseData Data { get; set; }

    public string ToJson() => JsonConvert.SerializeObject(this);
  }
}
