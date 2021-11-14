using System;
using System.Collections.Generic;
using System.Text;

namespace WkyApiSharp.Service
{
    public class WkyApiException : Exception
    {
        public WkyApiException(string message) : base(message)
        {
        }
    }
}
