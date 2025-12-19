using EduSystem.UI.Web.Client.Models;
using EduSystem.UI.Web.Client.Models.Common;

namespace EduSystem.UI.Web.Client.Services
{
    public interface ITeacherService
    {
        Task<List<TeacherModel>> GetAllTeachersAsync();
        Task<TeacherModel> GetTeacherByIdAsync(long id);
        Task CreateTeacherAsync(TeacherModel Teacher);
        Task UpdateTeacherAsync(TeacherModel Teacher);
        Task DeleteTeacherAsync(long id);
    }

    public class MockTeacherService : ITeacherService
    {
        private List<TeacherModel> _Teachers;

        public MockTeacherService()
        {
            _Teachers = new List<TeacherModel>();
            // Sey Mock Data
            for (int i = 1; i <= 20; i++)
            {
                _Teachers.Add(new TeacherModel
                {
                    Id = i,
                    Name = $"Teacher {i}",
                    Phone = $"017000000{i:00}",
                    Email = "teacher@gmail.com",
                    PresentAddress = new AddressModel { HouseNo = $"H-{i}", RoadNo = $"R-{i}", District = "Dhaka", Thana = "Mirpur" },
                    PermanentAddress = new AddressModel { HouseNo = $"H-{i}", RoadNo = $"R-{i}", District = "Borisal", Thana = "Sadar" }
                });
            }
        }

        public async Task<List<TeacherModel>> GetAllTeachersAsync()
        {
             await Task.Delay(100); // Simulate network
             return _Teachers;
        }

        public async Task<TeacherModel> GetTeacherByIdAsync(long id)
        {
             await Task.Delay(50);
             return _Teachers.FirstOrDefault(s => s.Id == id) ?? new TeacherModel();
        }

        public async Task CreateTeacherAsync(TeacherModel Teacher)
        {
             await Task.Delay(200);
             Teacher.Id = _Teachers.Count + 1;
             _Teachers.Add(Teacher);
        }

        public async Task UpdateTeacherAsync(TeacherModel Teacher)
        {
             await Task.Delay(200);
             var existing = _Teachers.FirstOrDefault(s => s.Id == Teacher.Id);
             if (existing != null)
             {
                 // Simple mapping
                 existing.Name = Teacher.Name;
                 existing.Phone = Teacher.Phone;
                 existing.Email = Teacher.Email;
                 existing.PermanentAddress = Teacher.PermanentAddress;
                 existing.PresentAddress = Teacher.PresentAddress;
                 // Add other fields as needed
             }
        }

        public async Task DeleteTeacherAsync(long id)
        {
             await Task.Delay(200);
             var existing = _Teachers.FirstOrDefault(s => s.Id == id);
             if (existing != null)
             {
                 _Teachers.Remove(existing);
             }
        }
    }
}
