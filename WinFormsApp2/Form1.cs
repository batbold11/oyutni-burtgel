using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Forms;

// Анхаар: namespace-ийг өөрийн төслийн namespace-ээр соль
// (Form1.Designer.cs дотор байгаа нэр)
namespace WinFormsApp2
{
    public class Student
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Major { get; set; } = "";
        public string Year { get; set; } = "1";
        public string Phone { get; set; } = "";
    }

    public partial class Form1 : Form
    {
        static readonly string FilePath =
            Path.Combine(AppContext.BaseDirectory, "students.json");

        List<Student> students = new List<Student>();

        TextBox txtId, txtName, txtMajor, txtPhone, txtSearch;
        ComboBox cmbYear;
        DataGridView grid;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            LoadData();
            RefreshGrid();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // ---------- UI ----------
        void BuildUI()
        {
            Text = "Оюутны бүртгэл";
            ClientSize = new Size(830, 530);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(830, 400);

            txtId = AddField("Оюутны код", new TextBox(), 15, 15, 130);
            txtName = AddField("Овог нэр", new TextBox(), 160, 15, 200);
            txtMajor = AddField("Хөтөлбөр", new TextBox(), 375, 15, 200);

            cmbYear = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYear.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            cmbYear.SelectedIndex = 0;
            AddField("Курс", cmbYear, 590, 15, 80);

            txtPhone = AddField("Утас", new TextBox(), 685, 15, 130);

            AddButton("Нэмэх", 15, 75, Add_Click);
            AddButton("Засах", 125, 75, Edit_Click);
            AddButton("Устгах", 235, 75, Delete_Click);
            AddButton("Цэвэрлэх", 345, 75, (s, e) => ClearForm());

            Controls.Add(new Label { Text = "Хайх:", Location = new Point(475, 83), AutoSize = true });
            txtSearch = new TextBox { Location = new Point(525, 80), Width = 290 };
            txtSearch.TextChanged += (s, e) => RefreshGrid();
            Controls.Add(txtSearch);

            grid = new DataGridView
            {
                Location = new Point(15, 125),
                Size = new Size(800, 390),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.CellClick += Grid_CellClick;
            Controls.Add(grid);
        }

        T AddField<T>(string label, T control, int x, int y, int width) where T : Control
        {
            Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true });
            control.Location = new Point(x, y + 22);
            control.Width = width;
            Controls.Add(control);
            return control;
        }

        void AddButton(string text, int x, int y, EventHandler onClick)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(100, 32) };
            b.Click += onClick;
            Controls.Add(b);
        }

        // ---------- Товчны үйлдлүүд ----------
        void Add_Click(object sender, EventArgs e)
        {
            if (!CheckInput()) return;

            string id = txtId.Text.Trim();
            if (students.Any(s => s.Id == id))
            {
                MessageBox.Show("Энэ кодтой оюутан бүртгэлтэй байна.", "Анхааруулга");
                return;
            }

            students.Add(ReadForm());
            SaveData();
            RefreshGrid();
            ClearForm();
        }

        void Edit_Click(object sender, EventArgs e)
        {
            var selected = grid.CurrentRow?.DataBoundItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Засах оюутнаа хүснэгтээс сонгоно уу.", "Анхааруулга");
                return;
            }
            if (!CheckInput()) return;

            string id = txtId.Text.Trim();
            if (students.Any(s => s.Id == id && s != selected))
            {
                MessageBox.Show("Энэ кодтой өөр оюутан бүртгэлтэй байна.", "Анхааруулга");
                return;
            }

            var f = ReadForm();
            selected.Id = f.Id;
            selected.Name = f.Name;
            selected.Major = f.Major;
            selected.Year = f.Year;
            selected.Phone = f.Phone;

            SaveData();
            RefreshGrid();
            ClearForm();
        }

        void Delete_Click(object sender, EventArgs e)
        {
            var selected = grid.CurrentRow?.DataBoundItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Устгах оюутнаа хүснэгтээс сонгоно уу.", "Анхааруулга");
                return;
            }

            var answer = MessageBox.Show(
                $"\"{selected.Name}\"-г устгах уу?", "Баталгаажуулах",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            students.Remove(selected);
            SaveData();
            RefreshGrid();
            ClearForm();
        }

        void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var s = grid.Rows[e.RowIndex].DataBoundItem as Student;
            if (s == null) return;

            txtId.Text = s.Id;
            txtName.Text = s.Name;
            txtMajor.Text = s.Major;
            cmbYear.SelectedItem = s.Year;
            txtPhone.Text = s.Phone;
        }

        // ---------- Туслах функцууд ----------
        bool CheckInput()
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("Оюутны кодыг оруулна уу.", "Анхааруулга");
                txtId.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Овог нэрийг оруулна уу.", "Анхааруулга");
                txtName.Focus();
                return false;
            }
            return true;
        }

        Student ReadForm()
        {
            return new Student
            {
                Id = txtId.Text.Trim(),
                Name = txtName.Text.Trim(),
                Major = txtMajor.Text.Trim(),
                Year = cmbYear.SelectedItem?.ToString() ?? "1",
                Phone = txtPhone.Text.Trim()
            };
        }

        void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
            txtMajor.Clear();
            txtPhone.Clear();
            cmbYear.SelectedIndex = 0;
            grid.ClearSelection();
            txtId.Focus();
        }

        void RefreshGrid()
        {
            string q = txtSearch.Text.Trim().ToLower();
            var list = students
                .Where(s => q == "" ||
                            s.Id.ToLower().Contains(q) ||
                            s.Name.ToLower().Contains(q) ||
                            s.Major.ToLower().Contains(q))
                .ToList();

            grid.DataSource = list;

            if (grid.Columns.Count >= 5)
            {
                grid.Columns["Id"].HeaderText = "Код";
                grid.Columns["Name"].HeaderText = "Овог нэр";
                grid.Columns["Major"].HeaderText = "Хөтөлбөр";
                grid.Columns["Year"].HeaderText = "Курс";
                grid.Columns["Phone"].HeaderText = "Утас";
            }
        }

        // ---------- Файлд хадгалах ----------
        void LoadData()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    students = JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Файл уншихад алдаа гарлаа: " + ex.Message);
                students = new List<Student>();
            }
        }

        void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                File.WriteAllText(FilePath, JsonSerializer.Serialize(students, options));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Файл хадгалахад алдаа гарлаа: " + ex.Message);
            }
        }
    }
}