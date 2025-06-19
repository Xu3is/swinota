using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace Airport
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=DESKTOP-KDS9A4G;Initial Catalog=ПОРТ;Integrated Security=True";
        private DataGridView dataGridView;
        private Button[] categoryButtons;
        private Button[] tableButtons;
        private Panel tableButtonsPanel;
        private Button deleteButton;
        private Button filterButton;
        private Button reportButton;
        private Button saveButton;
        private Panel actionButtonsPanel;
        private string currentTable; // To track the currently displayed table

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.Text = "Просмотр базы данных Аэропорт";
            this.Size = new Size(1500, 850);

            // Инициализация кнопок категорий
            string[] categories = {
                "Аэропорты и инфраструктура",
                "Авиакомпании и флот",
                "Рейсы и билеты",
                "Персонал и обслуживание"
            };
            categoryButtons = new Button[categories.Length];
            int buttonWidth = (this.ClientSize.Width - 20) / categories.Length;
            int buttonX = 10;
            for (int i = 0; i < categories.Length; i++)
            {
                categoryButtons[i] = new Button
                {
                    Text = categories[i],
                    Location = new Point(buttonX, 10),
                    Size = new Size(buttonWidth - 5, 50),
                    Tag = categories[i],
                    Font = new Font("Arial", 14, FontStyle.Bold)
                };
                categoryButtons[i].Click += CategoryButton_Click;
                this.Controls.Add(categoryButtons[i]);
                buttonX += buttonWidth;
            }

            // Панель для кнопок таблиц
            tableButtonsPanel = new Panel
            {
                Location = new Point(10, 70),
                Size = new Size(1460, 60),
                AutoScroll = true
            };
            this.Controls.Add(tableButtonsPanel);

            // Инициализация кнопок для таблиц
            string[] tableNames = {
                "Аэропорты", "Терминалы", "Выходы",
                "Авиакомпании", "Самолёты",
                "Рейсы", "Билеты",
                "Сотрудники", "Пассажиры", "Обслуживание_рейсов"
            };
            tableButtons = new Button[tableNames.Length];
            for (int i = 0; i < tableNames.Length; i++)
            {
                tableButtons[i] = new Button
                {
                    Text = tableNames[i],
                    Size = new Size(220, 50),
                    Tag = tableNames[i],
                    Visible = false,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                tableButtons[i].Click += TableButton_Click;
                tableButtonsPanel.Controls.Add(tableButtons[i]);
            }

            // Инициализация DataGridView
            dataGridView = new DataGridView
            {
                Location = new Point(10, 140),
                Size = new Size(1460, 580),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = true,
                ReadOnly = false,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Arial", 14),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 40
            };
            this.Controls.Add(dataGridView);

            // Панель для кнопок действий
            actionButtonsPanel = new Panel
            {
                Location = new Point(10, 730),
                Size = new Size(1460, 60)
            };
            this.Controls.Add(actionButtonsPanel);

            // Инициализация кнопок действий
            buttonX = 10;
            deleteButton = new Button
            {
                Text = "Удалить запись",
                Location = new Point(buttonX, 10),
                Size = new Size(180, 50),
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            deleteButton.Click += DeleteButton_Click;
            actionButtonsPanel.Controls.Add(deleteButton);
            buttonX += 190;

            saveButton = new Button
            {
                Text = "Сохранить",
                Location = new Point(buttonX, 10),
                Size = new Size(180, 50),
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            saveButton.Click += SaveButton_Click;
            actionButtonsPanel.Controls.Add(saveButton);
            buttonX += 190;

            buttonX = actionButtonsPanel.Width - (140 + 180 + 20);
            filterButton = new Button
            {
                Text = "Фильтр",
                Location = new Point(buttonX, 10),
                Size = new Size(140, 50),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.Orange
            };
            filterButton.Click += FilterButton_Click;
            actionButtonsPanel.Controls.Add(filterButton);
            buttonX += 150;

            reportButton = new Button
            {
                Text = "Создать отчет",
                Location = new Point(buttonX, 10),
                Size = new Size(180, 50),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.Green
            };
            reportButton.Click += ReportButton_Click;
            actionButtonsPanel.Controls.Add(reportButton);

            ShowTablesForCategory("Аэропорты и инфраструктура");
            LoadTableData("Аэропорты");
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string category = button.Tag.ToString();
                ShowTablesForCategory(category);
                string[] tables = GetTablesForCategory(category);
                if (tables.Length > 0)
                {
                    LoadTableData(tables[0]);
                }
            }
        }

        private void ShowTablesForCategory(string category)
        {
            foreach (var button in tableButtons)
            {
                button.Visible = false;
            }

            string[] tables = GetTablesForCategory(category);
            int visibleButtonCount = tables.Length;

            if (visibleButtonCount == 0) return;

            int panelWidth = tableButtonsPanel.Width;
            int buttonWidth = 220;
            int spacing = 10;
            int totalWidth = visibleButtonCount * buttonWidth + (visibleButtonCount - 1) * spacing;
            int startX = (panelWidth - totalWidth) / 2;

            int buttonX = startX;
            foreach (var table in tables)
            {
                var button = tableButtons.FirstOrDefault(b => b.Tag.ToString() == table);
                if (button != null)
                {
                    button.Location = new Point(buttonX, 5);
                    button.Visible = true;
                    buttonX += buttonWidth + spacing;
                }
            }
        }

        private string[] GetTablesForCategory(string category)
        {
            switch (category)
            {
                case "Аэропорты и инфраструктура":
                    return new[] { "Аэропорты", "Терминалы", "Выходы" };
                case "Авиакомпании и флот":
                    return new[] { "Авиакомпании", "Самолёты" };
                case "Рейсы и билеты":
                    return new[] { "Рейсы", "Билеты" };
                case "Персонал и обслуживание":
                    return new[] { "Сотрудники", "Пассажиры", "Обслуживание_рейсов" };
                default:
                    return Array.Empty<string>();
            }
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string tableName = button.Tag.ToString();
                Console.WriteLine($"Нажата кнопка для таблицы: {tableName} at {DateTime.Now}");
                LoadTableData(tableName);
            }
        }

        private void LoadTableData(string tableName)
        {
            Console.WriteLine($"Попытка загрузки таблицы: {tableName} at {DateTime.Now}");
            string[] validTables = { "Аэропорты", "Терминалы", "Выходы", "Авиакомпании", "Самолёты", "Рейсы", "Билеты", "Пассажиры", "Сотрудники", "Обслуживание_рейсов" };
            if (!validTables.Contains(tableName))
            {
                MessageBox.Show($"Недопустимое имя таблицы: '{tableName}'. Доступные таблицы: {string.Join(", ", validTables)}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM [{tableName}]";
                    Console.WriteLine($"Выполняется запрос: {query}");
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count == 0)
                        {
                            MessageBox.Show($"Таблица '{tableName}' пуста.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        dataGridView.DataSource = dataTable;
                        currentTable = tableName;
                    }
                }
                this.Text = $"Просмотр базы данных AirportDB - Таблица: {tableName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridView.DataSource = null;
                currentTable = null;
                Console.WriteLine($"Исключение: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, сначала выделите запись для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView.DataSource is DataTable dataTable && !string.IsNullOrEmpty(currentTable))
            {
                try
                {
                    DataRowView rowView = (DataRowView)dataGridView.SelectedRows[0].DataBoundItem;
                    if (rowView != null)
                    {
                        DataRow row = rowView.Row;
                        if (row != null)
                        {
                            string primaryKeyColumn = dataTable.Columns[0].ColumnName;
                            object primaryKeyValue = row[primaryKeyColumn];

                            DeleteRelatedRecords(currentTable, primaryKeyColumn, primaryKeyValue);
                            row.Delete();

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                string query = $"SELECT * FROM [{currentTable}]";
                                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                                {
                                    using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))
                                    {
                                        adapter.Update(dataTable);
                                    }
                                }
                            }
                            MessageBox.Show($"Запись удалена.\nВозможные связи таблицы '{currentTable}' уже обработаны.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (dataTable != null) dataTable.RejectChanges();
                }
            }
            else
            {
                MessageBox.Show("Данные таблицы не загружены или произошла ошибка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRelatedRecords(string tableName, string primaryKeyColumn, object primaryKeyValue)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                switch (tableName)
                {
                    case "Аэропорты":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Терминалы] WHERE [Код_Аэропорта] = @Key", primaryKeyValue);
                        ExecuteDeleteQuery(connection, "DELETE FROM [Выходы] WHERE [Код_Аэропорта] = @Key", primaryKeyValue);
                        break;
                    case "Терминалы":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Выходы] WHERE [Код_Терминала] = @Key", primaryKeyValue);
                        break;
                    case "Авиакомпании":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Самолёты] WHERE [Код_Авиакомпании] = @Key", primaryKeyValue);
                        break;
                    case "Рейсы":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Билеты] WHERE [Код_Рейса] = @Key", primaryKeyValue);
                        ExecuteDeleteQuery(connection, "DELETE FROM [Обслуживание_рейсов] WHERE [Код_Рейса] = @Key", primaryKeyValue);
                        break;
                    case "Сотрудники":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Обслуживание_рейсов] WHERE [Код_Сотрудника] = @Key", primaryKeyValue);
                        break;
                    case "Пассажиры":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Билеты] WHERE [Код_Пассажира] = @Key", primaryKeyValue);
                        break;
                }
                ExecuteDeleteQuery(connection, $"DELETE FROM [{tableName}] WHERE [{primaryKeyColumn}] = @Key", primaryKeyValue);
            }
        }

        private void ExecuteDeleteQuery(SqlConnection connection, string query, object keyValue = null)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (keyValue != null)
                {
                    command.Parameters.AddWithValue("@Key", keyValue);
                }
                command.ExecuteNonQuery();
            }
        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.DataSource is DataTable dataTable && !string.IsNullOrEmpty(currentTable))
            {
                FilterGrid(dataGridView, currentTable);
            }
            else
            {
                MessageBox.Show("Данные таблицы не загружены или произошла ошибка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterGrid(DataGridView grid, string tableName)
        {
            try
            {
                Form filterForm = new Form();
                filterForm.Text = $"Фильтрация таблицы {tableName}";
                filterForm.Size = new Size(300, 200);
                filterForm.StartPosition = FormStartPosition.CenterParent;

                Label columnLabel = new Label { Text = "Выберите столбец:", Location = new Point(10, 20), AutoSize = true };
                filterForm.Controls.Add(columnLabel);

                ComboBox columnComboBox = new ComboBox { Location = new Point(10, 40), Size = new Size(260, 20) };
                if (grid.DataSource is DataTable dataTable)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        columnComboBox.Items.Add(column.ColumnName);
                    }
                }
                columnComboBox.SelectedIndex = 0;
                filterForm.Controls.Add(columnComboBox);

                Label valueLabel = new Label { Text = "Введите значение:", Location = new Point(10, 70), AutoSize = true };
                filterForm.Controls.Add(valueLabel);

                TextBox valueTextBox = new TextBox { Location = new Point(10, 90), Size = new Size(260, 20) };
                filterForm.Controls.Add(valueTextBox);

                Button confirmButton = new Button
                {
                    Text = "Применить",
                    Font = new Font("Arial", 12),
                    Location = new Point(10, 120),
                    Size = new Size(100, 40)
                };
                confirmButton.Click += (s, e) =>
                {
                    if (grid.DataSource is DataTable filteredDataTable)
                    {
                        string selectedColumn = columnComboBox.SelectedItem.ToString();
                        string filterValue = valueTextBox.Text.Trim();

                        DataView dataView = filteredDataTable.DefaultView;
                        if (string.IsNullOrEmpty(filterValue))
                        {
                            dataView.RowFilter = "";
                        }
                        else
                        {
                            if (filteredDataTable.Columns[selectedColumn].DataType == typeof(string))
                            {
                                dataView.RowFilter = $"{selectedColumn} LIKE '%{filterValue}%'";
                            }
                            else
                            {
                                dataView.RowFilter = $"{selectedColumn} = '{filterValue}'";
                            }
                        }
                        grid.DataSource = dataView;
                    }
                    filterForm.Close();
                };
                filterForm.Controls.Add(confirmButton);

                filterForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReportButton_Click(object sender, EventArgs e)
        {
            DataSet dataSet = new DataSet();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string[] tableNames = {
                        "Аэропорты", "Терминалы", "Выходы",
                        "Авиакомпании", "Самолёты",
                        "Рейсы", "Билеты",
                        "Сотрудники", "Пассажиры", "Обслуживание_рейсов"
                    };
                    foreach (string tableName in tableNames)
                    {
                        string query = $"SELECT * FROM [{tableName}]";
                        using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                        {
                            DataTable table = new DataTable(tableName);
                            adapter.Fill(table);
                            dataSet.Tables.Add(table);
                        }
                    }
                }

                ExportToExcel(dataSet, currentTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel(DataSet dataSet, string currentTable)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workBook = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = true;
                workBook = excelApp.Workbooks.Add();

                Excel.Worksheet workSheetAdditional = (Excel.Worksheet)workBook.Sheets[1];
                workSheetAdditional.Name = "Дополнительно";

                int sheetIndex = 2;
                foreach (DataTable table in dataSet.Tables)
                {
                    Excel.Worksheet workSheet = (Excel.Worksheet)workBook.Sheets.Add();
                    workSheet.Name = table.TableName.Length > 31 ? table.TableName.Substring(0, 31) : table.TableName;

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        workSheet.Cells[1, i + 1] = table.Columns[i].ColumnName;
                    }

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            workSheet.Cells[i + 2, j + 1] = table.Rows[i][j]?.ToString();
                        }
                    }

                    workSheet.Columns.AutoFit();
                    workSheet.Rows.AutoFit();

                    sheetIndex++;
                }

                if (!string.IsNullOrEmpty(currentTable))
                {
                    DataTable currentDataTable = dataSet.Tables[currentTable];
                    if (currentDataTable != null)
                    {
                        CreateChart(workSheetAdditional, currentDataTable, currentTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (workBook != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workBook);
                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }
            }
        }

        private void CreateChart(Excel.Worksheet workSheet, DataTable dataTable, string tableName)
        {
            try
            {
                string categoryColumn = "";
                string valueColumn = "";
                string chartTitle = "";
                bool useCount = true;

                switch (tableName)
                {
                    case "Аэропорты":
                        categoryColumn = "Название_аэропорта";
                        valueColumn = null;
                        chartTitle = "Количество терминалов по аэропортам";
                        break;
                    case "Терминалы":
                        categoryColumn = "Название_терминала";
                        valueColumn = null;
                        chartTitle = "Количество выходов по терминалам";
                        break;
                    case "Выходы":
                        categoryColumn = "Номер_выхода";
                        valueColumn = null;
                        chartTitle = "Количество выходов по терминалам";
                        break;
                    case "Авиакомпании":
                        categoryColumn = "Название_авиакомпании";
                        valueColumn = null;
                        chartTitle = "Количество самолетов по авиакомпаниям";
                        break;
                    case "Самолёты":
                        categoryColumn = "Вместимость";
                        valueColumn = null;
                        useCount = false;
                        chartTitle = "Распределение самолетов по вместимости";
                        break;
                    case "Рейсы":
                        categoryColumn = "Номер_Рейса";
                        valueColumn = null;
                        chartTitle = "Количество рейсов";
                        break;
                    case "Билеты":
                        categoryColumn = "Номер_Места";
                        valueColumn = "Цена";
                        useCount = false;
                        chartTitle = "Распределение цен билетов";
                        break;
                    case "Сотрудники":
                        categoryColumn = "Должность";
                        valueColumn = "Зарплата";
                        useCount = false;
                        chartTitle = "Зарплаты по должностям";
                        break;
                    case "Пассажиры":
                        categoryColumn = "Имя";
                        valueColumn = null;
                        chartTitle = "Количество пассажиров по именам";
                        break;
                    case "Обслуживание_рейсов":
                        categoryColumn = "Код_Рейса";
                        valueColumn = null;
                        chartTitle = "Количество обслуживаний по рейсам";
                        break;
                    default:
                        return;
                }

                if (!dataTable.Columns.Contains(categoryColumn))
                {
                    MessageBox.Show($"Столбец '{categoryColumn}' не найден в таблице '{tableName}'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var groupedData = dataTable.AsEnumerable()
                    .GroupBy(r => r.Field<object>(categoryColumn)?.ToString())
                    .Select(g => new
                    {
                        Category = g.Key,
                        Value = useCount
                            ? (double)g.Count()
                            : g.Sum(row => row.Field<object>(valueColumn) != null ? Convert.ToDouble(row.Field<object>(valueColumn)) : 0)
                    })
                    .OrderBy(g => g.Category)
                    .ToList();

                if (groupedData.Count == 0)
                {
                    MessageBox.Show($"Нет данных для построения диаграммы для таблицы '{tableName}'.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                workSheet.Cells[1, 1] = "Категория";
                workSheet.Cells[1, 2] = useCount ? "Количество" : valueColumn;
                int excelRow = 2;
                foreach (var item in groupedData)
                {
                    workSheet.Cells[excelRow, 1] = item.Category ?? "Не указано";
                    workSheet.Cells[excelRow, 2] = item.Value;
                    excelRow++;
                }

                Excel.Range chartRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[excelRow - 1, 2]];

                Excel.ChartObjects chartObjects = (Excel.ChartObjects)workSheet.ChartObjects();
                Excel.ChartObject chartObject = chartObjects.Add(100, 100, 600, 400);
                Excel.Chart chart = chartObject.Chart;
                chart.SetSourceData(chartRange);
                chart.ChartType = Excel.XlChartType.xlColumnClustered;
                chart.HasTitle = true;
                chart.ChartTitle.Text = chartTitle;
                chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = categoryColumn;
                chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = useCount ? "Количество" : valueColumn;

                chart.Legend.Delete();
                chartRange.Columns.AutoFit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании диаграммы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        private void SaveChanges()
        {
            try
            {
                if (dataGridView.DataSource is DataTable dataTable && !string.IsNullOrEmpty(currentTable))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = $"SELECT * FROM [{currentTable}]";
                        using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                        {
                            using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))
                            {
                                adapter.Update(dataTable);
                            }
                        }
                        MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Данные таблицы не загружены или произошла ошибка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SaveChanges();
        }
    }
}