using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebadminBackend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("surname")]
        public string Surname { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("generatedEmail")]
        public string GeneratedEmail { get; set; } = string.Empty;

        [BsonElement("birthdate")]
        public string Birthdate { get; set; } = string.Empty;

        [BsonElement("gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("right")]
        public string Right { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Givenname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Birthdate { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Right { get; set; } = string.Empty;
    }

    public class MassRegisterRequest
    {
        public int Mass { get; set; }
        public string Right { get; set; } = string.Empty;
        public string Password { get; set; } = "Start123!";
        public bool GenerateEmail { get; set; } = false;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
