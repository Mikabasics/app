using MongoDB.Driver;
using WebadminBackend.Models;
using BCrypt.Net;

namespace WebadminBackend.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _connectionString = "mongodb+srv://tobizander:mnvSdd2p1SIqyFJh@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data";

        public UserService()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("adminuser");
            _users = database.GetCollection<User>("user");
        }

        public async Task<bool> HandleLoginAsync(string username, string password)
        {
            try
            {
                var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
                
                if (user != null && BCrypt.Verify(password, user.Password))
                {
                    Console.WriteLine("Benutzerabfrage durchgeführt: Benutzer gefunden");
                    return true;
                }
                else
                {
                    Console.WriteLine("Benutzerabfrage durchgeführt: Benutzer nicht gefunden oder Passwort falsch");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"MongoDB Fehler: {e.Message}");
                throw new Exception($"Datenbankfehler: {e.Message}");
            }
        }

        private string GenerateEmail(string givenname, string surname)
        {
            // Umlaute ersetzen
            string ReplaceUmlauts(string input)
            {
                return input.ToLower()
                    .Replace("ä", "ae")
                    .Replace("ö", "oe")
                    .Replace("ü", "ue")
                    .Replace("ß", "ss");
            }

            // Konvertiere Namen und entferne verbleibende Sonderzeichen
            var cleanGivenname = System.Text.RegularExpressions.Regex.Replace(
                ReplaceUmlauts(givenname), "[^a-zA-Z]", "");
            var cleanSurname = System.Text.RegularExpressions.Regex.Replace(
                ReplaceUmlauts(surname), "[^a-zA-Z]", "");

            // Erstelle E-Mail: vorname.nachname
            return $"{cleanGivenname}.{cleanSurname}@ovgu.de";
        }

        public async Task HandleRegistrationAsync(RegisterRequest userData)
        {
            try
            {
                var existingUser = await _users.Find(u => 
                    u.Username == userData.Givenname &&
                    u.Surname == userData.Surname &&
                    u.Email == userData.Email &&
                    u.Gender == userData.Gender &&
                    u.Right == userData.Right
                ).FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    throw new Exception("Benutzer existiert bereits");
                }

                if (string.IsNullOrEmpty(userData.Givenname) || string.IsNullOrEmpty(userData.Surname))
                {
                    throw new Exception("Vorname und Nachname sind erforderlich");
                }

                var baseEmail = GenerateEmail(userData.Givenname, userData.Surname);
                var generatedEmail = baseEmail;

                var emailExists = await _users.Find(u => u.GeneratedEmail == generatedEmail).FirstOrDefaultAsync();
                int counter = 1;
                while (emailExists != null)
                {
                    generatedEmail = baseEmail.Replace("@ovgu.de", "") + counter + "@ovgu.de";
                    emailExists = await _users.Find(u => u.GeneratedEmail == generatedEmail).FirstOrDefaultAsync();
                    counter++;
                }

                var user = new User
                {
                    Username = userData.Givenname,
                    Surname = userData.Surname,
                    Email = userData.Email,
                    GeneratedEmail = generatedEmail,
                    Birthdate = userData.Birthdate,
                    Gender = userData.Gender,
                    Right = userData.Right
                };

                await _users.InsertOneAsync(user);
                Console.WriteLine("Benutzer erfolgreich registriert");
            }
            catch (Exception e)
            {
                Console.WriteLine($"MongoDB Fehler: {e.Message}");
                throw;
            }
        }

        public async Task HandleMassRegistrationAsync(int mass, string right, string password, bool generateEmail)
        {
            try
            {
                var results = new List<object>();
                var baseUsername = "gast";
                int counter = 1;

                for (int i = 0; i < mass; i++)
                {
                    var username = $"{baseUsername}{counter}";
                    var email = generateEmail ? $"{username}@ovgu.de" : null;

                    var existingUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
                    while (existingUser != null)
                    {
                        counter++;
                        username = $"{baseUsername}{counter}";
                        existingUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
                    }

                    var user = new User
                    {
                        Username = username,
                        Password = BCrypt.HashPassword(password),
                        Right = right
                    };

                    if (!string.IsNullOrEmpty(email))
                    {
                        user.Email = email;
                    }

                    await _users.InsertOneAsync(user);
                    results.Add(user);
                    counter++;
                }

                Console.WriteLine($"{mass} Benutzer erfolgreich registriert");
            }
            catch (Exception e)
            {
                Console.WriteLine($"MongoDB Fehler: {e.Message}");
                throw;
            }
        }
    }
}
