using Microsoft.AspNetCore.Mvc;
using quiz_web_app.Infrastructure.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace quiz_web_app.Infrastructure.Middlewares
{
    public class GlobalExceptionHandler : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var problemDetails = new ProblemDetails();
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Возникли проблемы с корректностью данных";
                var errors = new List<object>();
                foreach(var error in ex.Errors)
                {
                    var errorResult = new {Detail = error.ErrorMessage};
                    errors.Add(errorResult);
                }
                problemDetails.Extensions.Add("Errors", errors);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch(BaseQuizAppException appException)
            {
                var problemDetails = new ProblemDetails()
                {
                    Title = "Возникли ошибки при выполнении операции",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = appException.Message
                };
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync<ProblemDetails>(problemDetails);
            }
            catch(Exception ex)
            {
                var problemDetails = new ProblemDetails();
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Возникла ошибка при выполнении операции";
                problemDetails.Detail = "Обратитесь в техподдержку.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
