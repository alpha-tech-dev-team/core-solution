using Identity.Application.Constants;

namespace Identity.Application.Contracts.Admin;

public record GetUsersRequest(
    int Page = ValidationConstants.PageMin,
    int PageSize = ValidationConstants.PageSizeDefault);
