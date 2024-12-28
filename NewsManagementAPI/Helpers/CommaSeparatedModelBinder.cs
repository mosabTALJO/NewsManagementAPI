using Microsoft.AspNetCore.Mvc.ModelBinding;
using NewsManagementAPI.Models;

namespace NewsManagementAPI.Helpers
{
    public class CommaSeparatedEnumModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var elements = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (elements.All(e => int.TryParse(e, out _)))
            {
                var intList = elements.Select(x => Enum.Parse(typeof(NewsMaterialStatus), x)).Cast<NewsMaterialStatus>().ToList();
                bindingContext.Result = ModelBindingResult.Success(intList);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "The value is not a valid list of integers.");
            }

            return Task.CompletedTask;
        }
    }
}