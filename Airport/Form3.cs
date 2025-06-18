using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Airport
{
    public partial class Form3 : Form
    {
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=ПОРТ;Integrated Security=True";

        public Form3()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.Text = "Добро пожаловать";
            this.Size = new Size(600, 400);
            this.BackColor = Color.Beige;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblWelcome = new Label
            {
                Text = "Лучшие билеты из Санкт-Петербурга!",
                Location = new Point(150, 50),
                Size = new Size(300, 60),
                Font = new Font("Arial", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.SaddleBrown
            };

            Button btnBuyTicket = new Button
            {
                Text = "Купить билет",
                Location = new Point(150, 150),
                Size = new Size(300, 60),
                Font = new Font("Arial", 16, FontStyle.Bold),
                BackColor = Color.PeachPuff,
                ForeColor = Color.DarkSlateGray
            };
            btnBuyTicket.Click += (s, e) =>
            {
                Form2 form2 = new Form2();
                form2.ShowDialog();
            };

            Button btnEmployeeLogin = new Button
            {
                Text = "Вход для сотрудников",
                Location = new Point(150, 230),
                Size = new Size(300, 60),
                Font = new Font("Arial", 16, FontStyle.Bold),
                BackColor = Color.PeachPuff,
                ForeColor = Color.DarkSlateGray
            };
            btnEmployeeLogin.Click += (s, e) =>
            {
                LoginForm loginForm = new LoginForm(connectionString);
                loginForm.ShowDialog();
            };

            this.Controls.Add(lblWelcome);
            this.Controls.Add(btnBuyTicket);
            this.Controls.Add(btnEmployeeLogin);
        }
    }

    public class LoginForm : Form
    {
        private string connectionString;

        public LoginForm(string connectionString)
        {
            this.connectionString = connectionString;
            InitializeLoginControls();
        }

        private void InitializeLoginControls()
        {
            this.Text = "Вход для сотрудников";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblName = new Label
            {
                Text = "Имя:",
                Location = new Point(50, 30),
                Size = new Size(100, 30),
                Font = new Font("Arial", 12)
            };

            TextBox txtName = new TextBox
            {
                Location = new Point(150, 30),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12)
            };

            Label lblSurname = new Label
            {
                Text = "Фамилия:",
                Location = new Point(50, 80),
                Size = new Size(100, 30),
                Font = new Font("Arial", 12)
            };

            TextBox txtSurname = new TextBox
            {
                Location = new Point(150, 80),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12)
            };

            Label lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 130),
                Size = new Size(100, 30),
                Font = new Font("Arial", 12)
            };

            TextBox txtPassword = new TextBox
            {
                Location = new Point(150, 130),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12),
                UseSystemPasswordChar = true
            };

            Button btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(150, 180),
                Size = new Size(100, 40),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            btnLogin.Click += (s, e) =>
            {
                string name = txtName.Text.Trim();
                string surname = txtSurname.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "SELECT Должность, День_выплаты_зарплаты, Дата_отпуска, Зарплата, Уровень_доступа FROM Сотрудники WHERE Имя = @Name AND Фамилия = @Surname AND Пароль = @Password";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Surname", surname);
                            command.Parameters.AddWithValue("@Password", password);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string position = reader.IsDBNull(0) ? null : reader.GetString(0);
                                    int? salaryDay = reader.IsDBNull(1) ? null : (int?)reader.GetInt32(1);
                                    DateTime? vacationDate = reader.IsDBNull(2) ? null : (DateTime?)reader.GetDateTime(2);
                                    decimal? salary = reader.IsDBNull(3) ? null : (decimal?)reader.GetDecimal(3);
                                    int accessLevel = reader.GetInt32(4); // Получаем уровень доступа

                                    MessageBox.Show($"Добро пожаловать, {name} {surname}!", "Приветствие", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Close();

                                    // Проверка только для админа (уровень доступа 0)
                                    if (accessLevel == 0)
                                    {
                                        Form1 adminForm = new Form1();
                                        adminForm.ShowDialog();
                                    }
                                    else
                                    {
                                        EmployeeForm employeeForm = new EmployeeForm(name, surname, position, salaryDay, vacationDate, salary);
                                        employeeForm.ShowDialog();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Неверные данные для входа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblSurname);
            this.Controls.Add(txtSurname);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }
    }

    public class EmployeeForm : Form
    {
        public EmployeeForm(string name, string surname, string position, int? salaryDay, DateTime? vacationDate, decimal? salary)
        {
            InitializeEmployeeControls(name, surname, position, salaryDay, vacationDate, salary);
        }

        private void InitializeEmployeeControls(string name, string surname, string position, int? salaryDay, DateTime? vacationDate, decimal? salary)
        {
            this.Text = "Информация о сотруднике";
            this.Size = new Size(400, 360);
            this.BackColor = Color.Beige;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yOffset = 30;
            int labelWidth = 150;
            int valueWidth = 200;

            Label lblFullName = new Label
            {
                Text = "Имя и фамилия:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblFullNameValue = new Label
            {
                Text = $"{name} {surname}",
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 40;

            Label lblPosition = new Label
            {
                Text = "Должность:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblPositionValue = new Label
            {
                Text = position ?? "Не указана",
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 40;

            Label lblDaysToSalary = new Label
            {
                Text = "Дней до зарплаты:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblDaysToSalaryValue = new Label
            {
                Text = CalculateDaysToSalary(salaryDay),
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 40;

            Label lblSalary = new Label
            {
                Text = "Зарплата:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblSalaryValue = new Label
            {
                Text = salary.HasValue ? $"{salary.Value:F2} руб." : "Не указана",
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 40;

            Label lblVacationDate = new Label
            {
                Text = "Дата отпуска:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblVacationDateValue = new Label
            {
                Text = vacationDate.HasValue ? vacationDate.Value.ToString("dd.MM.yyyy") : "Не указана",
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 40;

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(150, yOffset),
                Size = new Size(100, 40),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.PeachPuff,
                ForeColor = Color.DarkSlateGray
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lblFullName);
            this.Controls.Add(lblFullNameValue);
            this.Controls.Add(lblPosition);
            this.Controls.Add(lblPositionValue);
            this.Controls.Add(lblDaysToSalary);
            this.Controls.Add(lblDaysToSalaryValue);
            this.Controls.Add(lblSalary);
            this.Controls.Add(lblSalaryValue);
            this.Controls.Add(lblVacationDate);
            this.Controls.Add(lblVacationDateValue);
            this.Controls.Add(btnClose);
        }

        private string CalculateDaysToSalary(int? salaryDay)
        {
            if (!salaryDay.HasValue)
                return "Не указан";

            int day = salaryDay.Value;
            if (day < 1 || day > 31)
                return "Некорректный день";

            DateTime today = DateTime.Today;
            DateTime nextSalaryDate;

            if (today.Day <= day)
            {
                nextSalaryDate = new DateTime(today.Year, today.Month, Math.Min(day, DateTime.DaysInMonth(today.Year, today.Month)));
            }
            else
            {
                DateTime nextMonth = today.AddMonths(1);
                nextSalaryDate = new DateTime(nextMonth.Year, nextMonth.Month, Math.Min(day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));
            }

            int daysUntilSalary = (nextSalaryDate - today).Days;
            return daysUntilSalary.ToString();
        }
    }
}