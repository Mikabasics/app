@Grab(group='org.mongodb', module='mongodb-driver-sync', version='4.4.0')
import com.mongodb.client.MongoClients
import com.mongodb.client.MongoDatabase
import com.mongodb.client.MongoCollection
import org.bson.Document
import groovy.json.JsonOutput

// MongoDB Atlas-Verbindung
def connectionString = "mongodb+srv://tobizander:mnvSdd2p1SIqyFJh@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data"
def client = MongoClients.create(connectionString)

// Wähle Datenbank und Collection
MongoDatabase database = client.getDatabase("adminuser")
MongoCollection<Document> collection = database.getCollection("user")

// Dokumente abfragen
def documents = collection.find()

// Jedes Dokument in JSON umwandeln und ausgeben
def documentList = []
documents.each { document ->
    documentList << document
}

// Gesamtes JSON-Array ausgeben
def jsonArray = JsonOutput.prettyPrint(JsonOutput.toJson(documentList))
println jsonArray

// Verbindung schließen
client.close()
