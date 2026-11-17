using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;

namespace Atlas.Application.Common;

public abstract class SessionStateHandler<TRequest>
{
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;

    protected AtlasUserSession Session
    {
        get { return (AtlasUserSession)_requestContext.Session; }
    }
    protected abstract string NextWorkflowState { get; }
    protected SessionStateHandler(IRequestContext requestContext, ISessionService sessionService)
    {
        _requestContext = requestContext;
        _sessionService = sessionService;
    }

    protected Task UpdateState(TRequest request, CancellationToken cancellationToken)
    {
        var session = Session;

        ApplyState(session, request);

        return _sessionService.UpdateState(session, NextWorkflowState, cancellationToken);
    }

    protected virtual void ApplyState(AtlasUserSession session, TRequest request)
    {
    }
}
