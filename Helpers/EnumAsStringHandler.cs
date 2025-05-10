using Dapper;
using System.Data;

public class EnumAsStringHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override T Parse(object value)
    {
        return Enum.Parse<T>(value.ToString()!);
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.ToString();
        parameter.DbType = DbType.String;
    }
}
