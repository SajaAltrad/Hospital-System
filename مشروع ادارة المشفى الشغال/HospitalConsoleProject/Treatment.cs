namespace HospitalConsoleProject;

public abstract class Treatment
{
    public int TreatmentId { get; set; }
    public int PatientId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public double Cost { get; set; }
    public string Notes { get; set; }
    public string DepartmentName { get; set; }
    public List<int> DoctorIds { get; set; }

    protected Treatment(int treatmentId, int patientId, DateTime treatmentDate, double cost, string notes, string departmentName, IEnumerable<int> doctorIds)
    {
        TreatmentId = treatmentId;
        PatientId = patientId;
        TreatmentDate = treatmentDate;
        Cost = cost;
        Notes = notes;
        DepartmentName = Department.Normalize(departmentName);
        DoctorIds = doctorIds.ToList();
    }

    public abstract string Type { get; }

    public virtual void ShowInfo()
    {
        Console.WriteLine($"Treatment #{TreatmentId} | Patient: {PatientId} | Date: {TreatmentDate:yyyy-MM-dd} | Cost: {Cost} | Type: {Type} | Department: {DepartmentName} | Doctors: {string.Join(',', DoctorIds)} | Notes: {Notes}");
    }

    public abstract string ToFileLine();
}
