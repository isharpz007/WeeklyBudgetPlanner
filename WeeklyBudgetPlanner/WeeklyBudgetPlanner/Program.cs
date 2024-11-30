namespace WeeklyBudgetPlanner
{
    public abstract class Transaction
    {
        // The Transaction class represents a financial entry, like an expense or income.
        // It is abstract because it serves as a base for more specific transaction types, such as Expense.
        // This design allows flexibility for future extensions, like adding income or other transaction categories.

        public string Description { get; set; } // The description of the transaction.
        public decimal Amount { get; set; } // The monetary value of the transaction.

        protected Transaction(string description, decimal amount)
        {
            Description = description;
            Amount = amount;
        }

        // Provides a formatted string representation of the transaction, now using the £ symbol instead of $.
        public override string ToString()
        {
            return $"{Description}: £{Amount:F2}";
        }
    }

    public class Expense : Transaction
    {
        // Represents a specific type of transaction: an expense.
        public Expense(string description, decimal amount) : base(description, amount) { }
    }

    public class Budget
    {
        // The total budget for the week.
        public decimal WeeklyBudget { get; set; }

        // This property holds all the user's expenses for the current budget period.
        public List<Expense> Expenses { get; private set; }

        // Initializes a new budget with a specified weekly amount.
        public Budget(decimal weeklyBudget)
        {
            WeeklyBudget = weeklyBudget;
            Expenses = new List<Expense>();
        }

        // Adds a new expense to the budget after validating the amount.
        public void AddExpense(string description, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            Expenses.Add(new Expense(description, amount));
        }

        // Calculates the total of all expenses.
        public decimal CalculateTotalExpenses()
        {
            decimal total = 0;
            foreach (var expense in Expenses)
            {
                total += expense.Amount;
            }
            return total;
        }

        // Calculates the remaining budget by subtracting total expenses from the weekly budget.
        public decimal CalculateRemainingBudget()
        {
            return WeeklyBudget - CalculateTotalExpenses();
        }

        // Displays a summary of the budget, including all expenses and remaining budget.
        public void DisplayBudgetSummary()
        {
            Console.WriteLine($"\nWeekly Budget: £{WeeklyBudget:F2}");
            Console.WriteLine("Expenses:");
            foreach (var expense in Expenses)
            {
                Console.WriteLine(expense);
            }

            Console.WriteLine($"Total Expenses: £{CalculateTotalExpenses():F2}");
            Console.WriteLine($"Remaining Budget: £{CalculateRemainingBudget():F2}");
        }
    }

    class Program
    {
        // File path for saving and loading budget data.
        private static readonly string FilePath = "budget_data.json";

        static async Task Main(string[] args)
        {
            // Load existing budget or initialize a new one if no data is found.
            Budget budget = await LoadBudgetAsync() ?? InitializeBudget();

            while (true)
            {
                // Command-line menu for user interaction.
                Console.WriteLine("\nBudget Planner Options:");
                Console.WriteLine("1. Add Expense");
                Console.WriteLine("2. View Budget Summary");
                Console.WriteLine("3. Save and Exit");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            AddExpense(budget);
                            break;
                        case "2":
                            budget.DisplayBudgetSummary();
                            break;
                        case "3":
                            await SaveBudgetAsync(budget);
                            Console.WriteLine("Budget saved. Goodbye!");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Catch and display any errors that occur during user interaction.
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static Budget InitializeBudget()
        {
            // Prompts the user to enter a weekly budget.
            Console.Write("Enter your weekly budget: ");
            decimal weeklyBudget = ReadPositiveDecimal();
            return new Budget(weeklyBudget);
        }

        private static void AddExpense(Budget budget)
        {
            // Prompts the user to enter expense details and adds it to the budget.
            Console.Write("Enter expense description: ");
            string description = Console.ReadLine();

            Console.Write("Enter expense amount: ");
            decimal amount = ReadPositiveDecimal();

            budget.AddExpense(description, amount);
            Console.WriteLine("Expense added successfully.");
        }

        private static decimal ReadPositiveDecimal()
        {
            // This method validates user input to ensure only positive decimal values are accepted.
            // It keeps prompting the user until a valid input is provided.
            while (true)
            {
                if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
                {
                    return value;
                }

                Console.Write("Invalid input. Enter a positive decimal value: ");
            }
        }

        private static async Task SaveBudgetAsync(Budget budget)
        {
            // Asynchronously saves the budget data to a JSON file.
            string json = JsonSerializer.Serialize(budget, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(FilePath, json);
            Console.WriteLine("Budget saved successfully.");
        }

        private static async Task<Budget> LoadBudgetAsync()
        {
            // Asynchronously loads the budget data from a JSON file if it exists.
            if (!File.Exists(FilePath))
                return null;

            string json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<Budget>(json);
        }
    }
}