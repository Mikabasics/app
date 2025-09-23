<?php
declare(strict_types=1);

// Einfache PHP-API als Ersatz für index.groovy (Javalin)
// Abhängigkeiten: PHP >=8.1, ext-mongodb, ext-json

// --- Konfiguration ---
$mongoUri ='';
$dbName = '';
$userCollection = 'user';

// --- CORS ---
header('Access-Control-Allow-Origin: http://localhost:5173');
header('Access-Control-Allow-Credentials: true');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

// --- Hilfsfunktionen ---
function send_json(array $data, int $status = 200): void {
    http_response_code($status);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode($data, JSON_UNESCAPED_UNICODE);
}

function read_json_body(): array {
    $raw = file_get_contents('php://input');
    if ($raw === false || $raw === '') {
        return [];
    }
    $data = json_decode($raw, true);
    return is_array($data) ? $data : [];
}

function get_mongo_client(string $uri) {
    try {
        $class = 'MongoDB\\Client';
        if (!class_exists($class)) {
            throw new RuntimeException('MongoDB Client-Bibliothek fehlt. Bitte `composer require mongodb/mongodb` ausführen.');
        }
        return new $class($uri);
    } catch (Throwable $e) {
        error_log('MongoDB Fehler: ' . $e->getMessage());
        throw new RuntimeException('Datenbankfehler: ' . $e->getMessage());
    }
}

function replace_umlauts(string $input): string {
    $lower = mb_strtolower($input, 'UTF-8');
    $replaced = strtr($lower, [
        'ä' => 'ae', 'ö' => 'oe', 'ü' => 'ue', 'ß' => 'ss',
    ]);
    return preg_replace('/[^a-zA-Z]/', '', $replaced) ?? '';
}

function generate_email(string $givenname, string $surname): string {
    $cleanGiven = replace_umlauts($givenname);
    $cleanSur = replace_umlauts($surname);
    return $cleanGiven . '.' . $cleanSur . '@ovgu.de';
}

// --- Domain-Funktionen ---
function handle_login(string $mongoUri, string $username, string $password): bool {
    $client = get_mongo_client($mongoUri);
    try {
        $collection = $client->selectCollection('adminuser', 'user');
        $user = $collection->findOne(['username' => $username]);
        if ($user === null) {
            error_log('Benutzerabfrage durchgeführt: Benutzer nicht gefunden');
            return false;
        }
        $doc = (array)$user;
        $hash = $doc['password'] ?? '';
        $ok = is_string($hash) && password_verify($password, $hash);
        if ($ok) {
            error_log('Benutzerabfrage durchgeführt: Benutzer gefunden');
            return true;
        }
        error_log('Benutzerabfrage durchgeführt: Passwort falsch');
        return false;
    } catch (Throwable $e) {
        error_log('MongoDB Fehler: ' . $e->getMessage());
        throw new RuntimeException('Datenbankfehler: ' . $e->getMessage());
    }
}

function handle_registration(string $mongoUri, array $userData): array {
    $client = get_mongo_client($mongoUri);
    try {
        $collection = $client->selectCollection('adminuser', 'user');

        $given = (string)($userData['givenname'] ?? '');
        $sur = (string)($userData['surname'] ?? '');
        $email = (string)($userData['email'] ?? '');
        $birthdate = (string)($userData['birthdate'] ?? '');
        $gender = (string)($userData['gender'] ?? '');
        $right = (string)($userData['right'] ?? '');

        if ($given === '' || $sur === '') {
            throw new InvalidArgumentException('Vorname und Nachname sind erforderlich');
        }

        $existing = $collection->findOne([
            'username' => (string)$userData['username'] ?? $given,
            'surname' => $sur,
            'email' => $email,
            'gender' => $gender,
            'right' => $right,
        ]);
        if ($existing !== null) {
            throw new RuntimeException('Benutzer existiert bereits');
        }

        $baseEmail = generate_email($given, $sur);
        $generatedEmail = $baseEmail;
        $counter = 1;
        while ($collection->findOne(['generatedEmail' => $generatedEmail]) !== null) {
            $local = substr($baseEmail, 0, -strlen('@ovgu.de'));
            $generatedEmail = $local . $counter . '@ovgu.de';
            $counter++;
        }

        $doc = [
            'username' => $given,
            'surname' => $sur,
            'email' => $email,
            'generatedEmail' => $generatedEmail,
            'birthdate' => $birthdate,
            'gender' => $gender,
            'right' => $right,
        ];
        $result = $collection->insertOne($doc);
        error_log('Benutzer erfolgreich registriert');
        return ['insertedId' => (string)$result->getInsertedId()];
    } catch (Throwable $e) {
        error_log('MongoDB Fehler: ' . $e->getMessage());
        throw $e;
    }
}

