<template>
  <div>
    <!-- <Crawler /> -->
    <Update />
  </div>
</template>

<script>
import Crawler from "./components/Crawler.vue";
import Viewer from "./components/Viewer.vue";
import Update from "./components/Update.vue";

export default {
  name: "App",
  components: {
    Crawler,
    Viewer,
    Update
  },
};
</script>
