using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using practice.Models;
public class UserService
{
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static readonly string FolderPath = Path.Combine(DesktopPath, "LocalDB");
    private static readonly string FilePath = Path.Combine(FolderPath, "users.json");
    public string? loggedInUserName { get; set; }

    // creating a new transactions for user
    public List<practice.Models.Transaction> GetUserTransactions(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        return user?.Transactions ?? new List<practice.Models.Transaction>(); 
    }

    // clearing transactions
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

    //updating credit
    public void UpdateCredit(string username, int newCredit)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            user.Credit += newCredit;
            
            if(user.Transactions == null)
            {
                user.Transactions = new List<Transaction>();
            }

            user.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = "Credit",
                Amount = newCredit,
                Description = $"Added {newCredit} to Credit"
            });

            SaveUsers(users);         
        }
    }

    // updating debit
    public void UpdateDebit(string username, int newDebit)
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
                Description = $"Added {newDebit} to Debit"
            });

            SaveUsers(users);
        }
    }

    //updating debt
    public void UpdateDebt(string username, int newDebt)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u =>u.Username == username);
        if (user != null)
        {
            user.Debt += newDebt;
            user.Credit += newDebt;
        }

        if(user.Transactions == null)
        {
            user.Transactions = new List<Transaction>();
        }

        user.Transactions.Add(new Transaction
        {
            Date = DateTime.Now,
            Type = "Debt",
            Amount = newDebt,
            Description = $"Added {newDebt} to Debt"
        });
        SaveUsers(users);
        }

    // clearing debt
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
                Description = $"Cleared {user.Debt} from Debt"
            });

            user.Credit -= user.Debt;
            user.Debt = 0;

            SaveUsers(users);
        }
        else
        {
            throw new Exception("Your debt is more than your credit.");
        }
    }


//resetting debit on logout
public void ResetDebit(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        user.Debit = 0;
        SaveUsers(users);
    }

    public User? GetLoggedInUser()
    {
        var users = LoadUsers();
        return users.FirstOrDefault(u => u.Username == loggedInUserName);
        
    }



    public List<User> LoadUsers()
    {
        if (!File.Exists(FilePath))
            return new List<User>();  

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }
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
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);  
    }
    public bool ValidatePassword(string inputPassword, string storedPassword)
    {
        var hashedInputPassword = HashPassword(inputPassword);
        return hashedInputPassword == storedPassword;
    }
}