function handle_mass_registration(string $mongoUri, int $mass, string $right, string $password, bool $generateEmail): array {
    $client = get_mongo_client($mongoUri);
    try {
        $collection = $client->selectCollection('adminuser', 'user');
        $results = [];
        $baseUsername = 'gast';
        $counter = 1;
        for ($i = 0; $i < $mass; $i++) {
            $username = $baseUsername . $counter;
            $email = $generateEmail ? ($username . '@ovgu.de') : null;
            $existing = $collection->findOne(['username' => $username]);
            while ($existing !== null) {
                $counter++;
                $username = $baseUsername . $counter;
                $existing = $collection->findOne(['username' => $username]);
            }
            $doc = [
                'username' => $username,
                'password' => password_hash($password, PASSWORD_BCRYPT),
                'right' => $right,
            ];
            if ($email !== null) {
                $doc['email'] = $email;
            }
            $res = $collection->insertOne($doc);
            $results[] = (string)$res->getInsertedId();
            $counter++;
        }
        error_log($mass . ' Benutzer erfolgreich registriert');
        return $results;
    } catch (Throwable $e) {
        error_log('MongoDB Fehler: ' . $e->getMessage());
        throw $e;
    }
}

// --- Router ---
$path = parse_url($_SERVER['REQUEST_URI'] ?? '/', PHP_URL_PATH) ?: '/';
$method = $_SERVER['REQUEST_METHOD'] ?? 'GET';

try {
    if ($method === 'POST' && $path === '/api/login') {
        $body = read_json_body();
        $username = (string)($body['username'] ?? '');
        $password = (string)($body['password'] ?? '');
        if ($username === '' || $password === '') {
            send_json(['error' => 'Benutzername und Passwort sind erforderlich'], 400);
            exit;
        }
        $ok = handle_login($mongoUri, $username, $password);
        if ($ok) {
            send_json(['success' => true]);
        } else {
            send_json(['error' => 'Ungültige Anmeldedaten'], 401);
        }
        exit;
    }

    if ($method === 'POST' && $path === '/api/register') {
        $userData = read_json_body();
        if (empty($userData['givenname']) || empty($userData['surname'])) {
            send_json(['error' => 'Alle Pflichtfelder müssen ausgefüllt werden'], 400);
            exit;
        }
        handle_registration($mongoUri, $userData);
        send_json(['success' => true, 'message' => 'Benutzer erfolgreich registriert']);
        exit;
    }

    if ($method === 'POST' && $path === '/api/massregister') {
        $body = read_json_body();
        if (!isset($body['mass']) || !isset($body['right'])) {
            send_json(['error' => 'Menge und Rechte sind erforderlich'], 400);
            exit;
        }
        $mass = (int)$body['mass'];
        $right = (string)$body['right'];
        $password = (string)($body['password'] ?? 'Start123!');
        $genEmail = (bool)($body['generateEmail'] ?? false);
        handle_mass_registration($mongoUri, $mass, $right, $password, $genEmail);
        send_json(['success' => true, 'message' => $mass . ' Benutzer erfolgreich registriert']);
        exit;
    }

    // Fallback
    send_json(['error' => 'Not Found'], 404);
} catch (Throwable $e) {
    send_json(['error' => 'Interner Server-Fehler: ' . $e->getMessage()], 500);
}


