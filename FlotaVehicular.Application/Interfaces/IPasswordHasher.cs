namespace FlotaVehicular.Application.Interfaces;

// Genera y verifica hashes de contraseña sin almacenar la contraseña original.
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}
