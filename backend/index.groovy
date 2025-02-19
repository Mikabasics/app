@Grab(group='io.javalin', module='javalin', version='5.6.3')
@Grab(group='org.slf4j', module='slf4j-simple', version='2.0.7')
@Grab(group='org.mongodb', module='mongodb-driver-sync', version='4.11.1')
@Grab(group='com.fasterxml.jackson.core', module='jackson-databind', version='2.15.2')
@Grab(group='org.mindrot', module='jbcrypt', version='0.4')

//javalin
import io.javalin.Javalin
import org.mindrot.jbcrypt.BCrypt
import com.mongodb.client.MongoClients
import com.mongodb.client.model.Filters
import com.fasterxml.jackson.databind.ObjectMapper

// MongoDB Verbindungsdaten
def connectionString = "mongodb+srv://tobizander:mnvSdd2p1SIqyFJh@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data"
def mongoUri = connectionString

def handleLogin(mongoUri, username, password) {
    def client = MongoClients.create(mongoUri)
    try {
        def database = client.getDatabase("adminuser")
        def collection = database.getCollection("user")
        
        def user = collection.find(
            Filters.and(
                Filters.eq("username", username),
            )
        ).first()
        if (user && BCrypt.checkpw(password, user.password)) {
            println "Benutzerabfrage durchgeführt: Benutzer gefunden"
            return true
        } else {
            println "Benutzerabfrage durchgeführt: Benutzer nicht gefunden oder Passwort falsch"
            return false
        }
    } catch (Exception e) {
        println "MongoDB Fehler: ${e.message}"
        throw new Exception("Datenbankfehler: ${e.message}")
    } finally {
        client.close()
        println "MongoDB Verbindung geschlossen"
    }
}

def handleDelete(mongoUri, username) {
    def client = MongoClients.create(mongoUri)
    try {
        def database = client.getDatabase("adminuser")
        def collection = database.getCollection("user")
        
        // Benutzer löschen
        def result = collection.deleteOne(Filters.eq("username", username))
        if (result.deletedCount > 0) {
            println "Benutzer erfolgreich gelöscht"
            return true
        } else {
            println "Benutzer nicht gefunden"
            return false
        }
    } catch (Exception e) {
        println "MongoDB Fehler: ${e.message}"
        throw new Exception("Datenbankfehler: ${e.message}")
    } finally {
        client.close()
        println "MongoDB Verbindung geschlossen"
    }
}
def handleUpdate(mongoUri, username, userData) {
    def client = MongoClients.create(mongoUri)
    try {
        def database = client.getDatabase("adminuser")
        def collection = database.getCollection("user")
        
        // Benutzer aktualisieren
        def result = collection.updateOne(
            Filters.eq("username", username),
            new org.bson.Document("\$set", new org.bson.Document("surname", userData.surname)
                .append("email", userData.email)
                .append("gender", userData.gender)
                .append("right", userData.right))
        )
        
        if (result.modifiedCount > 0) {
            println "Benutzer erfolgreich aktualisiert"
            return true
        } else {
            println "Benutzer nicht gefunden oder keine Änderungen vorgenommen"
            return false
        }
    } catch (Exception e) {
        println "MongoDB Fehler: ${e.message}"
        throw new Exception("Datenbankfehler: ${e.message}")
    } finally {
        client.close()
        println "MongoDB Verbindung geschlossen"
    }
}

def handleRegistration(mongoUri, userData) {
    def client = MongoClients.create(mongoUri)


    try {
        def database = client.getDatabase("adminuser")
        def collection = database.getCollection("user")
        
        // Prüfen ob Benutzer bereits existiert
        def existingUser = collection.find(Filters.eq("username", userData.username)).first()
        if (existingUser) {
            throw new Exception("Benutzer existiert bereits")
        }
        
        def hashedPassword = BCrypt.hashpw(userData.password, BCrypt.gensalt()) // Passwort hashen
     
        def document = [
                username: userData.username,
                birthdate: userData.birthdate,
                surname: userData.surname,
                email: userData.email, // E-Mail-Adresse speichern
                gender: userData.gender,
                right: userData.right,
            ]
     
        def result = collection.insertOne(document as org.bson.Document)
        println "Benutzer erfolgreich registriert"
        return result
    } catch (Exception e) {
        println "MongoDB Fehler: ${e.message}"
        throw e
    } finally {
        client.close()
        println "MongoDB Verbindung geschlossen"
    }
}

def app = Javalin.create { config ->
    config.plugins.enableCors { cors ->
        cors.add {
            it.allowHost("http://localhost:5173")
            it.allowCredentials = true
        }
    }
}.start(8080)

// Login-Route
app.post("/api/login") { ctx ->

    def body = new ObjectMapper().readValue(ctx.body(), Map)
    def username = body.username
    def password = body.password
    
    if (!username || !password) {
        ctx.status(400).json([error: "Benutzername und Passwort sind erforderlich"])
        return
    }
    
    try {
        def loginResult = handleLogin(mongoUri, username, password) // mongoUri übergeben
        if (loginResult) {
            ctx.json([success: true])
        } else {
            ctx.status(401).json([error: "Ungültige Anmeldedaten"])
        }
    } catch (Exception e) {
        ctx.status(500).json([error: "Interner Server-Fehler: ${e.message}"])
    }
}

// Registrierungs-Route
app.post("/api/register") { ctx ->
    def userData = new ObjectMapper().readValue(ctx.body(), Map)
    
    if (!userData.username || !userData.surname) {
        ctx.status(400).json([error: "Alle Pflichtfelder müssen ausgefüllt werden"])
        return
    }
    
    try {
        handleRegistration(mongoUri, userData) // mongoUri übergeben
        ctx.json([success: true, message: "Benutzer erfolgreich registriert"])
    } catch (Exception e) {
        ctx.status(500).json([error: e.message])
    }
}

app.put("/api/update") { ctx ->
    def body = new ObjectMapper().readValue(ctx.body(), Map)
    def username = body.username
    def userData = [
        surname: body.surname,
        email: body.email,
        gender: body.gender,
        right: body.right
    ]
    
    if (!username || !userData.surname) {
        ctx.status(400).json([error: "Benutzername und Nachname sind erforderlich"])
        return
    }
    
    try {
        def updateResult = handleUpdate(mongoUri, username, userData) // mongoUri übergeben
        if (updateResult) {
            ctx.json([success: true, message: "Benutzer erfolgreich aktualisiert"])
        } else {
            ctx.status(404).json([error: "Benutzer nicht gefunden"])
        }
    } catch (Exception e) {
        ctx.status(500).json([error: "Interner Server-Fehler: ${e.message}"])
    }
}

app.delete("/api/delete") { ctx ->
    def body = new ObjectMapper().readValue(ctx.body(), Map)
    def username = body.username
    
    if (!username) {
        ctx.status(400).json([error: "Benutzername ist erforderlich"])
        return
    }
    
    try {
        def deleteResult = handleDelete(mongoUri, username) // mongoUri übergeben
        if (deleteResult) {
            ctx.json([success: true, message: "Benutzer erfolgreich gelöscht"])
        } else {
            ctx.status(404).json([error: "Benutzer nicht gefunden"])
        }
    } catch (Exception e) {
        ctx.status(500).json([error: "Interner Server-Fehler: ${e.message}"])
    }
}

println "Server läuft auf http://localhost:8080"