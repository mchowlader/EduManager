using EduSystem.UI.Web.Client.Models.Academy;

namespace EduSystem.UI.Web.Client.Services.Academy;

public class AcademyService : IAcademyService
{
    public async Task<TranscriptModel?> GetTranscriptAsync(string studentId)
    {
        await Task.Delay(500); // Simulate API call
        
        if (string.IsNullOrEmpty(studentId)) return null;

        return new TranscriptModel
        {
            StudentId = studentId,
            StudentName = "Md. Mithun Islam",
            Class = "Class 10",
            Session = "2024-2025",
            Department = "Science",
            RollNo = "101",
            GPA = 4.85,
            Result = "Passed",
            Grades = new List<SubjectGrade>
            {
                new() { SubjectCode = "101", SubjectName = "Bangla 1st Paper", Marks = 85, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "102", SubjectName = "Bangla 2nd Paper", Marks = 82, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "107", SubjectName = "English 1st Paper", Marks = 78, Grade = "A", GradePoint = 4.0 },
                new() { SubjectCode = "108", SubjectName = "English 2nd Paper", Marks = 75, Grade = "A", GradePoint = 4.0 },
                new() { SubjectCode = "111", SubjectName = "General Mathematics", Marks = 92, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "127", SubjectName = "Physics", Marks = 88, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "138", SubjectName = "Biology", Marks = 90, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "143", SubjectName = "Chemistry", Marks = 84, Grade = "A+", GradePoint = 5.0 },
                new() { SubjectCode = "154", SubjectName = "Information Technology", Marks = 95, Grade = "A+", GradePoint = 5.0 }
            }
        };
    }

    public async Task<TestimonialModel?> GetTestimonialAsync(string studentId)
    {
        await Task.Delay(500); // Simulate API call

        if (string.IsNullOrEmpty(studentId)) return null;

        return new TestimonialModel
        {
            StudentId = studentId,
            StudentName = "Mohd. Rasadul Islam",
            FatherName = "Mohd. Delwer Hossain",
            MotherName = "Fatema Begum",
            Village = "Khurua",
            PostOffice = "Chitoshi",
            Upazila = "Monohorgonj",
            District = "Comilla",
            Class = "Class 10",
            ExamYear = "2003",
            GPA = "2.50",
            DateOfBirth = new DateTime(1987, 5, 15),
            SlNo = "31-03",
            Conduct = "satisfactory",
            Group = "Science",
            Board = "Secondary and Higher Secondary Examination Board",
            Roll = "Shahrasti-2",
            No = "163715",
            RegistrationNo = "790821",
            Session = "1999",
            EstablishedYear = "1947",
            Telephone = "043-20667",
            POB = "708"
        };
    }
}
