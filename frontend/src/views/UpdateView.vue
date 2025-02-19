<script setup>
import Header from '../components/Header.vue';
import Footer from "../components/Footer.vue";
import { ref } from 'vue';

const input = ref({
  givenname: "",
  surname: "",
  password: "",
  email: "",
  right: "",
  gender: ""
});
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
          <input type="text" id="Surname" placeholder="Gib den Nachnamen ein..." required v-model="input.surname" />
        </div>
        <div class="mb-3">
          <label for="Password">Password: </label>
          <input type="password" id="password" placeholder="Gib das Password ein..." required v-model="input.password" />
        </div>
        <div class="mb-3">
          <label for="Email">Email: </label>
          <input type= "email" id="email" required placeholder="Gebe die Mailadresse ein..." v-model="input.email" />
        </div>
        <div class="mb-3">
          <label for="Gender">Gender: </label><br>
          <select name="gender" id="gender" v-model="input.gender" placeholder="Wähle das Geschlecht aus..">
            <option>Männlich</option><option>Weiblich</option><option>Divers</option><option>ohne Angabe</option>
          </select>
        </div>
        <div class="mb-3">
          <label for="Right">Rechte: </label><br>
          <select name="right" id="right" v-model="input.right" placeholder="Wähle die Rechte aus...">
            <option>Mitarbeiter</option><option>Student</option><option>Gast</option><option>keine Angabe</option>
          </select>
          </div>
          <button class="btn btn-outline-dark" type="submit">Änderung speichern</button>
        </form>
    </main>

    <footer>
      <Footer/>
    </footer>
  </body>
</template>



<script>
export default {
    name: 'UpdateView',
    data(){
      return{
        input:{
          givenname: "",
          surname: "",
          password: "",
          email: "",
          right:"",
        }
      }
    },

    methods: {
      async submitForm() {
        if(this.input.givenname && this.input.surname) {
          try {
            const response = await fetch('http://localhost:8080/api/update', {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                username: this.input.givenname, // Benutzername für die Aktualisierung
                surname: this.input.surname,
                email: this.input.email,
                gender: this.input.gender,
                right: this.input.right,
              })
            });

            const data = await response.json();
            if (response.ok) {
              console.log("Benutzer erfolgreich aktualisiert");
              // Formular zurücksetzen
              this.input = {
                givenname: "",
                surname: "",
                password: "",
                email: "",
                gender: "",
                right: "",
              };
            } else {
              console.error("Aktualisierung fehlgeschlagen:", data.error);
            }
          } catch (error) {
            console.error("Fehler bei der Aktualisierung:", error);
          }
        } else {
          console.log("Vorname und Nachname müssen ausgefüllt werden");
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
