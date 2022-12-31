<template>
  <!-- <img alt="Vue logo" src="./assets/logo.png">
  <HelloWorld msg="Welcome to Your Vue.js App"/> -->
  <Crawler />
</template>

<script>
import HelloWorld from "./components/HelloWorld.vue";
import Crawler from "./components/Crawler.vue";
import Viewer from "./components/Viewer.vue";

export default {
  name: "App",
  components: {
    HelloWorld,
    Crawler,
    Viewer
  },
};
</script>
