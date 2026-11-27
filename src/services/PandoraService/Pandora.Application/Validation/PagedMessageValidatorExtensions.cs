using CoreSolution.Shared.Application.Validation;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using FluentValidation;
using Pandora.Domain.Enums;

namespace Pandora.Application.Validation;

public static class PagedMessageValidatorExtensions
{
    public static void AddPagingRules<TMessage>(this AbstractValidator<TMessage> validator)
        where TMessage : PagedMessage
    {
        validator.RuleFor(message => message.Page)
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .WithErrorCode(nameof(ErrorCode.InvalidPage))
            .When(message => message.Page.HasValue);

        validator.RuleFor(message => message.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .WithErrorCode(nameof(ErrorCode.InvalidPageSize))
            .When(message => message.PageSize.HasValue);
    }

    public static void AddPagedSortRules<TMessage>(
        this AbstractValidator<TMessage> validator,
        IReadOnlyCollection<string> sortFields)
        where TMessage : PagedMessage, ISortMessage
    {
        validator.AddPagingRules();
        validator.ApplySortRules(
            sortFields,
            nameof(ErrorCode.InvalidSortField),
            nameof(ErrorCode.InvalidSortDirection));
    }

    public static void AddPagedSearchSortRules<TMessage>(
        this AbstractValidator<TMessage> validator,
        IReadOnlyCollection<string> sortFields)
        where TMessage : PagedSearchSortMessage
    {
        validator.AddPagedSortRules(sortFields);
    }
}
