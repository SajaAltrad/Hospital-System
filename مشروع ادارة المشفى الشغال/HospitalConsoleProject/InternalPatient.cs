namespace HospitalConsoleProject;

public class InternalPatient : Patient
{
    public bool IsDischarged { get; set; }
    public string DepartmentName { get; set; }
    public List<InternalTreatment> InternalTreatments { get; set; } = new();
    public List<ExternalTreatment> ExternalTreatments { get; set; } = new();

    public InternalPatient(int id, string name, string address, DateTime birthDate, string departmentName)
        : base(id, name, address, birthDate)
    {
        DepartmentName = Department.Normalize(departmentName);
        IsDischarged = false;
    }

    public override string Type => "Internal";

    public void Discharge() => IsDischarged = true;

    public override void AddTreatment(Treatment treatment)
    {
        if (treatment is InternalTreatment internalTreatment) InternalTreatments.Add(internalTreatment);
        else if (treatment is ExternalTreatment externalTreatment) ExternalTreatments.Add(externalTreatment);
    }

    public override IEnumerable<Treatment> GetAllTreatments()
    {
        foreach (var t in InternalTreatments) yield return t;
        foreach (var t in ExternalTreatments) yield return t;
    }

    public override void ShowInfo()
    {
        base.ShowInfo();
        Console.WriteLine($"Department: {DepartmentName} | Discharged: {IsDischarged}");
    }

    public override string ToFileLine()
    {
        return $"Internal|{Id}|{Name}|{Address}|{BirthDate:yyyy-MM-dd}|{DepartmentName}|{IsDischarged}";
    }
}
