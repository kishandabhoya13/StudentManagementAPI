using System.Data;

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
