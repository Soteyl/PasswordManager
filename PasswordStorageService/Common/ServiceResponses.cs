using FluentValidation.Results;
using PasswordManager;

namespace PasswordStorageService.Common;

public static class ServiceResponses
{
    public static ServiceResponse Success => new ServiceResponse() { IsSuccess = true };

    public static ServiceResponse NotFound(string propertyName) 
        => new ServiceResponse
            {
                IsSuccess = false,
                Error = new Error()
                {
                    Property = propertyName, 
                    Message = "The entity is not found.", 
                    StatusCode = ErrorStatusCode.NotFound
                }
            };
    
    public static ServiceResponse ValidationFailure(ValidationResult result) 
        => new ServiceResponse
            {
                IsSuccess = false,
                Error = new Error()
                {
                    Property = result.Errors[0].PropertyName,
                    Message =  result.Errors[0].ErrorMessage
                }
            };
}