namespace ToDoApp.Shared.Common.Filtering;

public class QueryFilter
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public QueryFilterOperator Operator { get; set; } = QueryFilterOperator.Equals;
}

public enum QueryFilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}
