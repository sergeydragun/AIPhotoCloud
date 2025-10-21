using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PhotoCloud.Validators.Filters;

public class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments)
        {
            var value = arg.Value;
            if(value == null)
                continue;
            
            var argType = value.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argType);
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;
            
            if(validator == null)
                continue;
            
            var validatorContext = new ValidationContext<object>(value);
            var result = await validator.ValidateAsync(validatorContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    foreach (var err in result.Errors)
                    {
                        context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                }
            }

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }
            
            await next();
        }
    }
}