using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;
using practice.Models;

public class UserService
{
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static readonly string FolderPath = Path.Combine(DesktopPath, "LocalDB");
    private static readonly string FilePath = Path.Combine(FolderPath, "users.json");
    public string? loggedInUserName { get; set; }

    // Retrieve all transactions for a specific user
    public List<practice.Models.Transaction> GetUserTransactions(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        return user?.Transactions ?? new List<practice.Models.Transaction>();
    }

    // Remove all transactions for a specific user
    public void ClearTransaction(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            user.Transactions.Clear();
            SaveUsers(users);
        }
    }

    // Increase credit balance and add a credit transaction
    public void UpdateCredit(string username, int newCredit, string Tag, string note)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            user.Credit += newCredit;

            if (user.Transactions == null)
            {
                user.Transactions = new List<Transaction>();
            }

            user.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = "Credit",
                Amount = newCredit,
                Description = $"Successfully added {newCredit} credits to the user account.",
                Tag = Tag,
                note = note
            });

            SaveUsers(users);
        }
    }

    // Decrease credit balance and add a debit transaction
    public void UpdateDebit(string username, int newDebit, string Tag, string note)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            if (user.Credit >= newDebit)
            {
                user.Debit += newDebit;
                user.Credit -= newDebit;
            }

            if (user.Transactions == null)
            {
                user.Transactions = new List<Transaction>();
            }

            user.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = "Debit",
                Amount = newDebit,
                Description = $"Successfully debited {newDebit} from the user account.",
                Tag = Tag,
                note = note
            });

            SaveUsers(users);
        }
    }

    // Add a new debt and update credit balance
    public void UpdateDebt(string username, int newDebt, string Tag, string note)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            user.Debt += newDebt;
            user.Credit += newDebt;

            if (user.Transactions == null)
            {
                user.Transactions = new List<Transaction>();
            }

            user.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = "Debt",
                Amount = newDebt,
                Description = $"Successfully recorded a debt of {newDebt} to the user account.",
                Tag = Tag, 
                note = note

            });

            SaveUsers(users);
        }
    }

    // Clear all debt if credit is sufficient
    public void ClearDebt(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null && user.Credit >= user.Debt && user.Debt != 0)
        {
            if (user.Transactions == null)
            {
                user.Transactions = new List<Transaction>();
            }

            user.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = "Debt",
                Amount = user.Debt,
                Description = $"Successfully cleared a debt of {user.Debt} from the user account."
            });

            user.Credit -= user.Debt;
            user.Debt = 0;

            SaveUsers(users);
        }
        else
        {
            throw new Exception("Unable to clear debt: Insufficient credit to cover the debt.");
        }
    }

    // Reset debit amount when user logs out
    public void ResetDebit(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        user.Debit = 0;
        SaveUsers(users);
    }

    // Retrieve the currently logged-in user
    public User? GetLoggedInUser()
    {
        var users = LoadUsers();
        return users.FirstOrDefault(u => u.Username == loggedInUserName);
    }

    // Load user data from a JSON file
    public List<User> LoadUsers()
    {
        if (!File.Exists(FilePath))
            return new List<User>();

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }

    // Save user data to a JSON file
    public void SaveUsers(List<User> users)
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, "[]");
        }

        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    // Generate a SHA256 hash of a password
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    // Validate a password by comparing its hash to the stored hash
    public bool ValidatePassword(string inputPassword, string storedPassword)
    {
        var hashedInputPassword = HashPassword(inputPassword);
        return hashedInputPassword == storedPassword;
    }
}
