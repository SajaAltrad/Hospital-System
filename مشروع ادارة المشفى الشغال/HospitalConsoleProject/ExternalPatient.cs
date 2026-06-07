namespace HospitalConsoleProject;

public class ExternalPatient : Patient
{
    public bool IsAccepted { get; set; }
    public List<ExternalTreatment> ExternalTreatments { get; set; } = new();

    public ExternalPatient(int id, string name, string address, DateTime birthDate)
        : base(id, name, address, birthDate)
    {
        IsAccepted = false;
    }

    public override string Type => "External";

    public void Accept() => IsAccepted = true;

    public override void AddTreatment(Treatment treatment)
    {
        if (treatment is ExternalTreatment externalTreatment) ExternalTreatments.Add(externalTreatment);
        else Console.WriteLine("Warning: External patient should receive external treatment only.");
    }

    public override IEnumerable<Treatment> GetAllTreatments() => ExternalTreatments;

    public override void ShowInfo()
    {
        base.ShowInfo();
        Console.WriteLine($"Accepted: {IsAccepted}");
    }

    public override string ToFileLine()
    {
        return $"External|{Id}|{Name}|{Address}|{BirthDate:yyyy-MM-dd}|{IsAccepted}";
    }
}
