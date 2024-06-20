using System;
using System.Collections.Generic;
using System.Text;

namespace DemoApiWithoutEF.Utilities
{
    public class Enums
    {
        public enum ApiResponseCodes
        {
            Success=200,
            NotFound = 404,
            InternalServerError = 500
        }

        public enum ExecuteType
        {
            ExecuteScalar,
            ExecuteDataSet,
            ExecuteNonQuery,
            ExecuteReader
        }
    }
}
