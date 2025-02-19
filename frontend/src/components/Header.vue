<template>
  <header>
    <nav>
        <RouterLink to="/about">Nutzer einfügen</RouterLink>
        <br>
        <RouterLink to="/delete">Nutzer löschen</RouterLink>
        <br>
        <RouterLink to="/update">Nutzer bearbeiten</RouterLink>
        <br>
        <RouterLink to="/">Logout</RouterLink>
    </nav>
  </header>
</template>


<script setup>
</script>



<style scoped>
  header {
    padding: 10%;
    display: flex;
    justify-content: space-around;
    align-items: center; /* Zentriert die Elemente vertikal */
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1); 
    border-radius: 8px;
  }

  nav a {
    text-decoration: none; 
    color: #fff;
    font-weight: bold; 
    transition: color 0.3s; /* Sanfter Übergang für Hover-Effekte */
  }

  nav a:hover {
    color: #4a0031; /* Dunklere Farbe beim Hover */
  }
</style>