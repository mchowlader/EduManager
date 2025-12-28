namespace EduSystem.UI.Web.Client.Models.Academy;

public class TranscriptModel
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Session { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string RollNo { get; set; } = string.Empty;
    public double GPA { get; set; }
    public string Result { get; set; } = string.Empty;
    public List<SubjectGrade> Grades { get; set; } = new();
}

public class SubjectGrade
{
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public double Marks { get; set; }
    public string Grade { get; set; } = string.Empty;
    public double GradePoint { get; set; }
}

public class TestimonialModel
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string PostOffice { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string ExamYear { get; set; } = string.Empty;
    public string GPA { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string IssueDate { get; set; } = DateTime.Now.ToString("dd MMMM yyyy");
    public string SlNo { get; set; } = "31-03";
    public string Group { get; set; } = "Science";
    public string Board { get; set; } = "Secondary and Higher Secondary Examination Board";
    public string Roll { get; set; } = "Shahrasti-2";
    public string No { get; set; } = "163715";
    public string RegistrationNo { get; set; } = "790821";
    public string Session { get; set; } = "1999";
    public string Conduct { get; set; } = "satisfactory";
    public string EstablishedYear { get; set; } = "1947";
    public string Telephone { get; set; } = "0123-456789";
    public string POB { get; set; } = "708";
}
