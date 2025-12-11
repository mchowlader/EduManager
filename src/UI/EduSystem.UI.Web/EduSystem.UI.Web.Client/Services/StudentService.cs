using EduSystem.UI.Web.Client.Models;

namespace EduSystem.UI.Web.Client.Services
{
    public interface IStudentService
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(long id);
        Task CreateStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(long id);
    }

    public class MockStudentService : IStudentService
    {
        private List<Student> _students;

        public MockStudentService()
        {
            _students = new List<Student>();
            // Sey Mock Data
            for (int i = 1; i <= 20; i++)
            {
                _students.Add(new Student
                {
                    Id = i,
                    Name = $"Student {i}",
                    Phone = $"017000000{i:00}",
                    Class = (ClassCategory)(i % 12),
                    Department = (DepartmentCategory)(i % 4),
                    DateOfBirth = DateTime.Today.AddYears(-15).AddDays(i),
                    DateOfBirthNo = $"BID-{2000+i}-000{i}",
                    DetailsPresentAddress = new Address { HouseNo = $"H-{i}", RoadNo = $"R-{i}", District = "Dhaka", Thana = "Mirpur" },
                    DetailsPermanentAddress = new Address { HouseNo = $"H-{i}", RoadNo = $"R-{i}", District = "Borisal", Thana = "Sadar" }
                });
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
             await Task.Delay(100); // Simulate network
             return _students;
        }

        public async Task<Student> GetStudentByIdAsync(long id)
        {
             await Task.Delay(50);
             return _students.FirstOrDefault(s => s.Id == id) ?? new Student();
        }

        public async Task CreateStudentAsync(Student student)
        {
             await Task.Delay(200);
             student.Id = _students.Count + 1;
             _students.Add(student);
        }

        public async Task UpdateStudentAsync(Student student)
        {
             await Task.Delay(200);
             var existing = _students.FirstOrDefault(s => s.Id == student.Id);
             if (existing != null)
             {
                 // Simple mapping
                 existing.Name = student.Name;
                 existing.Phone = student.Phone;
                 existing.Class = student.Class;
                 existing.Department = student.Department;
                 existing.DateOfBirth = student.DateOfBirth;
                 // Add other fields as needed
             }
        }

        public async Task DeleteStudentAsync(long id)
        {
             await Task.Delay(200);
             var existing = _students.FirstOrDefault(s => s.Id == id);
             if (existing != null)
             {
                 _students.Remove(existing);
             }
        }
    }
}
