using System.Data;
using Dapper;

namespace DataAccessor.TypeHandlers;

public class StronglyTypedIdHandler<TId, TValue>(
    Func<TValue, TId> create,
    Func<TId, TValue> getValue) : SqlMapper.TypeHandler<TId> where TId : struct {
    public override TId Parse(object value) =>
        create((TValue)value);

    public override void SetValue(IDbDataParameter parameter, TId value) {
        parameter.Value = getValue(value);
    }
}