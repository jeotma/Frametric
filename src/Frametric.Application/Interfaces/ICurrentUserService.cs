namespace Frametric.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
