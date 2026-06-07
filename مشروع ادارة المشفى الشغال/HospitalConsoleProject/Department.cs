namespace HospitalConsoleProject;

public static class Department
{
    public static string Normalize(string department)
    {
        return string.IsNullOrWhiteSpace(department) ? "غير محدد" : department.Trim();
    }
}
