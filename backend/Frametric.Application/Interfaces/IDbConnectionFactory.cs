using System.Data;

namespace Frametric.Application.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
