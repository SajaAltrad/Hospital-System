namespace HospitalConsoleProject;

public class ResidentDoctor : Doctor
{
    public DateTime TrainingStart { get; set; }
    public DateTime TrainingEnd { get; set; }
    public double PermanentBaseSalary { get; set; }

    public ResidentDoctor(int id, string name, string address, DateTime birthDate, string departmentName,
        DateTime trainingStart, DateTime trainingEnd, double permanentBaseSalary)
        : base(id, name, address, birthDate, departmentName)
    {
        TrainingStart = trainingStart;
        TrainingEnd = trainingEnd;
        PermanentBaseSalary = permanentBaseSalary;
    }

    public override string Type => "Resident";

    public override double CalculateSalary(IEnumerable<Treatment> treatments)
    {
        int years = DateTime.Now.Year - TrainingStart.Year;
        if (DateTime.Now < TrainingStart.AddYears(years)) years--;
        if (years <= 0) return Math.Round(PermanentBaseSalary * 0.50, 2);
        if (years == 1) return Math.Round(PermanentBaseSalary * 0.75, 2);
        return PermanentBaseSalary;
    }

    public PermanentDoctor PromoteToPermanent()
    {
        return new PermanentDoctor(Id, Name, Address, BirthDate, DepartmentName, PermanentBaseSalary, DateTime.Now);
    }

    public override string ToFileLine()
    {
        return $"Resident|{Id}|{Name}|{Address}|{BirthDate:yyyy-MM-dd}|{DepartmentName}|{TrainingStart:yyyy-MM-dd}|{TrainingEnd:yyyy-MM-dd}|{PermanentBaseSalary}";
    }
}
