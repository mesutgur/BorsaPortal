using System;
using System.Globalization;
using System.Web.Mvc;

namespace BorsaPortal.Web.Models
{
    /// <summary>
    /// Custom model binder for decimal values that accepts both comma and dot as decimal separator
    /// Supports Turkish culture (3,5) and invariant culture (3.5)
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var modelState = new ModelState { Value = valueResult };
            object actualValue = null;

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] Binding field: {bindingContext.ModelName}");

            if (valueResult == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: ValueResult is null, returning null");
                return null;
            }

            var valueAsString = valueResult.AttemptedValue;
            System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: AttemptedValue = '{valueAsString}'");

            if (string.IsNullOrWhiteSpace(valueAsString))
            {
                System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: Value is null or whitespace, returning null");
                return null;
            }

            try
            {
                // Remove any whitespace
                valueAsString = valueAsString.Trim();

                // Try to parse with invariant culture first (dot as decimal separator)
                if (decimal.TryParse(valueAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultInvariant))
                {
                    actualValue = resultInvariant;
                    System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: Successfully parsed with InvariantCulture = {resultInvariant}");
                }
                // Try to parse with Turkish culture (comma as decimal separator)
                else if (decimal.TryParse(valueAsString, NumberStyles.Any, new CultureInfo("tr-TR"), out decimal resultTurkish))
                {
                    actualValue = resultTurkish;
                    System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: Successfully parsed with tr-TR culture = {resultTurkish}");
                }
                // Try to parse with current culture as fallback
                else if (decimal.TryParse(valueAsString, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal resultCurrent))
                {
                    actualValue = resultCurrent;
                    System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: Successfully parsed with CurrentCulture = {resultCurrent}");
                }
                else
                {
                    // If all parsing attempts fail, add error
                    var errorMsg = string.Format("'{0}' geçerli bir sayı formatı değil. Lütfen geçerli bir ondalık sayı giriniz (örn: 3,5 veya 3.5)", valueAsString);
                    modelState.Errors.Add(errorMsg);
                    System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: FAILED to parse '{valueAsString}' - {errorMsg}");
                }
            }
            catch (FormatException ex)
            {
                var errorMsg = string.Format("'{0}' geçerli bir sayı formatı değil. Lütfen geçerli bir ondalık sayı giriniz (örn: 3,5 veya 3.5)", valueAsString);
                modelState.Errors.Add(errorMsg);
                System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: FormatException - {ex.Message}");
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            System.Diagnostics.Debug.WriteLine($"[DecimalModelBinder] {bindingContext.ModelName}: Returning {actualValue?.ToString() ?? "null"}");
            return actualValue;
        }
    }
}
