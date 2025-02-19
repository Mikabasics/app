<script setup>
import Header from '../components/Header.vue';
import Footer from "../components/Footer.vue";
import { defineExpose } from 'vue'; // Import hinzufügen

// Funktion zur Generierung einer neuen E-Mail-Adresse
const generateNewEmail = (givenname, surname) => {
    const randomSuffix = Math.floor(Math.random() * 1000); // Zufällige Zahl zwischen 0 und 999
    return `${givenname.toLowerCase()}.${surname.toLowerCase()}${randomSuffix}@ovgu.de`;
};

defineExpose({ generateNewEmail }); // Funktion exportieren
</script>



<template>
  <body>
    <header>
      <Header />
    </header>

    <article>
      <noscript>
        <strong>We're sorry but this site doesn't work properly without JavaScript enabled. Please enable it to continue.</strong>
      </noscript>
    </article>

    <main>
      <form name="insert-form" @submit.prevent="submitForm">
        <div class="mb-3">
            <label for="GivenName">Vorname: </label>
            <input type="text" id="GivenName" autofocus required placeholder="Gib den Vornamen ein..." v-model="input.givenname" />
          </div>
        <div class="mb-3">
          <label for="Surname">Nachname: </label>
          <input type="text" id="Surname" required placeholder="Gib den Nachnamen ein..." v-model="input.surname" />
        </div>
        <div class="mb-3">
          <label for="Birthdate">Geburtsdatum: </label>
          <input type="date" id="birthdate" required placeholder="Gib das Geburtsdatum ein..." v-model="input.birthdate" />
        </div>
        <div class="mb-3">
          <label for="Email">Private Emailadresse: </label>
          <input type= "email" id="email" required placeholder="Gebe deine private Mailadresse ein..." v-model="input.email" />
        </div>
        <div class="mb-3">
          <label for="Gender">Gender: </label><br>
          <select name="gender" id="gender" required v-model="input.gender" placeholder="Wähle das Geschlecht aus..">
            <option value="1">Männlich</option><option value="2">Weiblich</option><option value="3">Divers</option><option value="4">ohne Angabe</option>
          </select>
        </div>
        <div class="mb-3">
          <label for="Right">Rechte: </label><br>
          <select name="right" id="right" required v-model="input.right" placeholder="Wähle die Rechte aus...">
            <option>Student</option><option>Gast</option><option>Emeritiert</option><option>Sonstiges</option>
          </select>
          </div>
        <div class="mb-3">
          <label><input type="checkbox" name="checkbox" id="checkbox" v-model="input.generateEmail" />Neue E-Mail-Adresse generieren</label>
        </div>
        <button class="btn btn-outline-dark" type="submit">Registrieren</button>
      </form>
    </main>

    <footer>
      <Footer/>
    </footer>
  </body>
</template>



<script>
export default {
    name: 'AboutView',
    data(){
      return{
        input:{
          givenname: "",
          surname: "",
          birthdate: "",
          email: "",
          right:"",
          generateEmail: false,
        }
      }
    },

    methods: {
    async submitForm() {
        if(this.input.givenname && this.input.surname) {
            try {
                // Generiere neue E-Mail-Adresse, wenn das Kontrollkästchen aktiviert ist
                if (this.input.generateEmail) {
                    this.input.email = generateNewEmail(this.input.givenname, this.input.surname);
                }
                const response = await fetch('http://localhost:8080/api/register', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        username: this.input.givenname,
                        birthdate: this.input.birthdate,
                        surname: this.input.surname,
                        email: this.input.email,
                        gender: this.input.gender,
                        right: this.input.right,
                    })
                });

            const data = await response.json();
            if (response.ok) {
              console.log("Benutzer erfolgreich registriert");
              // Formular zurücksetzen
              this.input = {
                givenname: "",
                surname: "",
                birthdate: "",
                email: "",
                gender:"",
                right:"",
              };
            } else {
              console.error("Registrierung fehlgeschlagen:", data.error);
            }
          } catch (error) {
            console.error("Fehler bei der Registrierung:", error);
          }
        } else {
          console.log("Alle Felder müssen ausgefüllt werden");
        }
      }
    },
  }
</script>



<style scoped>
  input {
    border: 0;
    border-radius: 8px;
    width: 100%;
    height: 30px;
    justify-content: space-around;
  }
  button{
    background-color: #4a0031;
    border-radius: 8px;
    width: 100%;
    height: 30px;
    justify-content: space-around;
  }
</style>
