using Microsoft.Data.Sqlite;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Habit_Tracker
{
    internal class Program
    {
        public static string ConnectionString = "Data Source=HabitTracker.db";
        static void Main(string[] args)
        {
            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            var CreateTable = Connection.CreateCommand();
            CreateTable.CommandText = @"CREATE TABLE IF NOT EXISTS DrinkingWater(
                                                                   ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                   Quantity INTEGER,
                                                                   Date TEXT);";
            CreateTable.ExecuteNonQuery();
            Connection.Close();
            MainMenu();
        }

        private static void MainMenu()
        {
            Console.Clear();
            Console.WriteLine("\n Welcome to your Habit Tracker!");
            Console.WriteLine("\n\t Press 1 to view your Habits");
            Console.WriteLine("\t Press 2 to insert a new Habit.");
            Console.WriteLine("\t Press 3 to delete a Habit.");
            Console.WriteLine("\t Press 4 to update a Habit.");
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
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\t Invalid Input, try again:");
                    Console.ReadKey();
                    MainMenu();
                    break;
            }
        }
        private static void ViewHabits()
        {
            /*Clear the Console to easier see all of habits. We then go through all the elements in the table and create classes.
           We add said classes to a List and then use the List to print all of the values in a formatted way. 
           */

            Console.Clear();
            
            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            var PrintTable = Connection.CreateCommand();
            PrintTable.CommandText = $@"SELECT * FROM DrinkingWater";

            var tableList = new List<DrinkWater>();
            using var reader = PrintTable.ExecuteReader();
            if (!reader.Read()) Console.WriteLine("\tHabit Tracker is empty");
            while (reader.Read())
            {
                string DateString = reader.GetString(2);
                DrinkWater habit = new DrinkWater
                {
                    ID = reader.GetInt32(0),
                    Quantity = reader.GetInt32(1),
                    Date = DateTime.ParseExact(DateString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
                };
                tableList.Add(habit);
            }

            foreach (DrinkWater habit in tableList)
            {
                Console.WriteLine($"ID: {habit.ID} -- Quantity: {habit.Quantity}, Date: {habit.Date.ToString("dd/MM/yyyy")}");
            }

            Connection.Close();
        }
        private static void InsertHabit()
        {
            Console.Clear();

            string Date = GetUserDate();
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
            AddHabit.CommandText = $@"INSERT INTO DrinkingWater(Quantity, Date)
                                      VALUES({Quantity}, '{Date}')";
            int RowsInserted = AddHabit.ExecuteNonQuery();
            if (RowsInserted > 0) Console.WriteLine("\tEntry Inserted");
            else Console.WriteLine("\tInsert Failed");
            Connection.Close();
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
            DeleteHabit.CommandText = $@"DELETE FROM DrinkingWater
                                        WHERE ID = {answer}";
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
            Console.WriteLine("\tPress 1 to change Quantity, Press 2 to change Date, Press 0 to cancel ");
            string? Choice = Console.ReadLine();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var UpdateValue = connection.CreateCommand();
            string? NewValue;
            switch (Choice)
            {
                case "0":
                    MainMenu();
                    break;
                case "1":
                    Console.WriteLine("Input New Quantity Value: (INTEGER)");
                    NewValue = Console.ReadLine();
                    UpdateValue.CommandText = $@"UPDATE DrinkingWater
                                         SET Quantity = {NewValue}
                                         WHERE ID = {HabitId}";
                    break;
                case "2":
                    NewValue = GetUserDate();
                    UpdateValue.CommandText = $@"UPDATE DrinkingWater
                                         SET Date = '{NewValue}'
                                         WHERE ID = {HabitId}";
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
        internal class DrinkWater
        {
            public int ID { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
