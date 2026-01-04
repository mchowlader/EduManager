using EduSystem.UI.Web.Client.Models;
using EduSystem.UI.Web.Client.Models.Common;
using EduSystem.UI.Web.Client.Services.Base;

namespace EduSystem.UI.Web.Client.Services;

public interface IClassesService
{
    Task<ApiResponse<PagedList<ClassesDto>>> GetAllClassesAsync();
    Task<ApiResponse<ClassesDto>> GetClassByIdAsync(long id);
    Task<ApiResponse<ClassesDto>> CreateClassAsync(ClassesDto model);
    Task<ApiResponse<ClassesDto>> UpdateClassAsync(ClassesDto model);
    Task<ApiResponse<bool>> DeleteClassAsync(long id);
}

public class ClassesService : IClassesService
{
    private readonly IApiService _apiService;
    private const string Endpoint = "api/applicationuser/v1/classes"; // Gateway Endpoint
    public ClassesService(IApiService apiService)
    {
        _apiService = apiService;
    }
    public async Task<ApiResponse<PagedList<ClassesDto>>> GetAllClassesAsync() =>
        await _apiService.GetAsync<PagedList<ClassesDto>>(Endpoint);
    public async Task<ApiResponse<ClassesDto>> GetClassByIdAsync(long id) =>
        await _apiService.GetAsync<ClassesDto>($"{Endpoint}/{id}");
    public async Task<ApiResponse<ClassesDto>> CreateClassAsync(ClassesDto model) =>
        await _apiService.PostAsync<ClassesDto, ClassesDto>(Endpoint, model);
    public async Task<ApiResponse<ClassesDto>> UpdateClassAsync(ClassesDto model) =>
        await _apiService.PutAsync<ClassesDto, ClassesDto>($"{Endpoint}/{model.Id}", model);
    public async Task<ApiResponse<bool>> DeleteClassAsync(long id) =>
        await _apiService.DeleteAsync($"{Endpoint}/{id}");
}
