using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.DataContext
{
    public class DbParameters
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public DbType DBType { get; set; }

        public ParameterDirection Direction { get; set; }

        public string TypeName { get; set; }
    }
}
