namespace Semogly.Maestro.Abstractions.Activities;

public static class ActivityExecutionContextExtensions
{
    /// <exception cref="InvalidOperationException">The engine only fail-fasts on params marked Required on the Activity; a handler needing a param the definition doesn't mark Required must still guard for it, which this throws for.</exception>
    public static string GetRequired(this ActivityExecutionContext context, string key) =>
        context.Parameters.TryGetValue(key, out var value) && value is not null
            ? value
            : throw new InvalidOperationException($"Required parameter '{key}' was not resolved before execution.");

    public static string? GetOptional(this ActivityExecutionContext context, string key, string? defaultValue = null) =>
        context.Parameters.TryGetValue(key, out var value) ? value ?? defaultValue : defaultValue;

    public static bool GetBoolean(this ActivityExecutionContext context, string key, bool defaultValue = false) =>
        bool.TryParse(context.GetOptional(key), out var parsed) ? parsed : defaultValue;
}
