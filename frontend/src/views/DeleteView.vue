<script setup>
import Footer from "../components/Footer.vue";
import Header from '../components/Header.vue';
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
            <form name="delete-form" @submit.prevent="submitForm">  <!-- Änderung hier -->
                <div class="mb-3">
                    <label for="GivenName">Vorname: </label>
                    <input type="text" id="GivenName" required placeholder="Gib den Vornamen ein..." v-model="input.givenname" />
                </div>
                <div class="mb-3">
                    <label for="Surname">Nachname: </label>
                    <input type="text" id="Surname" required placeholder="Gib den Nachnamen ein..." v-model="input.surname" />
                </div>
                <div class="mb-3">
                    <label for="Email">Email: </label>
                    <input type= "email" id="email" required placeholder="Gebe die Mailadresse ein..." v-model="input.email" />
                </div>
                <button class="btn btn-outline-dark" type="submit">Nutzer löschen</button>  <!-- Änderung hier -->
            </form>
        </main>

        <footer>
            <Footer/>
        </footer>
    </body>
</template>



<script>
export default {
  name: 'DeleteView',
  data(){
    return{
      input:{
        givenname: "",
        surname: "",
        email: "",
        right:"",
      }
    }
  },

  methods: {
    async submitForm() {
      if(this.input.givenname && this.input.surname && this.input.email) {
        try {
          const response = await fetch('http://localhost:8080/api/delete', {
            method: 'DELETE',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({
              username: this.input.givenname,
              surname: this.input.surname,
              email: this.input.email,
            })
          });

          const data = await response.json();
          if (response.ok) {
            console.log("Benutzer erfolgreich gelöscht");
            // Formular zurücksetzen
            this.input = {
              givenname: "",
              surname: "",
              email: "",
              right:"",
            };
          } else {
            console.error("Löschung fehlgeschlagen:", data.error);
          }
        } catch (error) {
          console.error("Fehler bei der Löschung:", error);
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
