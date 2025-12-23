using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Habit_Tracker
{
    internal class Program
    {
        public static string ConnectionString = "Data Source=HabitTracker.db";
        public bool FirstStart = true;
        static void Main(string[] args)
        {

            MainMenu();
        }

        private static void MainMenu()
        {
            CreateTable();
            Console.Clear();
            Console.WriteLine("\n Welcome to your Habit Tracker!");
            Console.WriteLine("\n\t Press 1 to view your Habits");
            Console.WriteLine("\t Press 2 to insert a new Habit.");
            Console.WriteLine("\t Press 3 to delete a Habit.");
            Console.WriteLine("\t Press 4 to update a Habit.");
            Console.WriteLine("\t Press 5 to Delete all Habits.");
            Console.WriteLine("\t Press 0 to exit.");

            string? answer = Console.ReadLine();

            switch (answer)
            {
                case "1":
                    ViewHabits();
                    Console.WriteLine("\nPress Anything to retun to the Main Menu");
                    Console.ReadKey();
                    MainMenu();
                    break;
                case "2":
                    InsertHabit();
                    break;
                case "3":
                    DeleteHabit();
                    break;
                case "4":
                    UpdateHabit();
                    break;
                case "5":
                    DeleteAllHabits();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\t Invalid Input, try again!");
                    Console.ReadKey();
                    MainMenu();
                    break;
            }
        }
        private static void CreateTable()
        {
            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            var CreateTable = Connection.CreateCommand();
            CreateTable.CommandText = @"CREATE TABLE IF NOT EXISTS Habits(
                                                                   ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                   HabitName TEXT,
                                                                   QuantityName TEXT,
                                                                   Quantity INTEGER,
                                                                   Date TEXT);";

            CreateTable.ExecuteNonQuery();
            Connection.Close();
            //First solution was to run it every time the table is created by checking if the table is empty/non existant but i realised that this is not a good solution
            //So the solution i came up with is to create a .txt file that marks the first time the program is executed. Not sure if there was a smarter solution but i couldnt think of one
            string habitDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string firstRunFilePath = Path.Combine(habitDirectory, "FirstRun.txt");
            if (!File.Exists(firstRunFilePath))
            {
                Console.WriteLine("First run detected, seeding data");
                SeedData();
                using (StreamWriter file = new StreamWriter(firstRunFilePath))
                {
                    file.WriteLine("This file indicates that the program has already run.");
                }
            }
        }
        private static void SeedData()
        {
            Dictionary<string, string> Habits = new();
            Habits.Add("Working Out", "Hours");
            Habits.Add("Drinking Water", "Glasses");
            Habits.Add("Reading a book", "Pages");
            Habits.Add("Practice coding", "Hours");
            Habits.Add("Play chess", "Games Played");
            Habits.Add("Run", "km");

            Random rand = new();
            int habitsAdded = 100;//rand.Next(10, 100);

            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            using var AddHabit = Connection.CreateCommand();


            for (int i = 0; i < habitsAdded; i++)
            {
                int habitPicker = rand.Next(0, 6);
                string day = (rand.Next(1, 32)).ToString("D2");
                string month = "12"; //(rand.Next(1, 13)).ToString("D2");
                string year = "2025"; 
                int Quantity = rand.Next(1,11);

                string HabitName = Habits.ElementAt(habitPicker).Key;
                string QuantityName = Habits.ElementAt(habitPicker).Value;
                string DateString = $"{day}-{month}-{year}";
                DateTime Date = DateTime.ParseExact(DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                AddHabit.CommandText = $@"INSERT INTO Habits(HabitName, QuantityName, Quantity, Date)
                                      VALUES('{HabitName}','{QuantityName}', '{Quantity}', '{DateString}')";
                AddHabit.ExecuteNonQuery();
            }
            Connection.Close();
            Console.WriteLine("Database seeded with random entries, returning to Main Menu");
/*            Console.ReadKey();
            MainMenu();*/
        }
        private static void ViewHabits()
        {
//MAYBE UNIT DATA, README
            Console.Clear();
            
            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            var PrintTable = Connection.CreateCommand();
            Console.WriteLine("Type 1 to see all habits, Type 2 to sort habits by HabitName, Type 3 to sort habits chronologically, Type 0 to cancel");
            string? answer = Console.ReadLine();
            switch (answer)
            {
                case "1":
                    PrintTable.CommandText = $@"SELECT * FROM Habits";
                    break;
                case "2":
                    Console.WriteLine("Write the name of the habit you are interested in");
                    string? HabitName = Console.ReadLine();
                    PrintTable.CommandText = $@"SELECT * FROM Habits WHERE HabitName = $HabitName";
                    PrintTable.Parameters.AddWithValue("$HabitName", HabitName);
                    break;
                case "3":
                    PrintTable.CommandText = $@"SELECT * FROM Habits ORDER BY Date ASC";
                    break;
                case "0":
                    MainMenu();
                    break;
                default:
                    Console.WriteLine("Invalid input, returning to main menu");
                    Console.ReadKey();
                    MainMenu();
                    break;

            }

            var tableList = new List<Habit>();
            using var reader = PrintTable.ExecuteReader();
            if (!reader.HasRows) Console.WriteLine("\tHabit Tracker is empty");
            else
            {
                while (reader.Read())
                {
                    string DateString = reader.GetString(4);
                    Habit habit = new()
                    {
                        ID = reader.GetInt32(0),
                        HabitName = reader.GetString(1),
                        QuantityName = reader.GetString(2),
                        Quantity = reader.GetInt32(3),
                        Date = DateTime.ParseExact(DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
                    };
                    tableList.Add(habit);
                }
                Console.Clear();
                foreach (Habit habit in tableList)
                {
                    Console.WriteLine($"ID: {habit.ID} -- {habit.HabitName}: {habit.Quantity} {habit.QuantityName}, Date: {habit.Date.ToString("dd/MM/yyyy")}");
                }
            }
            Connection.Close();
        }
        private static void InsertHabit()
        {
            Console.Clear();

            string Date = GetUserDate();
            Console.WriteLine("\tPlease input the Habit type (string)");
            Console.WriteLine("\tAlternatively type 0 to cancel;");
            string? HabitName = Console.ReadLine();
            if (HabitName == "0")
            {
                Console.WriteLine("'\tInsertion Cancelled");
                Console.ReadKey();
                MainMenu();
            }

            Console.WriteLine("\tPlease input the quantity type (string)");
            Console.WriteLine("\tAlternatively type 0 to cancel;");
            string? QuantityName = Console.ReadLine();
            if (QuantityName == "0")
            {
                Console.WriteLine("'\tInsertion Cancelled");
                Console.ReadKey();
                MainMenu();
            }

            Console.WriteLine("\tPlease input the quantity");
            Console.WriteLine("\tAlternatively type 0 to cancel;");
            string? Quantity = Console.ReadLine();
            if (Quantity == "0")
            {
                Console.WriteLine("'\tInsertion Cancelled");
                Console.ReadKey();
                MainMenu();
            }


            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            using var AddHabit = Connection.CreateCommand();
            AddHabit.CommandText = $@"INSERT INTO Habits(HabitName, QuantityName, Quantity, Date)
                                      VALUES($HabitName,$QuantityName, $Quantity, $Date)";
            AddHabit.Parameters.AddWithValue("$HabitName", HabitName);
            AddHabit.Parameters.AddWithValue("$QuantityName", QuantityName);
            AddHabit.Parameters.AddWithValue("$Quantity",Quantity);
            AddHabit.Parameters.AddWithValue("$Date", Date);

            int RowsInserted = AddHabit.ExecuteNonQuery();
            if (RowsInserted > 0) Console.WriteLine("\tEntry Inserted");
            else Console.WriteLine("\tInsert Failed");
            Connection.Close();
            Console.ReadKey();
            MainMenu();
        }

        private static void DeleteAllHabits()
        {
            Console.WriteLine("Type y to confirm that you want to delete everything");
            var confirm = Console.ReadLine();
            if (confirm == "y")
            {
                var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                var DeleteAll = connection.CreateCommand();
                DeleteAll.CommandText = "DROP TABLE Habits";
                DeleteAll.ExecuteNonQuery();
                Console.WriteLine("All habits have been deleted, returning to Main Menu");
                Console.ReadKey();
                MainMenu();
            }
            Console.WriteLine("Deleting all habits cancelled, returning to main menu");
            Console.ReadKey();
            MainMenu();
        }

        private static void DeleteHabit()
        {
            Console.Clear();
            ViewHabits();
            Console.WriteLine("\n\tEnter which ID you want to delete");
            string? answer = Console.ReadLine();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var DeleteHabit = connection.CreateCommand();
            DeleteHabit.CommandText = $@"DELETE FROM Habits
                                        WHERE ID = $answer";
            DeleteHabit.Parameters.AddWithValue("$answer", answer);
            int RowsDeleted = DeleteHabit.ExecuteNonQuery();
            if (RowsDeleted < 0) 
            {
                Console.WriteLine($"\n\tHabit with ID: {answer} not found, returning to Main Menu");
                MainMenu();
            }
            Console.WriteLine("\n\tHabit Deleted, click anything to return to main menu");
            Console.ReadKey();
            MainMenu();
        }
        private static void UpdateHabit()
        {
            Console.Clear();
            ViewHabits();
            Console.WriteLine("\t Which Habit would you like to update? (Choose ID) - Choose 0 to go back to Main Menu");
            string? HabitId = Console.ReadLine();
            if (HabitId == "0") MainMenu();
            Console.WriteLine("\tPress 1 to change HabitName, Press 2 to change QuantityName, Press 3 to change Quantity, Press 4 to change Date, Press 0 to cancel ");
            string? Choice = Console.ReadLine();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var UpdateValue = connection.CreateCommand();
            UpdateValue.Parameters.AddWithValue("$HabitId", HabitId);
            string? NewValue;
            switch (Choice)
            {
                case "0":
                    MainMenu();
                    break;
                case "1":
                    Console.WriteLine("Input New HabitName Value: (String)");
                    NewValue = Console.ReadLine();
                    UpdateValue.Parameters.AddWithValue("$NewValue", NewValue);
                    UpdateValue.CommandText = $@"UPDATE Habits
                                         SET HabitName = $NewValue
                                         WHERE ID = $HabitId";
                    break;
                case "2":
                    Console.WriteLine("Input New QuantityName Value: (String)");
                    NewValue = Console.ReadLine();
                    UpdateValue.Parameters.AddWithValue("$NewValue", NewValue);
                    UpdateValue.CommandText = $@"UPDATE Habits
                                         SET QuantityName = $NewValue
                                         WHERE ID = $HabitId";
                    break;
                case "3":
                    Console.WriteLine("Input New Quantity Value: (INTEGER)");
                    NewValue = Console.ReadLine();
                    UpdateValue.Parameters.AddWithValue("$NewValue", NewValue);
                    UpdateValue.CommandText = $@"UPDATE DrinkingWater
                                         SET Quantity = $NewValue
                                         WHERE ID = $HabitId";
                    break;
                case "4":
                    NewValue = GetUserDate();
                    UpdateValue.CommandText = $@"UPDATE DrinkingWater
                                         SET Date = '{NewValue}'
                                         WHERE ID = $HabitId";
                    break;
                default:
                    Console.WriteLine("Invalid Input, Try Again");
                    UpdateHabit();
                    break;
            }

            int SuccessCheck = UpdateValue.ExecuteNonQuery();
            if (SuccessCheck >= 0)
            {
                Console.WriteLine($"Successfully Updated Habit with ID {HabitId}, press anything to go back to main menu");
                Console.ReadKey();
                MainMenu();
            }
            else
            {
                Console.WriteLine("Update Failed, returning to Main Menu");
                Console.ReadKey();
                MainMenu();
            }
            connection.Close();

        }

        private static string GetUserDate()
        {
            Console.WriteLine("\n\tPlease input a valid date (Format: dd-mm-yyyy)");
            Console.WriteLine("\tAlternatively type 0 to cancel;");
            string? Date = Console.ReadLine();
            if (Date == "0") MainMenu();
            while (!DateTime.TryParseExact(Date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                Console.WriteLine("\n\tInvalid Date Format, please input a valid date (Format: dd-mm-yyyy)");
                Console.WriteLine("\tAlternatively type 0 to cancel;");
                Date = Console.ReadLine();
                if (Date == "0")
                {
                    Console.WriteLine("'\tInsertion Cancelled");
                    Console.ReadKey();
                    MainMenu();
                }
            }
            return Date;
        }
        internal class Habit
        {
            public int ID { get; set; }
            public required string HabitName { get; set; }
            public required string QuantityName { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
