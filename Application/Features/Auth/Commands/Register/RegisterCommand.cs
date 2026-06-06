
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Commands.Register
{
    public sealed record RegisterCommand(string Email, string Password) : ICommand<TokensResponseDto>;
    
}
