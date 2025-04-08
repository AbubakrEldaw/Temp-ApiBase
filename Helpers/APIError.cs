using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace APIBase.Helpers
{
    public class ApiError
    {
        public string StatusCode { get; private set; }
        public string StatusDescription { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; private set; }

        public ApiError(string statusCode, string statusDescription)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
        }

        public ApiError(string statusCode, string statusDescription, string message)
            : this(statusCode, statusDescription)
        {
            this.Message = message;
        }
    }

    public class CustomError
    {
        public CustomError(string id,string ErrorMessage) {
            this.id = id;
            this.ErrorMessage = ErrorMessage;
        }
        public string id { get; set; }
        public string ErrorMessage { get; set; }
    }
    public static class ErrorHelper
    {
        //Admin Errors
        public static ApiError InvalidRefreshToken { get { return new ApiError("token-0001", "InvalidRefreshToken"); } }

        //Client Errors
        public static ApiError NoOpenWorkDay { get { return new ApiError("wd-0001", "There is no open work day"); } }
        public static ApiError WorkdayHasActiveOrders { get { return new ApiError("wd-0002", "Cant close work day while having active orders"); } }
        public static ApiError NoOpenEmployeeShift { get { return new ApiError("wds-0001", "There is no open employee shift"); } }
        public static ApiError EmployeeHasAlreadyOpenShift { get { return new ApiError("wds-0002", "The employee has already open shift"); } }
        public static ApiError DatabaseError { get { return new ApiError("db-0001", "DatabaseError"); } }
        public static ApiError UnknownError { get { return new ApiError("Uk0001", "Unknown Error!"); } }
        

    }
}
