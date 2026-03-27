using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_DAL
{
    public class BusinessException : Exception
    {
        public BusinessException(string userMessage ,Exception innerException) 
            : base(userMessage ,innerException) { }
    }
}
