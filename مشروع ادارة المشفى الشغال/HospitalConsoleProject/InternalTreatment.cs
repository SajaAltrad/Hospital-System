namespace HospitalConsoleProject;

public class InternalTreatment : Treatment
{
    public DateTime GraduationDate { get; set; }

    public InternalTreatment(int treatmentId, int patientId, DateTime treatmentDate, double cost, string notes,
        DateTime graduationDate, string departmentName, List<int> supervisorDoctorIds)
        : base(treatmentId, patientId, treatmentDate, cost, notes, departmentName, supervisorDoctorIds)
    {
        GraduationDate = graduationDate;
    }

    public override string Type => "Internal";

    public override void ShowInfo()
    {
        base.ShowInfo();
        Console.WriteLine($"Graduation: {GraduationDate:yyyy-MM-dd} | Supervisor doctors: {string.Join(',', DoctorIds)}");
    }

    public override string ToFileLine()
    {
        return $"InternalTreatment|{TreatmentId}|{PatientId}|{TreatmentDate:yyyy-MM-dd}|{Cost}|{Notes}|{DepartmentName}|{GraduationDate:yyyy-MM-dd}|{string.Join(',', DoctorIds)}";
    }
}
