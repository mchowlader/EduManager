using EduSystem.UI.Web.Client.Models.Academy;

namespace EduSystem.UI.Web.Client.Services.Academy;

public interface IAcademyService
{
    Task<TranscriptModel?> GetTranscriptAsync(string studentId);
    Task<TestimonialModel?> GetTestimonialAsync(string studentId);
}
