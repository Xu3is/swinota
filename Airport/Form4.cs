using System;
using System.Drawing;
using System.Windows.Forms;

namespace Airport
{
    public partial class Form4 : Form
    {
        public Form4(string name, string surname, string position, int? salaryDay, DateTime? vacationDate, decimal? salary)
        {
            InitializeComponent();
            InitializeFormControls(name, surname, position, salaryDay, vacationDate, salary);
        }

        private void InitializeFormControls(string name, string surname, string position, int? salaryDay, DateTime? vacationDate, decimal? salary)
        {
            this.Text = "Рабочая панель сотрудника";
            this.Size = new Size(450, 450);
            this.BackColor = Color.Beige;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yOffset = 30;
            int labelWidth = 150;
            int valueWidth = 250;

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
            yOffset += 50;

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
            yOffset += 50;

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
            yOffset += 50;

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
            yOffset += 50;

            Label lblDaysToVacation = new Label
            {
                Text = "Дней до отпуска:",
                Location = new Point(50, yOffset),
                Size = new Size(labelWidth, 30),
                Font = new Font("Arial", 12)
            };
            Label lblDaysToVacationValue = new Label
            {
                Text = CalculateDaysToVacation(vacationDate),
                Location = new Point(200, yOffset),
                Size = new Size(valueWidth, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.SaddleBrown
            };
            yOffset += 50;

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(175, yOffset),
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
            this.Controls.Add(lblDaysToVacation);
            this.Controls.Add(lblDaysToVacationValue);
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

        private string CalculateDaysToVacation(DateTime? vacationDate)
        {
            if (!vacationDate.HasValue)
                return "Не указана";

            DateTime today = DateTime.Today;
            DateTime vacation = vacationDate.Value.Date;

            if (vacation < today)
                return "Отпуск прошел";

            int daysUntilVacation = (vacation - today).Days;
            return daysUntilVacation.ToString();
        }
    }
}