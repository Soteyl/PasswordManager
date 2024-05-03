using FluentValidation;

namespace PasswordStorageService.Common;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> IsGuid<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder.Must(x => Guid.TryParse(x, out _))
            .WithMessage($"The property must be of a Guid type.");
    }
    
    public static IRuleBuilderOptions<T, string> IsUri<T>(this IRuleBuilder<T, string> ruleBuilder) {
        return ruleBuilder.Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage($"The property must be a valid Url.");
    }
}