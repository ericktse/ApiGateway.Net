using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiGateway.Net.Utility.Filter
{
    public class FilterException : Exception
    {
        public FilterException()
        {

        }

        public FilterException(string message) : base(message)
        {

        }

        public FilterException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}