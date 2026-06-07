namespace HospitalConsoleProject;

public class ExternalTreatment : Treatment
{
    public int ClinicId { get; set; }
    public int DoctorId => DoctorIds.FirstOrDefault();

    public ExternalTreatment(int treatmentId, int patientId, DateTime treatmentDate, double cost, string notes,
        int clinicId, int doctorId, string departmentName)
        : base(treatmentId, patientId, treatmentDate, cost, notes, departmentName, new[] { doctorId })
    {
        ClinicId = clinicId;
    }

    public override string Type => "External";

    public override void ShowInfo()
    {
        base.ShowInfo();
        Console.WriteLine($"Clinic: {ClinicId} | Doctor: {DoctorId}");
    }

    public override string ToFileLine()
    {
        return $"ExternalTreatment|{TreatmentId}|{PatientId}|{TreatmentDate:yyyy-MM-dd}|{Cost}|{Notes}|{DepartmentName}|{ClinicId}|{DoctorId}";
    }
}
