namespace Atlas.Core.Interfaces;

public interface IRequestContext
{
    Entities.UserSession? Session { get; set; }
}
