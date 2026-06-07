using System.Security.Cryptography.X509Certificates;

namespace HospitalConsoleProject;

public class Hospital
{
    public SortedLinkedList<Doctor> Doctors { get; set; } = new();
    public SortedLinkedList<Patient> Patients { get; set; } = new();

    public List<Treatment> AllTreatments => Patients.SelectMany(p => p.GetAllTreatments()).ToList();

    public bool AddDoctor(Doctor doctor) => Doctors.Add(doctor);
    public bool DeleteDoctor(int id) => Doctors.Remove(id);
    public Doctor? FindDoctor(int id) => Doctors.Find(id);
    public Patient? FindPatient(int id) => Patients.Find(id);

    public bool AddPatient(Patient patient) => Patients.Add(patient);

    public bool DischargePatient(int patientId)
    {
        if (Patients.Find(patientId) is not InternalPatient patient) return false;
        patient.Discharge();
        return true;
    }

    public bool AcceptExternalPatient(int patientId)
    {
        if (Patients.Find(patientId) is not ExternalPatient patient) return false;
        patient.Accept();
        return true;
    }

    public bool AddTreatmentToPatient(int patientId, Treatment treatment)
    {
        Patient? patient = Patients.Find(patientId);
        if (patient == null) return false;

        foreach (int doctorId in treatment.DoctorIds)
        {
            if (Doctors.Find(doctorId) == null)
            {
                Console.WriteLine($"Doctor id {doctorId} not found. Treatment was not added.");
                return false;
            }
        }

        patient.AddTreatment(treatment);
        return true;
    }

    public void ShowTreatmentsDuringPeriod(DateTime startdate, DateTime enddate)
    {
        var query = from patient in Patients
                    from treatment in patient.GetAllTreatments()
                    where treatment.TreatmentDate >= startdate
                    where treatment.TreatmentDate <= enddate
                    orderby treatment.TreatmentDate, patient.Id
                    select new { patient, treatment };

        bool found = false;
        foreach (var item in query)
        {
            found = true;
            string doctorNames = string.Join(", ", item.treatment.DoctorIds.Select(id =>
            {
                Doctor? d = Doctors.Find(id);
                return d == null ? $"Unknown doctor #{id}" : $"{d.Name} #{d.Id} - {d.DepartmentName}";
            }));

            Console.WriteLine($"Patient: {item.patient.Name} #{item.patient.Id} | Treatment: {item.treatment.TreatmentId} | Date: {item.treatment.TreatmentDate:yyyy-MM-dd} | Department: {item.treatment.DepartmentName} | Doctors: {doctorNames} | Cost: {item.treatment.Cost} | Type: {item.treatment.Type}");
        }

        if (!found) Console.WriteLine("No treatments found in this period.");
    }

    public void ShowPatientsTreatedInAllDepartments(DateTime from, DateTime to)
    {
        var allDepartments = Doctors.Select(d => d.DepartmentName).Distinct().ToList();
        if (allDepartments.Count == 0)
        {
            Console.WriteLine("No departments found.");
            return;
        }

        var result = Patients.Where(p =>
        {
            var patientDepartments = p.GetAllTreatments()
                .Where(t => t.TreatmentDate >= from && t.TreatmentDate <= to)
                .Select(t => t.DepartmentName)
                .Distinct()
                .ToList();
            return allDepartments.All(dep => patientDepartments.Contains(dep));
        }).ToList();

        if (result.Count == 0)
        {
            Console.WriteLine("No patient was treated in all departments during this period.");
            return;
        }

        foreach (var patient in result)
        {
            patient.ShowInfo();
            Console.WriteLine("------------------");
        }
    }

    public void ShowPatientTreatments(int patientId)
    {
        Patient? patient = Patients.Find(patientId);
        if (patient == null)
        {
            Console.WriteLine("Patient not found.");
            return;
        }

        patient.ShowInfo();
        var treatments = patient.GetAllTreatments().OrderBy(t => t.TreatmentDate).ToList();
        if (treatments.Count == 0)
        {
            Console.WriteLine("No treatments.");
            return;
        }

        foreach (var treatment in treatments)
        {
            string doctorNames = string.Join(", ", treatment.DoctorIds.Select(id => Doctors.Find(id)?.Name ?? $"Unknown #{id}"));
            Console.WriteLine($"Treatment #{treatment.TreatmentId} | Date: {treatment.TreatmentDate:yyyy-MM-dd} | Department: {treatment.DepartmentName} | Doctor(s): {doctorNames} | Cost: {treatment.Cost} | Type: {treatment.Type} | Notes: {treatment.Notes}");
        }
    }

