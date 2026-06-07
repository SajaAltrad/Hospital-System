using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedProgramingProject
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        //Doctors
        private void LoadDoctors()
        {
            using (SqlConnection con = Db.GetConnection())
            {
                string query = @"
        SELECT 
            Id AS [رقم الطبيب],
            Name AS [اسم الطبيب],
            BirthDate AS [تاريخ الميلاد],
            DepartmentName AS [القسم],
            DoctorType AS [نوع الطبيب],
            BaseSalary AS [الراتب الأساسي],
            HireDate AS [تاريخ التعيين],
            TrainingStart AS [بداية التدريب],
            TrainingEnd AS [نهاية التدريب]
        FROM Doctors";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable table = new DataTable();

                adapter.Fill(table);

                table.Columns.Add("الراتب المحسوب", typeof(double));

                foreach (DataRow row in table.Rows)
                {
                    int id = Convert.ToInt32(row["رقم الطبيب"]);
                    row["الراتب المحسوب"] = CalculateDoctorSalary(id);
                }

                DoctorsData_grdView.DataSource = table;
            }
        }

        private void ClearDoctorFields()
        {
            DoctorId_txtBox.Clear();
            DoctorName_txtBox.Clear();
            BaseSalary_txtBox.Clear();
        }

        private void DoctorType_cmpBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DoctorType_cmpBox.Text == "متدرب")
            {
                TrainingStart_picker.Enabled = true;
                TrainingEnd_picker.Enabled = true;
                HireDate_picker.Enabled = false;
            }
            else
            {
                TrainingStart_picker.Enabled = false;
                TrainingEnd_picker.Enabled = false;
                HireDate_picker.Enabled = true;
            }


            if (DoctorType_cmpBox.Text == "متعاقد")
            {
                BaseSalary_txtBox.Enabled = false;
            }
            else
            {
                BaseSalary_txtBox.Enabled = true;
            }
        }

        private double CalculateDoctorSalary(int doctorId)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                con.Open();

                string query = "SELECT DoctorType, BaseSalary, TrainingEnd FROM Doctors WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", doctorId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return 0;

                string type = reader["DoctorType"].ToString();
                double baseSalary = reader["BaseSalary"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BaseSalary"]);

                DateTime? trainingEnd = reader["TrainingEnd"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(reader["TrainingEnd"]);

                reader.Close();

                if (type == "متعاقد")
                {
                    string sumQuery = @"
            SELECT SUM(t.Cost)
            FROM Treatments t
            INNER JOIN TreatmentDoctors td
            ON t.TreatmentId = td.TreatmentId
            WHERE td.DoctorId = @DoctorId";

                    SqlCommand sumCmd = new SqlCommand(sumQuery, con);
                    sumCmd.Parameters.AddWithValue("@DoctorId", doctorId);

                    object result = sumCmd.ExecuteScalar();

                    double total = result == DBNull.Value ? 0 : Convert.ToDouble(result);

                    return total * 0.5;
                }
                else if (type == "متدرب")
                {
                    if (trainingEnd == null)
                        return 0;

                    int years = DateTime.Now.Year - trainingEnd.Value.Year;

                    if (years < 1)
                        return baseSalary * 0.5;
                    else if (years < 2)
                        return baseSalary * 0.75;
                    else
                        return baseSalary;
                }
                else if (type == "مقيم")
                {.
                    int years = DateTime.Now.Year - HireDate_picker.Value.Year;
                    int periods = years / 2;
                    for(int i = 0; i < periods; i++)
                    {
                        baseSalary += baseSalary * 0.10;
                    }

                    return baseSalary;
                }

                return 0;
            }
        }

        private void DoctorAdd_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                if (string.IsNullOrWhiteSpace(DoctorId_txtBox.Text))
                {
                    MessageBox.Show("رقم الطبيب مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(DoctorName_txtBox.Text))
                {
                    MessageBox.Show("اسم الطبيب مطلوب");
                    return;
                }

                if (DoctorDepartment_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر القسم");
                    return;
                }

                if (DoctorType_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر نوع الطبيب");
                    return;
                }

                string query = @"INSERT INTO Doctors
        (Id, Name, BirthDate, DepartmentName, DoctorType, BaseSalary, HireDate, TrainingStart, TrainingEnd)
        VALUES
        (@Id, @Name, @BirthDate, @DepartmentName, @DoctorType, @BaseSalary, @HireDate, @TrainingStart, @TrainingEnd)";

                SqlCommand cmd = new SqlCommand(query, con);

                string doctorType = DoctorType_cmpBox.Text;

                cmd.Parameters.AddWithValue("@Id", int.Parse(DoctorId_txtBox.Text));
                cmd.Parameters.AddWithValue("@Name", DoctorName_txtBox.Text);
                cmd.Parameters.AddWithValue("@BirthDate", DoctorBirthDate_picker.Value.Date);
                cmd.Parameters.AddWithValue("@DepartmentName", DoctorDepartment_cmpBox.Text);
                cmd.Parameters.AddWithValue("@DoctorType", doctorType);


                if (doctorType == "متدرب")
                {
                    cmd.Parameters.AddWithValue("@HireDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingStart", TrainingStart_picker.Value.Date);
                    cmd.Parameters.AddWithValue("@TrainingEnd", TrainingEnd_picker.Value.Date);
                }

                else
                {
                    cmd.Parameters.AddWithValue("@HireDate", HireDate_picker.Value.Date);
                    cmd.Parameters.AddWithValue("@TrainingStart", DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingEnd", DBNull.Value);
                }


                if (doctorType == "متعاقد")
                {
                    cmd.Parameters.AddWithValue("@BaseSalary", DBNull.Value);
                }
                else
                {
                    if (!double.TryParse(BaseSalary_txtBox.Text, out double baseSalary))
                    {
                        MessageBox.Show("الراتب يجب ان يكون رقم");
                        return;
                    }

                    cmd.Parameters.AddWithValue("@BaseSalary", baseSalary);
                }
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Id=@Id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@Id", int.Parse(DoctorId_txtBox.Text));

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("هذا الـ ID موجود مسبقاً");
                    return;
                }

                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("تمت إضافة الطبيب بنجاح");
            ClearDoctorFields();
            LoadDoctors();

        }

        private void ShowAllDoctors_btn_Click(object sender, EventArgs e)
        {
            LoadDoctors();
        }

        private void DoctorRemove_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                string query = "DELETE FROM Doctors WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", int.Parse(DoctorId_txtBox.Text));

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم حذف الطبيب");
            LoadDoctors();
        }

        private void DoctorUpdate_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                if (string.IsNullOrWhiteSpace(DoctorId_txtBox.Text))
                {
                    MessageBox.Show("رقم الطبيب مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(DoctorName_txtBox.Text))
                {
                    MessageBox.Show("اسم الطبيب مطلوب");
                    return;
                }

                if (DoctorDepartment_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر القسم");
                    return;
                }

                if (DoctorType_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر نوع الطبيب");
                    return;
                }

                string query = @"UPDATE Doctors SET
        Name = @Name,
        BirthDate = @BirthDate,
        DepartmentName = @DepartmentName,
        DoctorType = @DoctorType,
        BaseSalary = @BaseSalary,
        HireDate = @HireDate,
        TrainingStart = @TrainingStart,
        TrainingEnd = @TrainingEnd
        WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);

                string doctorType = DoctorType_cmpBox.Text;

                cmd.Parameters.AddWithValue("@Id", int.Parse(DoctorId_txtBox.Text));
                cmd.Parameters.AddWithValue("@Name", DoctorName_txtBox.Text.ToString());
                cmd.Parameters.AddWithValue("@BirthDate", DoctorBirthDate_picker.Value.Date);
                cmd.Parameters.AddWithValue("@DepartmentName", DoctorDepartment_cmpBox.Text);
                cmd.Parameters.AddWithValue("@DoctorType", doctorType);

                double salary = 0;
                if (!string.IsNullOrWhiteSpace(BaseSalary_txtBox.Text))
                {
                    if (!double.TryParse(BaseSalary_txtBox.Text, out salary))
                    {
                        MessageBox.Show("الراتب يجب ان يكون رقم");
                        return;
                    }
                }
                cmd.Parameters.AddWithValue("@BaseSalary", salary);

                if (doctorType == "متدرب")
                {
                    cmd.Parameters.AddWithValue("@HireDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingStart", TrainingStart_picker.Value.Date);
                    cmd.Parameters.AddWithValue("@TrainingEnd", TrainingEnd_picker.Value.Date);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@HireDate", HireDate_picker.Value.Date);
                    cmd.Parameters.AddWithValue("@TrainingStart", DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrainingEnd", DBNull.Value);
                }

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم تعديل الطبيب");
            LoadDoctors();
        }

        private void DoctorsData_grdView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = DoctorsData_grdView.Rows[e.RowIndex];

            DoctorId_txtBox.Text = row.Cells["رقم الطبيب"].Value?.ToString();
            DoctorName_txtBox.Text = row.Cells["اسم الطبيب"].Value?.ToString();
            DoctorDepartment_cmpBox.Text = row.Cells["القسم"].Value?.ToString();
            DoctorType_cmpBox.Text = row.Cells["نوع الطبيب"].Value?.ToString();
            BaseSalary_txtBox.Text = row.Cells["الراتب الأساسي"].Value?.ToString();

            if (row.Cells["تاريخ الميلاد"].Value != DBNull.Value)
                DoctorBirthDate_picker.Value = Convert.ToDateTime(row.Cells["تاريخ الميلاد"].Value);

            if (row.Cells["تاريخ التعيين"].Value != DBNull.Value)
                HireDate_picker.Value = Convert.ToDateTime(row.Cells["تاريخ التعيين"].Value);

            if (row.Cells["بداية التدريب"].Value != DBNull.Value)
                TrainingStart_picker.Value = Convert.ToDateTime(row.Cells["بداية التدريب"].Value);

            if (row.Cells["نهاية التدريب"].Value != DBNull.Value)
                TrainingEnd_picker.Value = Convert.ToDateTime(row.Cells["نهاية التدريب"].Value);
        }


        //Patient
        private void LoadPatients()
        {
            using (SqlConnection con = Db.GetConnection())
            {
                string query = @"
        SELECT 
            Id AS [رقم المريض],
            Name AS [اسم المريض],
            BirthDate AS [تاريخ الميلاد],
            DepartmentName AS [القسم],
            PatientType AS [نوع المريض],
            IsAccepted AS [القبول في قسم],
            Discharged AS [التخريج]
        FROM Patients";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable table = new DataTable();

                adapter.Fill(table);
                PatientsData_grdView.DataSource = table;


                PatientsData_grdView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                PatientsData_grdView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                PatientsData_grdView.ReadOnly = true;
            }
        }

        private void ClearPatientFields()
        {
            PatientId_txtBox.Clear();
            PatientName_txtBox.Clear();

            PatientDepartment_cmpBox.SelectedIndex = -1;
            PatientType_cmpBox.SelectedIndex = -1;

            PatientBirthDate_picker.Value = DateTime.Now;
        }

        private void PatientType_cmpBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isInternal = PatientType_cmpBox.Text == "داخلي";

            // القبول
            if (isInternal)
            {
                IsAccepted_chkBox.Checked = true;
                IsAccepted_chkBox.Enabled = false;

                IsDischarged_chkBox.Enabled = true;
            }
            else
            {
                IsAccepted_chkBox.Enabled = true;

                if (IsAccepted_chkBox.Checked)
                {
                    IsDischarged_chkBox.Enabled = true;
                }
                else
                {
                    IsDischarged_chkBox.Checked = false;
                    IsDischarged_chkBox.Enabled = false;
                }
            }
        }

        private void PatientAdd_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                if (string.IsNullOrWhiteSpace(PatientId_txtBox.Text))
                {
                    MessageBox.Show("رقم المريض مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PatientName_txtBox.Text))
                {
                    MessageBox.Show("اسم المريض مطلوب");
                    return;
                }

               if(PatientDepartment_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر القسم");
                    return;
                }

                if (PatientType_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر نوع المريض");
                    return;
                }

                string query = @"INSERT INTO Patients
        (Id, Name, BirthDate, DepartmentName, PatientType, IsAccepted, Discharged)
        VALUES
        (@Id, @Name, @BirthDate, @DepartmentName, @PatientType, @IsAccepted,@IsDischarged)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", int.Parse(PatientId_txtBox.Text));
                cmd.Parameters.AddWithValue("@Name", PatientName_txtBox.Text);
                cmd.Parameters.AddWithValue("@BirthDate", PatientBirthDate_picker.Value.Date);
                cmd.Parameters.AddWithValue("@DepartmentName", PatientDepartment_cmpBox.Text);
                cmd.Parameters.AddWithValue("@PatientType", PatientType_cmpBox.Text);

                bool isInternal = PatientType_cmpBox.Text == "داخلي";

                cmd.Parameters.AddWithValue("@IsAccepted",
                    isInternal ? true : IsAccepted_chkBox.Checked);

                cmd.Parameters.AddWithValue("@IsDischarged",
                    isInternal ? IsDischarged_chkBox.Checked : false);

                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Patients WHERE Id = @Id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@Id", int.Parse(PatientId_txtBox.Text));

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("هذا رقم المريض موجود مسبقاً");
                    return;
                }
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تمت إضافة المريض بنجاح");
            ClearPatientFields();
            LoadPatients();
        }

        private void ShowAllPatient_btn_Click(object sender, EventArgs e)
        {
            LoadPatients();
        }

        private void PatientRemove_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                string query = "DELETE FROM Patients WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", int.Parse(PatientId_txtBox.Text));

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم حذف المريض");
            LoadPatients();
        }

        private void PatientEdit_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                if (string.IsNullOrWhiteSpace(PatientId_txtBox.Text))
                {
                    MessageBox.Show("رقم المريض مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PatientName_txtBox.Text))
                {
                    MessageBox.Show("اسم المريض مطلوب");
                    return;
                }

                if (PatientDepartment_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر القسم");
                    return;
                }

                if (PatientType_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر نوع المريض");
                    return;
                }

                string query = @"UPDATE Patients SET
        Name = @Name,
        BirthDate = @BirthDate,
        DepartmentName = @DepartmentName,
        PatientType = @PatientType,
        IsAccepted = @IsAccepted,
        Discharged = @IsDischarged
        WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", int.Parse(PatientId_txtBox.Text));
                cmd.Parameters.AddWithValue("@Name", PatientName_txtBox.Text);
                cmd.Parameters.AddWithValue("@BirthDate", PatientBirthDate_picker.Value.Date);
                cmd.Parameters.AddWithValue("@DepartmentName", PatientDepartment_cmpBox.Text);
                cmd.Parameters.AddWithValue("@PatientType", PatientType_cmpBox.Text);
                cmd.Parameters.AddWithValue("@IsAccepted", IsAccepted_chkBox.Checked);
                cmd.Parameters.AddWithValue("@IsDischarged", IsDischarged_chkBox.Checked);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("تم تعديل المريض");
            ClearPatientFields();
            LoadPatients();
        }

        private void PatientsData_grdView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = PatientsData_grdView.Rows[e.RowIndex];

            PatientId_txtBox.Text = row.Cells["رقم المريض"].Value?.ToString();
            PatientName_txtBox.Text = row.Cells["اسم المريض"].Value?.ToString();
            PatientDepartment_cmpBox.Text = row.Cells["القسم"].Value?.ToString();
            PatientType_cmpBox.Text = row.Cells["نوع المريض"].Value?.ToString();

            if (row.Cells["تاريخ الميلاد"].Value != DBNull.Value)
                PatientBirthDate_picker.Value = Convert.ToDateTime(row.Cells["تاريخ الميلاد"].Value);

            IsAccepted_chkBox.Checked = (bool)row.Cells["القبول في قسم"].Value;
            IsDischarged_chkBox.Checked = (bool)row.Cells["التخريج"].Value;
        }


        //Treatment
        private void LoadTreatments()
        {
            using (SqlConnection con = Db.GetConnection())
            {
                string query = @"
SELECT 
    t.TreatmentId AS [رقم المعالجة],
    t.PatientId AS [رقم المريض],
    td.DoctorId AS [رقم الطبيب],
    p.Name AS [اسم المريض],
    d.Name AS [اسم الطبيب],
    t.TreatmentDate AS [التاريخ],
    t.Cost AS [التكلفة],
    t.DepartmentName AS [القسم],
    t.TreatmentType AS [نوع المعالجة],
    t.Notes AS [الملاحظات]
FROM Treatments t
INNER JOIN Patients p ON t.PatientId = p.Id
INNER JOIN TreatmentDoctors td ON t.TreatmentId = td.TreatmentId
INNER JOIN Doctors d ON td.DoctorId = d.Id";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable table = new DataTable();

                adapter.Fill(table);
                TreatmentsData_grdView.DataSource = table;


                TreatmentsData_grdView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                TreatmentsData_grdView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                TreatmentsData_grdView.ReadOnly = true;
            }
        }
        private void ClearTreatmentFields()
        {
            TreatmentId_txtBox.Clear();
            TreatmentPatientId_txtBox.Clear();
            TreatmentDoctorId_txtBox.Clear();
            TreatmentCost_txtBox.Clear();

            TreatmentDepartment_cmpBox.SelectedIndex = -1;
            TreatmentType_cmpBox.SelectedIndex = -1;

            TreatmentDate_picker.Value = DateTime.Now;

            TreatmentsNotes_txtBox.Clear();
        }

        private void TreatmentAdd_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
             
                if (!int.TryParse(TreatmentId_txtBox.Text, out int treatmentId))
                {
                    MessageBox.Show("رقم المعالجة غير صحيح");
                    return;
                }

                if (!int.TryParse(TreatmentPatientId_txtBox.Text, out int patientId))
                {
                    MessageBox.Show("رقم المريض غير صحيح");
                    return;
                }

                if (!int.TryParse(TreatmentDoctorId_txtBox.Text, out int doctorId))
                {
                    MessageBox.Show("رقم الطبيب غير صحيح");
                    return;
                }

                if (!double.TryParse(TreatmentCost_txtBox.Text, out double cost))
                {
                    MessageBox.Show("الكلفة غير صحيحة");
                    return;
                }

                if (TreatmentDepartment_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر القسم");
                    return;
                }

                if (TreatmentType_cmpBox.SelectedIndex == -1)
                {
                    MessageBox.Show("اختر نوع المعالجة");
                    return;
                }

                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                  
                    string checkQuery = "SELECT COUNT(*) FROM Treatments WHERE TreatmentId = @TreatmentId";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con, tran);
                    checkCmd.Parameters.AddWithValue("@TreatmentId", treatmentId);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("رقم المعالجة موجود مسبقاً");
                        tran.Rollback();
                        return;
                    }

            
                    string patientType = "";
                    bool isAccepted = false;
                    bool isDischarged = false;

                    string patientQuery = @"
                SELECT PatientType, IsAccepted, Discharged
                FROM Patients
                WHERE Id = @PatientId";

                    SqlCommand patientCmd = new SqlCommand(patientQuery, con, tran);
                    patientCmd.Parameters.AddWithValue("@PatientId", patientId);

                    using (SqlDataReader reader = patientCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            patientType = reader["PatientType"].ToString();
                            isAccepted = Convert.ToBoolean(reader["IsAccepted"]);
                            isDischarged = Convert.ToBoolean(reader["Discharged"]);
                        }
                    }

                    if (isDischarged)
                    {
                        MessageBox.Show("لا يمكن إضافة معالجة لمريض تم إخراجه من المشفى");
                        tran.Rollback();
                        return;
                    }

                    bool IsInPatientTreatment = TreatmentType_cmpBox.Text == "داخلية";

                    if (patientType == "خارجي")
                    {
                        if (IsInPatientTreatment &&!IsAccepted_chkBox.Checked)
                        {
                            MessageBox.Show("المريض الخارجي لا يستطيع إجراء معالجة داخلية قبل القبول");
                            tran.Rollback();
                            return;
                        }
                    }

        
                    string insertTreatment = @"
                INSERT INTO Treatments
                (TreatmentId, PatientId, TreatmentDate, Cost, DepartmentName, TreatmentType, Notes)
                VALUES
                (@TreatmentId, @PatientId, @TreatmentDate, @Cost, @DepartmentName, @TreatmentType, @Notes)";

                    SqlCommand treatmentCmd = new SqlCommand(insertTreatment, con, tran);
                    treatmentCmd.Parameters.AddWithValue("@TreatmentId", treatmentId);
                    treatmentCmd.Parameters.AddWithValue("@PatientId", patientId); treatmentCmd.Parameters.AddWithValue("@TreatmentDate", TreatmentDate_picker.Value);
                    treatmentCmd.Parameters.AddWithValue("@Cost", cost);
                    treatmentCmd.Parameters.AddWithValue("@DepartmentName", TreatmentDepartment_cmpBox.Text);
                    treatmentCmd.Parameters.AddWithValue("@TreatmentType", TreatmentType_cmpBox.Text);
                    treatmentCmd.Parameters.AddWithValue("@Notes", TreatmentsNotes_txtBox.Text ?? "");

                    treatmentCmd.ExecuteNonQuery();

              
                    string linkQuery = @"
                INSERT INTO TreatmentDoctors
                (TreatmentId, DoctorId)
                VALUES
                (@TreatmentId, @DoctorId)";

                    SqlCommand linkCmd = new SqlCommand(linkQuery, con, tran);
                    linkCmd.Parameters.AddWithValue("@TreatmentId", treatmentId);
                    linkCmd.Parameters.AddWithValue("@DoctorId", doctorId);

                    linkCmd.ExecuteNonQuery();


                    tran.Commit();

                    MessageBox.Show("تمت إضافة المعالجة وربطها بالطبيب بنجاح");

                    ClearTreatmentFields();
                    LoadTreatments();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("حدث خطأ: " + ex.Message);
                }
            }
        }

        private void TreatmentShowAll_btn_Click(object sender, EventArgs e)
        {
            LoadTreatments();
        }

        private void TreatmentRemove_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                con.Open();

                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    string deleteLink = "DELETE FROM TreatmentDoctors WHERE TreatmentId = @TreatmentId";
                    SqlCommand linkCmd = new SqlCommand(deleteLink, con, tran);
                    linkCmd.Parameters.AddWithValue("@TreatmentId", int.Parse(TreatmentId_txtBox.Text));
                    linkCmd.ExecuteNonQuery();

                    string deleteTreatment = "DELETE FROM Treatments WHERE TreatmentId = @TreatmentId";
                    SqlCommand treatmentCmd = new SqlCommand(deleteTreatment, con, tran);
                    treatmentCmd.Parameters.AddWithValue("@TreatmentId", int.Parse(TreatmentId_txtBox.Text));
                    treatmentCmd.ExecuteNonQuery();

                    tran.Commit();

                    MessageBox.Show("تم حذف المعالجة");
                    LoadTreatments();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("خطأ: " + ex.Message);
                }
            }
        }

        private void TreatmentEdit_btn_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = Db.GetConnection())
            {
                con.Open();

                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    string updateTreatment = @"UPDATE Treatments SET
            PatientId = @PatientId,
            TreatmentDate = @TreatmentDate,
            Cost = @Cost,
            DepartmentName = @DepartmentName,
            TreatmentType = @TreatmentType
            WHERE TreatmentId = @TreatmentId";

                    SqlCommand treatmentCmd = new SqlCommand(updateTreatment, con, tran);

                    treatmentCmd.Parameters.AddWithValue("@TreatmentId", int.Parse(TreatmentId_txtBox.Text));
                    treatmentCmd.Parameters.AddWithValue("@PatientId", int.Parse(TreatmentPatientId_txtBox.Text));
                    treatmentCmd.Parameters.AddWithValue("@TreatmentDate", TreatmentDate_picker.Value.Date);
                    treatmentCmd.Parameters.AddWithValue("@Cost", double.Parse(TreatmentCost_txtBox.Text));
                    treatmentCmd.Parameters.AddWithValue("@DepartmentName", TreatmentDepartment_cmpBox.Text);
                    treatmentCmd.Parameters.AddWithValue("@TreatmentType", TreatmentType_cmpBox.Text);

                    treatmentCmd.ExecuteNonQuery();

                    string updateDoctorLink = @"UPDATE TreatmentDoctors SET
            DoctorId = @DoctorId
            WHERE TreatmentId = @TreatmentId";

                    SqlCommand linkCmd = new SqlCommand(updateDoctorLink, con, tran);

                    linkCmd.Parameters.AddWithValue("@TreatmentId", int.Parse(TreatmentId_txtBox.Text));
                    linkCmd.Parameters.AddWithValue("@DoctorId", int.Parse(TreatmentDoctorId_txtBox.Text));

                    linkCmd.ExecuteNonQuery();

                    tran.Commit();

                    MessageBox.Show("تم تعديل المعالجة");
                    LoadTreatments();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("خطأ: " + ex.Message);
                }
            }
        }

        private void TreatmentsData_grdView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = TreatmentsData_grdView.Rows[e.RowIndex];

            TreatmentId_txtBox.Text = row.Cells["رقم المعالجة"].Value?.ToString();
            TreatmentPatientId_txtBox.Text = row.Cells["رقم المريض"].Value?.ToString();
            TreatmentDoctorId_txtBox.Text = row.Cells["رقم الطبيب"].Value?.ToString();
            TreatmentCost_txtBox.Text = row.Cells["التكلفة"].Value?.ToString();
            TreatmentDepartment_cmpBox.Text = row.Cells["القسم"].Value?.ToString();
            TreatmentType_cmpBox.Text = row.Cells["نوع المعالجة"].Value?.ToString();
            TreatmentsNotes_txtBox.Text = row.Cells["الملاحظات"].Value?.ToString();

            if (row.Cells["التاريخ"].Value != DBNull.Value)
                TreatmentDate_picker.Value = Convert.ToDateTime(row.Cells["التاريخ"].Value);
        }

     
    }


    public static class Db
    {
        public static string connectionString =
           @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HospitalDB;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
