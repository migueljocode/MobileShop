using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MobileShop.Models.Extensions;

namespace MobileShop.Web.Infrastructure;

/// <summary>
/// Binds <see cref="DateTime"/> and <see cref="DateTime?"/> values that arrive from the Shamsi
/// (Jalali) calendar inputs, for example <c>1405/07/18</c> or <c>1405/07/18 14:30</c>.
/// Text that cannot be a Shamsi date is handed to the framework binder, so Gregorian ISO
/// values (bookmarked links, seeded data) keep binding exactly as they did before.
/// </summary>
public sealed class JalaliDateTimeModelBinder : IModelBinder
{
    private const string InvalidShamsiDateMessage =
        "Enter the date in the Shamsi calendar, for example 1405/07/18.";

    private readonly SimpleTypeModelBinder _fallback;

    public JalaliDateTimeModelBinder(Type modelType, ILoggerFactory loggerFactory)
        => _fallback = new SimpleTypeModelBinder(modelType, loggerFactory);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        var text = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(text))
            return _fallback.BindModelAsync(bindingContext);

        if (JalaliDateExtensions.TryParseJalali(text, out var parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }

        // A leading year that can only be Shamsi means the value was meant for the Shamsi
        // calendar, so report a clear message instead of a confusing Gregorian range error.
        if (JalaliDateExtensions.LooksLikeJalaliDate(text))
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, InvalidShamsiDateMessage);
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        return _fallback.BindModelAsync(bindingContext);
    }
}

/// <summary>Applies <see cref="JalaliDateTimeModelBinder"/> to every <see cref="DateTime"/> value.</summary>
public sealed class JalaliDateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;
        if (modelType != typeof(DateTime) && modelType != typeof(DateTime?))
            return null;

        var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
        return new JalaliDateTimeModelBinder(modelType, loggerFactory);
    }
}
