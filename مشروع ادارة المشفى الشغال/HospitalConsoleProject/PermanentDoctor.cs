namespace HospitalConsoleProject;

public class PermanentDoctor : Doctor
{
    public double BaseSalary { get; set; }
    public DateTime HireDate { get; set; }

    public PermanentDoctor(int id, string name, string address, DateTime birthDate, string departmentName, double baseSalary, DateTime hireDate)
        : base(id, name, address, birthDate, departmentName)
    {
        BaseSalary = baseSalary;
        HireDate = hireDate;
    }

    public override string Type => "Permanent";

    public override double CalculateSalary(IEnumerable<Treatment> treatments)
    {
        int years = DateTime.Now.Year - HireDate.Year;
        if (DateTime.Now < HireDate.AddYears(years)) years--;
        int increases = Math.Max(0, years / 2);
        return Math.Round(BaseSalary * Math.Pow(1.10, increases), 2);
    }

    public override string ToFileLine()
    {
        return $"Permanent|{Id}|{Name}|{Address}|{BirthDate:yyyy-MM-dd}|{DepartmentName}|{BaseSalary}|{HireDate:yyyy-MM-dd}";
    }
}
