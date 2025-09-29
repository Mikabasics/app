from typing import Optional, List, Any, Dict
from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from pymongo import MongoClient
from pymongo.collection import Collection
from bson import ObjectId
import bcrypt


MONGO_URI = "mongodb+srv://tobizander:mnvSdd2p1SIqyFJh@data.3wel7.mongodb.net/?retryWrites=true&w=majority&appName=data"


def get_db_collection() -> Collection:
    client = MongoClient(MONGO_URI)
    db = client.get_database("adminuser")
    return db.get_collection("user")


class LoginBody(BaseModel):
    username: str
    password: str


class RegisterBody(BaseModel):
    givenname: str
    surname: str
    email: Optional[str] = None
    birthdate: Optional[str] = None
    gender: Optional[str] = None
    right: Optional[str] = None


class MassRegisterBody(BaseModel):
    mass: int
    right: str
    password: Optional[str] = "Start123!"
    generateEmail: Optional[bool] = False


def replace_umlauts(input_str: str) -> str:
    return (
        input_str.lower()
        .replace("ä", "ae")
        .replace("ö", "oe")
        .replace("ü", "ue")
        .replace("ß", "ss")
    )


def generate_email(givenname: str, surname: str) -> str:
    clean_givenname = "".join([c for c in replace_umlauts(givenname) if c.isalpha()])
    clean_surname = "".join([c for c in replace_umlauts(surname) if c.isalpha()])
    return f"{clean_givenname}.{clean_surname}@ovgu.de"


def handle_login(username: str, password: str) -> bool:
    client = MongoClient(MONGO_URI)
    try:
        collection = client.get_database("adminuser").get_collection("user")
        user = collection.find_one({"username": username})
        if user and "password" in user and isinstance(user["password"], (bytes, str)):
            stored_hash = user["password"]
            if isinstance(stored_hash, str):
                stored_hash = stored_hash.encode("utf-8")
            return bcrypt.checkpw(password.encode("utf-8"), stored_hash)
        return False
    except Exception as e:
        print(f"MongoDB Fehler: {e}")
        raise HTTPException(status_code=500, detail=f"Datenbankfehler: {e}")
    finally:
        client.close()
        print("MongoDB Verbindung geschlossen")


def handle_registration(user_data: Dict[str, Any]):
    client = MongoClient(MONGO_URI)
    try:
        collection = client.get_database("adminuser").get_collection("user")

        existing_user = collection.find_one({
            "username": str(user_data.get("username")),
            "surname": str(user_data.get("surname")),
            "email": str(user_data.get("email")),
            "gender": str(user_data.get("gender")),
            "right": str(user_data.get("right")),
        })
        if existing_user:
            raise HTTPException(status_code=400, detail="Benutzer existiert bereits")

        if not user_data.get("givenname") or not user_data.get("surname"):
            raise HTTPException(status_code=400, detail="Vorname und Nachname sind erforderlich")

        base_email = generate_email(str(user_data.get("givenname")), str(user_data.get("surname")))
        generated_email = base_email

        counter = 1
        while collection.find_one({"generatedEmail": str(generated_email)}):
            local_part = base_email.replace("@ovgu.de", "")
            generated_email = f"{local_part}{counter}@ovgu.de"
            counter += 1

        doc = {
            "username": str(user_data.get("givenname")),
            "surname": str(user_data.get("surname")),
            "email": str(user_data.get("email")),
            "generatedEmail": str(generated_email),
            "birthdate": str(user_data.get("birthdate")),
            "gender": str(user_data.get("gender")),
            "right": str(user_data.get("right")),
        }

        result = collection.insert_one(doc)
        print("Benutzer erfolgreich registriert")
        return {"inserted_id": str(result.inserted_id)}
    except HTTPException:
        raise
    except Exception as e:
        print(f"MongoDB Fehler: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        client.close()
        print("MongoDB Verbindung geschlossen")


def handle_mass_registration(mass: int, right: str, password: str, generate_email_flag: bool):
    client = MongoClient(MONGO_URI)
    try:
        collection = client.get_database("adminuser").get_collection("user")
        results: List[str] = []
        base_username = "gast"
        counter = 1
        for _ in range(int(mass)):
            username = f"{base_username}{counter}"
            email = f"{username}@ovgu.de" if generate_email_flag else None
            existing_user = collection.find_one({"username": username})
            while existing_user:
                counter += 1
                username = f"{base_username}{counter}"
                existing_user = collection.find_one({"username": username})

            password_hash = bcrypt.hashpw(password.encode("utf-8"), bcrypt.gensalt())

            doc = {
                "username": username,
                "password": password_hash,
                "right": str(right),
            }
            if email:
                doc["email"] = email

            result = collection.insert_one(doc)
            results.append(str(result.inserted_id))
            counter += 1

        print(f"{mass} Benutzer erfolgreich registriert")
        return results
    except Exception as e:
        print(f"MongoDB Fehler: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        client.close()
        print("MongoDB Verbindung geschlossen")


app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"]
)


@app.post("/api/login")
async def login(body: LoginBody):
    if not body.username or not body.password:
        raise HTTPException(status_code=400, detail="Benutzername und Passwort sind erforderlich")
    if handle_login(body.username, body.password):
        return {"success": True}
    raise HTTPException(status_code=401, detail="Ungültige Anmeldedaten")


@app.post("/api/register")
async def register(body: RegisterBody):
    if not body.givenname or not body.surname:
        raise HTTPException(status_code=400, detail="Alle Pflichtfelder müssen ausgefüllt werden")
    handle_registration(body.model_dump())
    return {"success": True, "message": "Benutzer erfolgreich registriert"}


@app.post("/api/massregister")
async def mass_register(body: MassRegisterBody):
    handle_mass_registration(
        body.mass,
        body.right,
        body.password or "Start123!",
        bool(body.generateEmail),
    )
    return {"success": True, "message": f"{body.mass} Benutzer erfolgreich registriert"}


# Start mit: uvicorn backend.index:app --reload

