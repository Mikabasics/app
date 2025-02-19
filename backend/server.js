const dotenv = require('dotenv'); // dotenv importieren
dotenv.config();
const express = require('express');
const cors = require('cors');
const app = express();

app.use(cors({
    origin: 'http://localhost:5173', // Vue.js dev server
    credentials: true
}));

app.use(express.json());

async function handleLogin(username, password) {
    const MongoClient = require('mongodb').MongoClient;
    const uri = `mongodb+srv://${process.env.API_NAME}:${process.env.API_PASSWORD}@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data`; // URI aus Umgebungsvariablen
    let client;

    try {
        client = new MongoClient(uri, {
            useNewUrlParser: true,
            useUnifiedTopology: true
        });
        
        await client.connect();
        console.log('MongoDB Verbindung hergestellt');
        
        const database = client.db("adminuser");
        const collection = database.collection("user");
        
        const user = await collection.findOne({
            username: username,
            password: password
        });
        
        console.log('Benutzerabfrage durchgeführt:', user ? 'Benutzer gefunden' : 'Benutzer nicht gefunden');
        return user !== null;
    } catch (error) {
        console.error('MongoDB Fehler:', error);
        throw new Error('Datenbankfehler: ' + error.message);
    } finally {
        if (client) {
            await client.close();
            console.log('MongoDB Verbindung geschlossen');
        }
    }
}

app.post('/api/login', async (req, res) => {
    try {
        const { username, password } = req.body;
        console.log('Login-Versuch für Benutzer:', username);
        
        if (!username || !password) {
            return res.status(400).json({ error: 'Benutzername und Passwort sind erforderlich' });
        }
        
        const loginResult = await handleLogin(username, password);
        console.log('Login-Ergebnis:', loginResult);
        
        if (loginResult) {
            res.json({ success: true });
        } else {
            res.status(401).json({ error: 'Ungültige Anmeldedaten' });
        }
    } catch (error) {
        console.error('Login-Fehler:', error);
        res.status(500).json({ error: 'Interner Server-Fehler: ' + error.message });
    }
});


async function handleRegistration(userData) {
    const MongoClient = require('mongodb').MongoClient;
    const uri = `mongodb+srv://${process.env.API_NAME}:${process.env.API_PASSWORD}@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data`; // URI aus Umgebungsvariablen
    let client;

    try {
        client = new MongoClient(uri, {
            useNewUrlParser: true,
            useUnifiedTopology: true
        });
        
        await client.connect();
        console.log('MongoDB Verbindung hergestellt');
        
        const database = client.db("adminuser");
        const collection = database.collection("user");
        
        // Prüfen ob Benutzer bereits existiert
        const existingUser = await collection.findOne({ username: userData.username });
        if (existingUser) {
            throw new Error('Benutzer existiert bereits');
        }
        
        // Neuen Benutzer einfügen
        const result = await collection.insertOne({
            username: userData.username,
            password: userData.password,
            surname: userData.surname,
            email: userData.email,
            gender: userData.gender,
            rechte: userData.rechte
        });
        
        console.log('Benutzer erfolgreich registriert');
        return result;
    } catch (error) {
        console.error('MongoDB Fehler:', error);
        throw error;
    } finally {
        if (client) {
            await client.close();
            console.log('MongoDB Verbindung geschlossen');
        }
    }
}

app.post('/api/register', async (req, res) => {
    try {
        const userData = req.body;
        
        if (!userData.username || !userData.password || !userData.surname || !userData.gender) { // Änderung hier: Geschlecht als Pflichtfeld hinzufügen
            return res.status(400).json({ error: 'Alle Pflichtfelder müssen ausgefüllt werden' });
        }
        
        await handleRegistration(userData);
        res.json({ success: true, message: 'Benutzer erfolgreich registriert' });
    } catch (error) {
        console.error('Registrierungsfehler:', error);
        res.status(500).json({ error: error.message });
    }
});

const port = 8080; 
app.listen(port, () => {
    console.log(`Server läuft auf http://localhost:${port}`);
});
