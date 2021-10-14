using System;

namespace Datack.Common.Models.RPC
{
    public class RpcException
    {
        public String Message { get; set; }
        public String StackTrace { get; set; }

        public RpcException InnerException { get; set; }

        public override String ToString()
        {
            if (InnerException != null)
            {
                return $"{InnerException}{Environment.NewLine}{Message}{Environment.NewLine}{StackTrace}";
            }

            return $"{Message}{Environment.NewLine}{StackTrace}";
        }
    }

    public static class RpcExceptionExtension
    {
        public static RpcException ToRpcException(this Exception ex)
        {
            var result = new RpcException
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.ToRpcException()
            };
            return result;
        }
    }
}
