namespace EduSystem.Identity.Application.IService;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
