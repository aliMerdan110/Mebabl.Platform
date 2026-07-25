namespace Mebabl.Platform.Application.Services.Password;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}