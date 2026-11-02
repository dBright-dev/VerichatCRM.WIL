using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VerichatCRM.Web.ModelBinding;

/// <summary>
/// Binds decimals and doubles with the invariant culture.
/// </summary>
/// <remarks>
/// An HTML number input always submits a dot as the decimal separator, whatever
/// the user's locale, but MVC binds simple numeric types with the current
/// culture. On an en-ZA host - where the dot is a group separator - "2400.00"
/// therefore fails to parse and the value arrives as zero, with no error unless
/// the action inspects ModelState. Nothing in this application pins a culture,
/// so the behaviour used to depend on the machine it was deployed to.
/// JSON bodies are unaffected: they are read by the JSON serialiser, which is
/// already invariant.
/// </remarks>
public sealed class InvariantDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return modelType == typeof(decimal) || modelType == typeof(double)
            ? new InvariantNumberModelBinder(modelType, isNullable: Nullable.GetUnderlyingType(context.Metadata.ModelType) != null)
            : null;
    }

    private sealed class InvariantNumberModelBinder(Type modelType, bool isNullable) : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var provided = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (provided == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, provided);

            var raw = provided.FirstValue;
            if (string.IsNullOrWhiteSpace(raw))
            {
                if (isNullable)
                    bindingContext.Result = ModelBindingResult.Success(null);
                else
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                        $"{bindingContext.ModelName} is required.");
                return Task.CompletedTask;
            }

            // Thousands separators are accepted, but only with a dot as the decimal
            // separator, so "1.234,5" from a European keyboard is rejected rather
            // than silently read as 1.2345.
            object? parsed = modelType == typeof(decimal)
                ? decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null
                : double.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var dbl) && !double.IsNaN(dbl) ? dbl : null;

            if (parsed == null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                    $"{raw} is not a number.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }
    }
}
