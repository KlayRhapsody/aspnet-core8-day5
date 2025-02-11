namespace WebApi.Services;

public interface IUserService
{
    Task<string> UserLogin(string email, string password);
}

public sealed class UserService(TokenProvider tokenProvider) : IUserService
{
    public async Task<string> UserLogin(string email, string password)
    {
        // TODO 改為從資料庫取得使用者資料
        await Task.Delay(1000);

        User user = new(
            id: "1", 
            email: email, 
            emailVerified: true);

        return tokenProvider.Create(user);
    }
}
