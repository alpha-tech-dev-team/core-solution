using Atlas.Domain.Models;
using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed record GetRoomsMessage : PagedMessage, IRequest<Result<RoomListPage>>;
