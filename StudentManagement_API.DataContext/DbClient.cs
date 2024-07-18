using DemoApiWithoutEF.DataContext;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DemoApiWithoutEF.Utilities.Enums;

namespace StudentManagement_API.DataContext
{
    public class DbClient
    {
        public static object ExecuteProcedureWithQuery(string query, Collection<DbParameters> parameters, ExecuteType executeType)
        {
            string connectionString = AppSettings.GetConnectionString();
            object returnValue;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.Text;

                if (parameters != null)
                {
                    AddParameters(ref sqlCommand, parameters);
                }
                if (executeType == ExecuteType.ExecuteNonQuery)
                {
                    returnValue = sqlCommand.ExecuteNonQuery();
                }
                else if (executeType == ExecuteType.ExecuteScalar)
                {
                    returnValue = sqlCommand.ExecuteScalar();
                }
                else if (executeType == ExecuteType.ExecuteReader)
                {
                    returnValue = sqlCommand.ExecuteReader();
                }
                else
                {
                    returnValue = "";
                }

            }
            return returnValue;
        }

        public static object ExecuteProcedure(string ProcedureName, Collection<DbParameters> parameters, ExecuteType executeType)
        {
            string connectionString = AppSettings.GetConnectionString();
            object returnValue;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(ProcedureName, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

                if (parameters != null)
                {
                    AddParameters(ref sqlCommand, parameters);
                }
                if (executeType == ExecuteType.ExecuteNonQuery)
                {
                    returnValue = sqlCommand.ExecuteNonQuery();
                }
                else if (executeType == ExecuteType.ExecuteScalar)
                {
                    returnValue = sqlCommand.ExecuteScalar();
                }
                else if (executeType == ExecuteType.ExecuteReader)
                {
                    returnValue = sqlCommand.ExecuteReader();
                }
                else
                {
                    returnValue = "";
                }

            }
            return returnValue;
        }

        public static IList<T> ExecuteProcedure<T>(string ProcedureName, Collection<DbParameters> parameters)
        {
            string connectionString = AppSettings.GetConnectionString();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(ProcedureName, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    AddParameters(ref sqlCommand, parameters);
                }
                //var result = ;
                return DataReaderToList<T>(sqlCommand.ExecuteReader());
            }
        }

        public static T ExecuteOneRecordProcedure<T>(string ProcedureName, Collection<DbParameters> parameters)
        {
            string connectionString = AppSettings.GetConnectionString();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(ProcedureName, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    AddParameters(ref sqlCommand, parameters);
                }
                //var result = ;
                return GetValueFromReader<T>(sqlCommand.ExecuteReader());
            }
        }

        public static T ExecuteOneRecordProcedureWithQuery<T>(string Query, Collection<DbParameters> parameters)
        {
            string connectionString = AppSettings.GetConnectionString();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(Query, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    AddParameters(ref sqlCommand, parameters);
                }
                //var result = ;
                return GetValueFromReader<T>(sqlCommand.ExecuteReader());
            }
        }

        public static void AddParameters(ref SqlCommand sqlCommand, Collection<DbParameters> parameters)
        {
            foreach (DbParameters param in parameters)
            {
                if(param.Direction == ParameterDirection.Output)
                {
                    SqlParameter sqlParameter = new()
                    {
                        DbType = param.DBType,
                        Direction = ParameterDirection.Output,
                        ParameterName = param.Name,
                    };
                    if (param.TypeName != null)
                    {
                        sqlParameter.TypeName = param.TypeName;
                        sqlParameter.SqlDbType = SqlDbType.Structured;
                    }
                    sqlCommand.Parameters.Add(sqlParameter);
                }
                else
                {
                    SqlParameter sqlParameter = new()
                    {
                        DbType = param.DBType,
                        Direction = ParameterDirection.Input,
                        ParameterName = param.Name,
                        Value = param.Value
                    };
                    if (param.TypeName != null)
                    {
                        sqlParameter.TypeName = param.TypeName;
                        sqlParameter.SqlDbType = SqlDbType.Structured;
                    }
                    sqlCommand.Parameters.Add(sqlParameter);
                }
               
            }
        }

        public static List<T> DataReaderToList<T>(IDataReader dataReader)
        {
            List<T> list = new();

            T obj = default(T);

            while (dataReader.Read())
            {
                obj = Activator.CreateInstance<T>();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    PropertyInfo propertyInfo = obj.GetType().GetProperties().FirstOrDefault(o => o.Name.ToLower() == dataReader.GetName(i).ToLower());
                    if (propertyInfo != null)
                    {
                        if (dataReader.GetFieldType(i) == typeof(Int64))
                        {
                            propertyInfo.SetValue(obj, dataReader.GetValue(i) != DBNull.Value ? Convert.ToInt32(dataReader.GetValue(i)) : null, null);
                        }
                        else
                        {
                            propertyInfo.SetValue(obj, dataReader.GetValue(i) != DBNull.Value ? dataReader.GetValue(i) : null, null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        private static T GetValueFromReader<T>(SqlDataReader dataReader)
        {
            
            T obj = Activator.CreateInstance<T>();
            if (!dataReader.Read()) // Check if there are any records to read
                return obj;

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                PropertyInfo propertyInfo = obj.GetType().GetProperties().FirstOrDefault(o => o.Name.ToLower() == dataReader.GetName(i).ToLower());
                if (propertyInfo != null)
                {
                    if (dataReader.GetFieldType(i) == typeof(Int64))
                    {
                        propertyInfo.SetValue(obj, dataReader.GetValue(i) != DBNull.Value ? Convert.ToInt32(dataReader.GetValue(i)) : null, null);
                    }
                    else
                    {
                        propertyInfo.SetValue(obj, dataReader.GetValue(i) != DBNull.Value ? dataReader.GetValue(i) : null, null);
                    }
                }
            }

            return obj;
        }
    }
}
