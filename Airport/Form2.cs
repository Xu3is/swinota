using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Airport
{
    public partial class Form2 : Form
    {
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=ПОРТ;Integrated Security=True";
        private DataGridView dataGridViewTickets;
        private Button btnRegister;
        private Random random = new Random();
        private int passengerCodeCounter = 1;

        public Form2()
        {
            InitializeComponent();
            InitializeControls();
            LoadTickets();
        }

        private void InitializeControls()
        {
            this.Text = "Все рейсы из Санкт-Петербурга";
            this.Size = new Size(1500, 850);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Initialize DataGridView
            dataGridViewTickets = new DataGridView
            {
                Location = new Point(12, 12),
                Size = new Size(1450, 700),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Arial", 18, FontStyle.Bold),
                    BackColor = Color.LightGray
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Arial", 15)
                }
            };
            // Установка заголовков столбцов
            dataGridViewTickets.Columns.Add("City", "Город назначения");
            dataGridViewTickets.Columns.Add("DepartureDate", "Дата вылета");
            dataGridViewTickets.Columns.Add("ArrivalDate", "Дата прилета");
            dataGridViewTickets.Columns.Add("FlightNumber", "Номер рейса");

            // Initialize Register Button
            btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(12, 720),
                Size = new Size(300, 50),
                Font = new Font("Arial", 16, FontStyle.Bold)
            };
            btnRegister.Click += BtnRegister_Click;

            // Add controls to form
            this.Controls.Add(dataGridViewTickets);
            this.Controls.Add(btnRegister);
        }

        private void LoadTickets()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            Р.Город_Прилета AS [Город назначения],
                            Р.Время_Вылета AS [Дата вылета],
                            Р.Время_Прилета AS [Дата прилета],
                            Р.Номер_Рейса AS [Номер рейса]
                        FROM Рейсы Р
                        WHERE Р.Город_Вылета = N'Санкт-Петербург'";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable ticketsTable = new DataTable();
                        adapter.Fill(ticketsTable);
                        dataGridViewTickets.Rows.Clear();
                        foreach (DataRow row in ticketsTable.Rows)
                        {
                            dataGridViewTickets.Rows.Add(
                                row["Город назначения"],
                                row["Дата вылета"],
                                row["Дата прилета"],
                                row["Номер рейса"]
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            Form registerForm = new Form
            {
                Text = "Регистрация",
                Size = new Size(400, 350),
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblName = new Label { Text = "Имя:", Location = new Point(20, 20), Font = new Font("Arial", 14) };
            TextBox txtName = new TextBox { Location = new Point(150, 20), Width = 200, Font = new Font("Arial", 14) };
            Label lblFamily = new Label { Text = "Фамилия:", Location = new Point(20, 60), Font = new Font("Arial", 14) };
            TextBox txtFamily = new TextBox { Location = new Point(150, 60), Width = 200, Font = new Font("Arial", 14) };
            Label lblPassport = new Label { Text = "Паспорт:", Location = new Point(20, 100), Font = new Font("Arial", 14) };
            TextBox txtPassport = new TextBox { Location = new Point(150, 100), Width = 200, Font = new Font("Arial", 14) };
            Label lblEmail = new Label { Text = "Почта:", Location = new Point(20, 140), Font = new Font("Arial", 14) };
            TextBox txtEmail = new TextBox { Location = new Point(150, 140), Width = 200, Font = new Font("Arial", 14) };
            Button btnSubmit = new Button { Text = "Подтвердить", Location = new Point(150, 200), Width = 200, Height=100, Font = new Font("Arial", 16, FontStyle.Bold) };

            btnSubmit.Click += (s, ev) =>
            {

                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(txtEmail.Text, emailPattern))
                {
                    MessageBox.Show("Неверный формат электронной почты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string seatNumber = $"{random.Next(10, 20)}{Convert.ToChar(random.Next(65, 91))}"; // Случайное место, например "12A", "15B"
                string flightNumber = dataGridViewTickets.Rows.Count > 0 ? dataGridViewTickets.Rows[0].Cells["FlightNumber"].Value.ToString() : "N/A";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Получение Код_Рейса по Номер_Рейса
                        int flightCode = GetFlightCode(flightNumber, connection);

                        // Вставка в таблицу Пассажиры
                        string passengerInsertQuery = @"
                            INSERT INTO Пассажиры (Имя, Фамилия, Номер_паспорта, Электронная_Почта)
                            VALUES (@Name, @Family, @Passport, @Email)";
                        using (SqlCommand cmdPassenger = new SqlCommand(passengerInsertQuery, connection))
                        {
                            cmdPassenger.Parameters.AddWithValue("@Name", txtName.Text);
                            cmdPassenger.Parameters.AddWithValue("@Family", txtFamily.Text);
                            cmdPassenger.Parameters.AddWithValue("@Passport", txtPassport.Text);
                            cmdPassenger.Parameters.AddWithValue("@Email", txtEmail.Text);
                            cmdPassenger.ExecuteNonQuery();
                        }

                        // Получение последнего вставленного Код_Пассажира
                        int passengerCode = GetLastPassengerCode(connection);

                        // Вставка в таблицу Билеты
                        string ticketInsertQuery = @"
                            INSERT INTO Билеты (Код_Рейса, Код_Пассажира, Номер_Места, Цена)
                            VALUES (@FlightCode, @PassengerCode, @Seat, 7500.00)"; // Цена задана как фиксированная для примера
                        using (SqlCommand cmdTicket = new SqlCommand(ticketInsertQuery, connection))
                        {
                            cmdTicket.Parameters.AddWithValue("@FlightCode", flightCode);
                            cmdTicket.Parameters.AddWithValue("@PassengerCode", passengerCode);
                            cmdTicket.Parameters.AddWithValue("@Seat", seatNumber);
                            cmdTicket.ExecuteNonQuery();
                        }

                        MessageBox.Show($"Вы зарегистрированы на рейс. Ваш номер места: {seatNumber}\nМы вышлем ваш посадочный талон на почту.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        registerForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            registerForm.Controls.Add(lblName);
            registerForm.Controls.Add(txtName);
            registerForm.Controls.Add(lblFamily);
            registerForm.Controls.Add(txtFamily);
            registerForm.Controls.Add(lblPassport);
            registerForm.Controls.Add(txtPassport);
            registerForm.Controls.Add(lblEmail);
            registerForm.Controls.Add(txtEmail);
            registerForm.Controls.Add(btnSubmit);
            registerForm.ShowDialog(this);
        }

        private int GetFlightCode(string flightNumber, SqlConnection connection)
        {
            string query = "SELECT Код_Рейса FROM Рейсы WHERE Номер_Рейса = @FlightNumber";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@FlightNumber", flightNumber);
                object result = cmd.ExecuteScalar();
                return result != null ? (int)result : -1;
            }
        }

        private int GetLastPassengerCode(SqlConnection connection)
        {
            string query = "SELECT MAX(Код_Пассажира) FROM Пассажиры";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                object result = cmd.ExecuteScalar();
                return result != null ? (int)result : 0;
            }
        }
    }
}