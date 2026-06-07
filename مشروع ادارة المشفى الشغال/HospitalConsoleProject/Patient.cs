namespace HospitalConsoleProject;

public abstract class Patient : IHasId
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public DateTime BirthDate { get; set; }

    protected Patient(int id, string name, string address, DateTime birthDate)
    {
        Id = id;
        Name = name;
        Address = address;
        BirthDate = birthDate;
    }

    public abstract string Type { get; }
    public abstract IEnumerable<Treatment> GetAllTreatments();
    public abstract void AddTreatment(Treatment treatment);

    public virtual void ShowInfo()
    {
        Console.WriteLine($"Patient ID: {Id} | Name: {Name} | Address: {Address} | BirthDate: {BirthDate:yyyy-MM-dd} | Type: {Type}");
    }

    public void ShowTreatments()
    {
        ShowInfo();
        var treatments = GetAllTreatments().ToList();
        if (treatments.Count == 0)
        {
            Console.WriteLine("No treatments.");
            return;
        }

        foreach (var treatment in treatments)
        {
            treatment.ShowInfo();
            Console.WriteLine("------------------");
        }
    }

    public abstract string ToFileLine();
}
