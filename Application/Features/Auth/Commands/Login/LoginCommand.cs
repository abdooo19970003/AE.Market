using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : ICommand<TokensResponseDto>;
    
}
