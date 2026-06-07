namespace HospitalConsoleProject;

public class Program
{
    static Hospital hospital = new();

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        SeedData();

        while (true)
        {
            PrintMenu();
            Console.Write("Choose: ");
            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": AddDoctorMenu(); break;
                case "2": DeleteDoctorMenu(); break;
                case "3": AddPatientMenu(); break;
                case "4": DischargePatientMenu(); break;
                case "5": AcceptExternalPatientMenu(); break;
                case "6": AddTreatmentMenu(); break;
                case "7": ShowTreatmentsDuringPeriodMenu(); break;
                case "8": ShowPatientsInAllDepartmentsMenu(); break;
                case "9": ShowPatientTreatmentsMenu(); break;
                case "10": PromoteResidentMenu(); break;
                case "11": Console.WriteLine($"Resident doctors count: {hospital.CountResidentDoctors()}"); break;
                case "12": CountPatientsInDepartmentMenu(); break;
                case "13": hospital.ShowDoctors(); break;
                case "14": hospital.ShowPatients(); break;
                case "15": hospital.SaveToFiles(); Console.WriteLine("Saved to doctors.txt, patients.txt, treatments.txt"); break;
                case "16": hospital.LoadFromFiles(); Console.WriteLine("Loaded from text files."); break;
                case "0": return;
                default: Console.WriteLine("Wrong choice."); break;
            }

            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }
    }

    static void PrintMenu()
    {
        Console.Clear();
        Console.WriteLine("===== Hospital Console System =====");
        Console.WriteLine("1- Add Doctor");
        Console.WriteLine("2- Delete Doctor");
        Console.WriteLine("3- Add Patient");
        Console.WriteLine("4- Discharge Internal Patient");
        Console.WriteLine("5- Accept External Patient");
        Console.WriteLine("6- Add Treatment To Patient");
        Console.WriteLine("7- Filter Treatments During Period (Patient + Doctor + Department)");
        Console.WriteLine("8- Show Patients Treated In All Departments During Period");
        Console.WriteLine("9- Show All Treatments For Patient");
        Console.WriteLine("10- Promote Resident Doctor");
        Console.WriteLine("11- Count Resident Doctors");
        Console.WriteLine("12- Count Patients In Department During Period");
        Console.WriteLine("13- Show Doctors");
        Console.WriteLine("14- Show Patients");
        Console.WriteLine("15- Save To Text Files");
        Console.WriteLine("16- Load From Text Files");
        Console.WriteLine("0- Exit");
    }

    static int ReadInt(string message)
    {
        Console.Write(message);
        return int.Parse(Console.ReadLine()!);
    }

    static double ReadDouble(string message)
    {
        Console.Write(message);
        return double.Parse(Console.ReadLine()!);
    }

    static string ReadString(string message)
    {
        Console.Write(message);
        return Console.ReadLine()!;
    }

    static DateTime ReadDate(string message)
    {
        Console.Write(message + " yyyy-mm-dd: ");
        return DateTime.Parse(Console.ReadLine()!);
    }

    static void AddDoctorMenu()
    {
        int id = ReadInt("Id: ");
        if(hospital.Doctors.Find(id) != null)
        {
            Console.WriteLine("doctor id is already exists");
            return ;
        }
        int type = ReadInt("Doctor type 1-Permanent 2-Resident 3-Contract: ");
        string name = ReadString("Name: ");
        string address = ReadString("Address: ");
        DateTime birth = ReadDate("Birth date");
        string department = ReadString("Department name : ");

        bool added = false;
        if (type == 1)
        {
            double salary = ReadDouble("Base salary: ");
            DateTime hire = ReadDate("Hire date");
            added = hospital.AddDoctor(new PermanentDoctor(id, name, address, birth, department, salary, hire));
        }
        else if (type == 2)
        {
            DateTime start = ReadDate("Training start");
            DateTime end = ReadDate("Training end");
            double baseSalary = ReadDouble("Permanent base salary: ");
            added = hospital.AddDoctor(new ResidentDoctor(id, name, address, birth, department, start, end, baseSalary));
        }
        else if (type == 3)
        {
            added = hospital.AddDoctor(new ContractDoctor(id, name, address, birth, department));
        }

        Console.WriteLine(added ? "Doctor added." : "Doctor id already exists or wrong type.");
    }

    static void DeleteDoctorMenu()
    {
        int id = ReadInt("Doctor id: ");
        Console.WriteLine(hospital.DeleteDoctor(id) ? "Deleted." : "Doctor not found.");
    }

    static void AddPatientMenu()
    {
        int id = ReadInt("Id: ");
        if(hospital.Patients.Find(id) != null)
        {
            Console.WriteLine("patient id already exists");
            return;
        }
        int type = ReadInt("Patient type 1-Internal 2-External: ");
        string name = ReadString("Name: ");
        string address = ReadString("Address: ");
        DateTime birth = ReadDate("Birth date");

        bool added;
        if (type == 1)
        {
            string dep = ReadString("Department name for admission: ");
            added = hospital.AddPatient(new InternalPatient(id, name, address, birth, dep));
        }
        else
        {
            added = hospital.AddPatient(new ExternalPatient(id, name, address, birth));
        }
        Console.WriteLine(added ? "Patient added." : "Patient id already exists.");
    }

    static void DischargePatientMenu()
    {
        int id = ReadInt("Internal patient id: ");
        Console.WriteLine(hospital.DischargePatient(id) ? "Patient discharged." : "Internal patient not found.");
    }

    static void AcceptExternalPatientMenu()
    {
        int id = ReadInt("External patient id: ");
        Console.WriteLine(hospital.AcceptExternalPatient(id) ? "Patient accepted." : "External patient not found.");
    }

    static void AddTreatmentMenu()
    {
        int type = ReadInt("Treatment type 1-Internal 2-External: ");
        int tid = ReadInt("Treatment id: ");
        if(hospital.TreatmentIdExists(tid))
            {
            Console.WriteLine("treatment id is already exists");
            }
        int pid = ReadInt("Patient id: ");
        DateTime date = ReadDate("Treatment date");
        double cost = ReadDouble("Cost: ");
        string notes = ReadString("Notes: ");
        string department = ReadString("Department name: ");

        Treatment treatment;
        if (type == 1)
        {
            DateTime grad = ReadDate("Graduation date");
            string supervisorsText = ReadString("Supervisor doctor ids separated by comma: ");
            List<int> supervisors = supervisorsText.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            treatment = new InternalTreatment(tid, pid, date, cost, notes, grad, department, supervisors);
        }
        else
        {
            int clinic = ReadInt("Clinic id: ");
            int doctor = ReadInt("Doctor id: ");
            treatment = new ExternalTreatment(tid, pid, date, cost, notes, clinic, doctor, department);
        }

        Console.WriteLine(hospital.AddTreatmentToPatient(pid, treatment) ? "Treatment added." : "Treatment was not added.");
    }

    static void ShowTreatmentsDuringPeriodMenu()
    {
        DateTime from = ReadDate("From");
        DateTime to = ReadDate("To");
        hospital.ShowTreatmentsDuringPeriod(from, to);
    }

    static void ShowPatientsInAllDepartmentsMenu()
    {
        DateTime from = ReadDate("From");
        DateTime to = ReadDate("To");
        hospital.ShowPatientsTreatedInAllDepartments(from, to);
    }

    static void ShowPatientTreatmentsMenu()
    {
        int id = ReadInt("Patient id: ");
        hospital.ShowPatientTreatments(id);
    }

    static void PromoteResidentMenu()
    {
        int id = ReadInt("Resident doctor id: ");
        Console.WriteLine(hospital.PromoteResidentDoctor(id) ? "Doctor promoted." : "Resident doctor not found.");
    }

    static void CountPatientsInDepartmentMenu()
    {
        string dep = ReadString("Department name: ");
        DateTime from = ReadDate("From");
        DateTime to = ReadDate("To");
        Console.WriteLine($"Count: {hospital.CountPatientsInDepartment(dep, from, to)}");
    }

    static void SeedData()
    {
        hospital.AddDoctor(new PermanentDoctor(1, "Ahmad", "Damascus", new DateTime(1980, 1, 1), "eye", 1000, new DateTime(2020, 1, 1)));
        hospital.AddDoctor(new ResidentDoctor(2, "Sara", "Aleppo", new DateTime(1998, 5, 10), "skin", new DateTime(2025, 1, 1), new DateTime(2027, 1, 1), 1000));
        hospital.AddDoctor(new ContractDoctor(3, "Omar", "Homs", new DateTime(1975, 3, 3), "emergancy"));

        hospital.AddPatient(new InternalPatient(1, "Ali", "Latakia", new DateTime(2000, 2, 2) , "eye"));
        hospital.AddPatient(new ExternalPatient(2, "Mona", "Hama", new DateTime(1995, 4, 4)));

        hospital.AddTreatmentToPatient(1, new InternalTreatment(1, 1, new DateTime(2026, 2, 10), 500, "Eye operation", new DateTime(2026, 2, 20), "eye", new List<int> { 1 }));
        hospital.AddTreatmentToPatient(1, new ExternalTreatment(2, 1, new DateTime(2026, 2, 12), 150, "Skin check", 201, 2, "skin"));
        hospital.AddTreatmentToPatient(2, new ExternalTreatment(3, 2, new DateTime(2026, 2, 15), 300, "Emergency visit", 301, 3,"emergancy "));
    }
}
