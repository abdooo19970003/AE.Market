using AE.Market.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace AE.Market.Application.Common.Interfaces
{
    public interface IBaseCommand;
    public interface ICommand : IBaseCommand, IRequest<Result>;
    public interface ICommand<TResponse> : IBaseCommand, IRequest<Result<TResponse>>;
}