    public bool PromoteResidentDoctor(int doctorId)
    {
        if (Doctors.Find(doctorId) is not ResidentDoctor resident) return false;
        Doctors.Remove(doctorId);
        Doctors.Add(resident.PromoteToPermanent());
        return true;
    }

    public int CountResidentDoctors() => Doctors.OfType<ResidentDoctor>().Count();

    public int CountPatientsInDepartment(string departmentName, DateTime from, DateTime to)
    {
        departmentName = Department.Normalize(departmentName);
        return Patients.Count(p => p.GetAllTreatments()
            .Any(t => t.DepartmentName.Equals(departmentName, StringComparison.OrdinalIgnoreCase)
                      && t.TreatmentDate >= from && t.TreatmentDate <= to));
    }

    public void ShowDoctors()
    {
        foreach (var doctor in Doctors)
        {
            doctor.ShowInfo(AllTreatments);
            Console.WriteLine("------------------");
        }
    }

    public void ShowPatients()
    {
        foreach (var patient in Patients)
        {
            patient.ShowInfo();
            Console.WriteLine("------------------");
        }
    }

    public void SaveToFiles()
    {
        File.WriteAllLines("doctors.txt", Doctors.Select(d => d.ToFileLine()));
        File.WriteAllLines("patients.txt", Patients.Select(p => p.ToFileLine()));
        File.WriteAllLines("treatments.txt", AllTreatments.Select(t => t.ToFileLine()));
    }

    public void LoadFromFiles()
    {
        Doctors.Clear();
        Patients.Clear();

        if (File.Exists("doctors.txt"))
        {
            foreach (string line in File.ReadAllLines("doctors.txt"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] p = line.Split('|');
                if (p[0] == "Permanent") AddDoctor(new PermanentDoctor(int.Parse(p[1]), p[2], p[3], DateTime.Parse(p[4]), p[5], double.Parse(p[6]), DateTime.Parse(p[7])));
                else if (p[0] == "Resident") AddDoctor(new ResidentDoctor(int.Parse(p[1]), p[2], p[3], DateTime.Parse(p[4]), p[5], DateTime.Parse(p[6]), DateTime.Parse(p[7]), double.Parse(p[8])));
                else if (p[0] == "Contract") AddDoctor(new ContractDoctor(int.Parse(p[1]), p[2], p[3], DateTime.Parse(p[4]), p[5]));
            }
        }

        if (File.Exists("patients.txt"))
        {
            foreach (string line in File.ReadAllLines("patients.txt"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] p = line.Split('|');
                if (p[0] == "Internal")
                {
                    var patient = new InternalPatient(int.Parse(p[1]), p[2], p[3], DateTime.Parse(p[4]), p[5]);
                    patient.IsDischarged = bool.Parse(p[6]);
                    AddPatient(patient);
                }
                else if (p[0] == "External")
                {
                    var patient = new ExternalPatient(int.Parse(p[1]), p[2], p[3], DateTime.Parse(p[4]));
                    patient.IsAccepted = bool.Parse(p[5]);
                    AddPatient(patient);
                }
            }
        }

        if (File.Exists("treatments.txt"))
        {
            foreach (string line in File.ReadAllLines("treatments.txt"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] p = line.Split('|');
                Treatment? treatment = null;
                int patientId = int.Parse(p[2]);

                if (p[0] == "InternalTreatment")
                {
                    var doctors = p[8].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    treatment = new InternalTreatment(int.Parse(p[1]), patientId, DateTime.Parse(p[3]), double.Parse(p[4]), p[5], DateTime.Parse(p[7]), p[6], doctors);
                }
                else if (p[0] == "ExternalTreatment")
                {
                    treatment = new ExternalTreatment(int.Parse(p[1]), patientId, DateTime.Parse(p[3]), double.Parse(p[4]), p[5], int.Parse(p[7]), int.Parse(p[8]), p[6]);
                }

                if (treatment != null)
                    AddTreatmentToPatient(patientId, treatment);
            }
        }
        
    }
    public bool TreatmentIdExists(int id)
    {
        foreach (var patient in Patients)
        {
            foreach (var treatment in patient.GetAllTreatments())
            {
                if (treatment.TreatmentId == id)
                    return true;
            }
        }
        return false;
    }
}
