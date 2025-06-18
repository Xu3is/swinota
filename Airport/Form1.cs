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
        private string connectionString = @"Data Source=ADCLG1;Initial Catalog=ПОРТ;Integrated Security=True";
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
            this.Size = new Size(1500, 850); // Увеличен размер формы для свободного места внизу

            // Инициализация кнопок категорий
            string[] categories = {
                "Аэропорты и инфраструктура",
                "Авиакомпании и флот",
                "Рейсы и билеты",
                "Персонал и обслуживание"
            };
            categoryButtons = new Button[categories.Length];
            int buttonWidth = (this.ClientSize.Width - 20) / categories.Length; // Равномерное распределение по ширине
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
                    Size = new Size(220, 50), // Увеличена ширина до 220 пикселей
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
                AllowUserToAddRows = true, // Разрешено добавление строк
                ReadOnly = false, // Включено редактирование
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2, // Редактирование по клавише или F2
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Arial", 14), // Шрифт для ячеек
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 40 // Увеличенная высота заголовков
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

            // Размещаем кнопки "Фильтр" и "Создать отчет" справа
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

            // Показать таблицы первой категории по умолчанию
            ShowTablesForCategory("Аэропорты и инфраструктура");
            // Изменено для явного вызова таблицы Аэропорты при загрузке
            LoadTableData("Аэропорты");
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string category = button.Tag.ToString();
                ShowTablesForCategory(category);
                // Загружаем первую таблицу категории при выборе
                string[] tables = GetTablesForCategory(category);
                if (tables.Length > 0)
                {
                    LoadTableData(tables[0]);
                }
            }
        }

        private void ShowTablesForCategory(string category)
        {
            // Скрыть все кнопки таблиц
            foreach (var button in tableButtons)
            {
                button.Visible = false;
            }

            // Определение таблиц для каждой категории
            string[] tables = GetTablesForCategory(category);
            int visibleButtonCount = tables.Length;

            if (visibleButtonCount == 0) return; // Если нет таблиц, выходим

            // Параметры панели и кнопок
            int panelWidth = tableButtonsPanel.Width; // Ширина панели (1460 пикселей)
            int buttonWidth = 220; // Ширина каждой кнопки
            int spacing = 10; // Небольшое расстояние между кнопками
            int totalWidth = visibleButtonCount * buttonWidth + (visibleButtonCount - 1) * spacing; // Общая ширина с учетом промежутков
            int startX = (panelWidth - totalWidth) / 2; // Центрируем кнопки

            // Показать кнопки соответствующих таблиц
            int buttonX = startX;
            foreach (var table in tables)
            {
                var button = tableButtons.FirstOrDefault(b => b.Tag.ToString() == table);
                if (button != null)
                {
                    button.Location = new Point(buttonX, 5);
                    button.Visible = true;
                    buttonX += buttonWidth + spacing; // Следующая кнопка с учетом шага
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
                LoadTableData(tableName); // Явно загружаем данные для выбранной таблицы
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
                    // Получаем выделенную строку
                    DataRowView rowView = (DataRowView)dataGridView.SelectedRows[0].DataBoundItem;
                    if (rowView != null)
                    {
                        DataRow row = rowView.Row;
                        if (row != null)
                        {
                            // Предполагаем, что первичный ключ — первая колонка (нужно уточнить структуру)
                            string primaryKeyColumn = dataTable.Columns[0].ColumnName;
                            object primaryKeyValue = row[primaryKeyColumn];

                            // Рекурсивное удаление связанных записей
                            DeleteRelatedRecords(currentTable, primaryKeyColumn, primaryKeyValue);

                            // Удаляем строку из текущей таблицы
                            row.Delete();

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                string query = $"SELECT * FROM [{currentTable}]";
                                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                                {
                                    using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))
                                    {
                                        adapter.Update(dataTable); // Выполняем удаление в базе данных
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
                    if (dataTable != null) dataTable.RejectChanges(); // Откат изменений при ошибке
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
                // Определяем связанные таблицы на основе предположений
                switch (tableName)
                {
                    case "Аэропорты":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Терминалы] WHERE [ID_Аэропорта] = @Key", primaryKeyValue);
                        ExecuteDeleteQuery(connection, "DELETE FROM [Выходы] WHERE [ID_Аэропорта] = @Key", primaryKeyValue);
                        break;
                    case "Терминалы":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Выходы] WHERE [ID_Терминала] = @Key", primaryKeyValue);
                        break;
                    case "Авиакомпании":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Самолёты] WHERE [ID_Авиакомпании] = @Key", primaryKeyValue);
                        break;
                    case "Рейсы":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Билеты] WHERE [ID_Рейса] = @Key", primaryKeyValue);
                        ExecuteDeleteQuery(connection, "DELETE FROM [Обслуживание_рейсов] WHERE [ID_Рейса] = @Key", primaryKeyValue);
                        break;
                    case "Сотрудники":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Обслуживание_рейсов] WHERE [ID_Сотрудника] = @Key", primaryKeyValue);
                        break;
                    case "Пассажиры":
                        ExecuteDeleteQuery(connection, "DELETE FROM [Билеты] WHERE [ID_Пассажира] = @Key", primaryKeyValue);
                        break;
                }

                // Удаляем из текущей таблицы (если есть зависимости, они уже обработаны выше)
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

                Label columnLabel = new Label();
                columnLabel.Text = "Выберите столбец:";
                columnLabel.Location = new Point(10, 20);
                columnLabel.AutoSize = true;
                filterForm.Controls.Add(columnLabel);

                ComboBox columnComboBox = new ComboBox();
                columnComboBox.Location = new Point(10, 40);
                columnComboBox.Size = new Size(260, 20);
                if (grid.DataSource is DataTable dataTable)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        columnComboBox.Items.Add(column.ColumnName);
                    }
                }
                columnComboBox.SelectedIndex = 0; // По умолчанию первый столбец
                filterForm.Controls.Add(columnComboBox);

                Label valueLabel = new Label();
                valueLabel.Text = "Введите значение:";
                valueLabel.Location = new Point(10, 70);
                valueLabel.AutoSize = true;
                filterForm.Controls.Add(valueLabel);

                TextBox valueTextBox = new TextBox();
                valueTextBox.Location = new Point(10, 90);
                valueTextBox.Size = new Size(260, 20);
                filterForm.Controls.Add(valueTextBox);

                Button confirmButton = new Button();
                confirmButton.Text = "Применить";
                confirmButton.Font = new Font("Arial", 12);
                confirmButton.Location = new Point(10, 120);
                confirmButton.Size = new Size(100, 40);
                confirmButton.Click += (s, e) =>
                {
                    if (grid.DataSource is DataTable filteredDataTable)
                    {
                        string selectedColumn = columnComboBox.SelectedItem.ToString();
                        string filterValue = valueTextBox.Text.Trim();

                        DataView dataView = filteredDataTable.DefaultView;
                        if (string.IsNullOrEmpty(filterValue))
                        {
                            dataView.RowFilter = ""; // Сброс фильтра
                        }
                        else
                        {
                            // Проверяем тип данных столбца
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
                    // Экспорт всех таблиц
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

                // Экспорт в Excel с добавлением диаграммы
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
                // Создание нового Excel-приложения
                excelApp = new Excel.Application();
                excelApp.Visible = true; // Excel будет виден пользователю
                workBook = excelApp.Workbooks.Add();

                // Создаем лист "Дополнительно" как первый лист
                Excel.Worksheet workSheetAdditional = (Excel.Worksheet)workBook.Sheets[1];
                workSheetAdditional.Name = "Дополнительно";

                // Перебираем все таблицы в dataSet и добавляем их в Excel
                int sheetIndex = 2;
                foreach (DataTable table in dataSet.Tables)
                {
                    // Создаем новый лист с именем таблицы
                    Excel.Worksheet workSheet = (Excel.Worksheet)workBook.Sheets.Add();
                    workSheet.Name = table.TableName.Length > 31 ? table.TableName.Substring(0, 31) : table.TableName; // Ограничение длины имени листа в Excel

                    // Заголовки колонок
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        workSheet.Cells[1, i + 1] = table.Columns[i].ColumnName;
                    }

                    // Данные таблицы
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            workSheet.Cells[i + 2, j + 1] = table.Rows[i][j]?.ToString(); // Приведение к строке для избежания проблем с типами
                        }
                    }

                    // Автоматически растягиваем колонки и строки
                    workSheet.Columns.AutoFit();
                    workSheet.Rows.AutoFit();

                    sheetIndex++;
                }

                // Добавляем диаграмму на лист "Дополнительно" для текущей таблицы
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
                // Освобождаем ресурсы
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
                // Определяем данные для диаграммы в зависимости от таблицы
                string categoryColumn = "";
                string valueColumn = "";
                string chartTitle = "";
                bool useCount = true; // Флаг для подсчета записей (используется, если нет подходящего числового столбца)

                switch (tableName)
                {
                    case "Аэропорты":
                        categoryColumn = "Название_аэропорта"; // Соответствует вашей структуре
                        valueColumn = null; // Подсчитываем количество терминалов
                        chartTitle = "Количество терминалов по аэропортам";
                        break;
                    case "Терминалы":
                        categoryColumn = "ID_Аэропорта";
                        valueColumn = null; // Подсчитываем количество выходов
                        chartTitle = "Количество терминалов по аэропортам";
                        break;
                    case "Выходы":
                        categoryColumn = "ID_Терминала";
                        valueColumn = null; // Подсчитываем количество выходов
                        chartTitle = "Количество выходов по терминалам";
                        break;
                    case "Авиакомпании":
                        categoryColumn = "Название_авиакомпании";
                        valueColumn = null; // Подсчитываем количество самолетов
                        chartTitle = "Количество самолетов по авиакомпаниям";
                        break;
                    case "Самолёты":
                        categoryColumn = "ID_Авиакомпании";
                        valueColumn = "Вместимость";
                        useCount = false; // Используем вместимость как числовое значение
                        chartTitle = "Вместимость самолетов по авиакомпаниям";
                        break;
                    case "Рейсы":
                        categoryColumn = "ID_Авиакомпании";
                        valueColumn = null; // Подсчитываем количество рейсов
                        chartTitle = "Количество рейсов по авиакомпаниям";
                        break;
                    case "Билеты":
                        categoryColumn = "ID_Рейса";
                        valueColumn = null; // Подсчитываем количество билетов
                        chartTitle = "Количество билетов по рейсам";
                        break;
                    case "Сотрудники":
                        categoryColumn = "Должность";
                        valueColumn = null; // Подсчитываем количество сотрудников по должности
                        chartTitle = "Количество сотрудников по должностям";
                        break;
                    case "Пассажиры":
                        categoryColumn = "Национальность";
                        valueColumn = null; // Подсчитываем количество пассажиров по национальности
                        chartTitle = "Количество пассажиров по национальности";
                        break;
                    case "Обслуживание_рейсов":
                        categoryColumn = "ID_Рейса";
                        valueColumn = null; // Подсчитываем количество обслуживаний по рейсам
                        chartTitle = "Количество обслуживаний по рейсам";
                        break;
                    default:
                        return; // Если таблица не определена, пропускаем
                }

                // Проверяем наличие столбца категорий
                if (!dataTable.Columns.Contains(categoryColumn))
                {
                    MessageBox.Show($"Столбец '{categoryColumn}' не найден в таблице '{tableName}'. Проверьте структуру таблицы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Подготовка данных для диаграммы
                var groupedData = dataTable.AsEnumerable()
                    .GroupBy(r => r.Field<object>(categoryColumn)?.ToString())
                    .Select(g => new
                    {
                        Category = g.Key,
                        Value = useCount
                            ? (double)g.Count() // Преобразуем int в double
                            : g.Sum(row => row.Field<object>(valueColumn) != null ? Convert.ToDouble(row.Field<object>(valueColumn)) : 0)
                    })
                    .OrderBy(g => g.Category)
                    .ToList();

                if (groupedData.Count == 0)
                {
                    MessageBox.Show($"Нет данных для построения диаграммы для таблицы '{tableName}'.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Записываем данные в лист "Дополнительно"
                workSheet.Cells[1, 1] = "Категория";
                workSheet.Cells[1, 2] = useCount ? "Количество" : valueColumn;
                int excelRow = 2; // Переименовано из row, чтобы избежать конфликта
                foreach (var item in groupedData)
                {
                    workSheet.Cells[excelRow, 1] = item.Category ?? "Не указано";
                    workSheet.Cells[excelRow, 2] = item.Value;
                    excelRow++;
                }

                // Создаем диапазон данных для диаграммы
                Excel.Range chartRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[excelRow - 1, 2]];

                // Создаем диаграмму
                Excel.ChartObjects chartObjects = (Excel.ChartObjects)workSheet.ChartObjects();
                Excel.ChartObject chartObject = chartObjects.Add(100, 100, 600, 400); // Позиция и размер диаграммы
                Excel.Chart chart = chartObject.Chart;
                chart.SetSourceData(chartRange);
                chart.ChartType = Excel.XlChartType.xlColumnClustered; // Тип диаграммы: столбчатая
                chart.HasTitle = true;
                chart.ChartTitle.Text = chartTitle;
                chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = categoryColumn;
                chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = useCount ? "Количество" : valueColumn;

                // Форматирование диаграммы
                chart.Legend.Delete(); // Удаляем легенду, так как она не нужна для одной серии данных
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
                                adapter.Update(dataTable); // Сохраняет новые, изменённые и удалённые строки
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
            // Автоматически сохраняем изменения после завершения редактирования ячейки
            SaveChanges();
        }
    }
}