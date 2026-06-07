namespace HospitalConsoleProject;

public class ContractDoctor : Doctor
{
    public ContractDoctor(int id, string name, string address, DateTime birthDate, string departmentName)
        : base(id, name, address, birthDate, departmentName)
    {
    }

    public override string Type => "Contract";

    public override double CalculateSalary(IEnumerable<Treatment> treatments)
    {
        double total = treatments.Where(t => t.DoctorIds.Contains(Id)).Sum(t => t.Cost);
        return Math.Round(total * 0.50, 2);
    }

    public override string ToFileLine()
    {
        return $"Contract|{Id}|{Name}|{Address}|{BirthDate:yyyy-MM-dd}|{DepartmentName}";
    }
}
