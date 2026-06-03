using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using FluentValidation;

namespace AE.Market.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private readonly IRepository<User> _repo;

        public RegisterCommandValidator(IRepository<User> repo)
        {
            RuleFor(c => c.Email)
                .EmailAddress()
                .NotEmpty()
                .MustAsync(IsEmailUnique)
                .WithMessage("This email is already registered.");
            RuleFor(c => c.Password)
                .NotEmpty()
                .MinimumLength(4)
                .Matches("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,}$");
            _repo = repo;
        }

        private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
        {
            var spec = new UserByEmailSpec(email);
            return !await _repo.AnyAsync(spec, cancellationToken);
        }
    }
}
