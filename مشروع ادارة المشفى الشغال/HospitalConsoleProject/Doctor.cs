namespace HospitalConsoleProject;

public abstract class Doctor : IHasId
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public DateTime BirthDate { get; set; }
    public string DepartmentName { get; set; }

    protected Doctor(int id, string name, string address, DateTime birthDate, string departmentName)
    {
        Id = id;
        Name = name;
        Address = address;
        BirthDate = birthDate;
        DepartmentName = Department.Normalize(departmentName);
    }

    public abstract string Type { get; }
    public abstract double CalculateSalary(IEnumerable<Treatment> treatments);

    public virtual void ShowInfo(IEnumerable<Treatment> treatments)
    {
        Console.WriteLine($"Doctor ID: {Id} | Name: {Name} | Address: {Address} | BirthDate: {BirthDate:yyyy-MM-dd} | Department: {DepartmentName} | Type: {Type} | Salary: {CalculateSalary(treatments)}");
    }

    public abstract string ToFileLine();
}
