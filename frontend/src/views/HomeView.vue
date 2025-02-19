<script setup>
  import Footer from "../components/Footer.vue";
</script>



<template>
  <body>
    <header>
    </header>

    <article>
      <noscript>
        <strong>We're sorry but this site doesn't work properly without JavaScript enabled. Please enable it to continue.</strong>
      </noscript>
    </article>

    <main>
      <form name="login-form" @submit.prevent="login">
        <div class="mb-3">
          <label for="username">Username: </label>
          <input type="text" id="username" autofocus placeholder="Gebe deinen Username ein ..." required v-model="input.username" />
        </div>
        <div class="mb-3">
          <label for="password">Password: </label>
          <input :type="showPassword ? 'text' : 'password'" id="password" required placeholder="Gebe dein Password ein ..." v-model="input.password" />
          <button type="button" @click="togglePassword">{{ showPassword ? 'Password verstecken' : 'Password einblenden' }}</button>
        </div>
        <button class="btn btn-outline-dark" type="submit">Login</button>
        <div v-if="errorMessage" class="alert alert-danger">
          {{ errorMessage }}
        </div>
      </form>
    </main>

    <footer>
      <Footer />
    </footer>
  </body>
</template>



<script>
export default {
  name: 'LoginView',
  data() {
    return {
      input: {
        username: "",
        password: ""
      },
      errorMessage: "",
      showPassword: false
    }
  },

  methods: {
    togglePassword() {
      this.showPassword = !this.showPassword; // Toggle-Funktion
    },
    async login() {
        if(this.input.username !== "" && this.input.password !== "") {
            try {
                const response = await fetch('http://localhost:8080/api/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    credentials: 'include',
                    body: JSON.stringify({
                        username: this.input.username,
                        password: this.input.password
                    })
                });

                const data = await response.json();

                if (response.ok) {
                    console.log('Login erfolgreich:', data);
                    this.$router.push('/about'); // Hier ist die Weiterleitung
                } else {
                    this.errorMessage = data.error || 'Login fehlgeschlagen';
                }
            } catch (error) {
                console.error('Login error:', error);
                this.errorMessage = 'Ein Fehler ist aufgetreten';
            }
        } else {
            this.errorMessage = 'Benutzername und Passwort dürfen nicht leer sein';
        }
    }
  }
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
