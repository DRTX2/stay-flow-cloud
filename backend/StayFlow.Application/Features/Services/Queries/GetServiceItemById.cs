using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Services;

namespace StayFlow.Application.Features.Services.Queries;

public sealed record GetServiceItemByIdQuery(Guid Id) : IRequest<ServiceItemDto>;

public sealed class GetServiceItemByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetServiceItemByIdQuery, ServiceItemDto>
{
    public async Task<ServiceItemDto> Handle(GetServiceItemByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await dbContext.ServiceItems
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new ServiceItemDto(s.Id, s.Name, s.Description, s.Price, s.Category, s.IsActive))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceItem), request.Id);

        return service;
    }
}
