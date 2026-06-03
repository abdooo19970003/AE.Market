
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
    public record RefreshCommand(string OldToken) : ICommand<TokensResponseDto>;
    
}
