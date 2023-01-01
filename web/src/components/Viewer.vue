<template lang="">
    <!-- <div v-html="html"></div> -->
    <iframe :src="url" frameborder="0" class="w-full h-full"></iframe>
</template>
<script>
export default {
  data() {
    return {
      html: undefined,
    };
  },
  props: {
    url: {
      type: String,
      default: "https://tailwindcss.com",
    },
  },
  watch: {
    url: {
      immediate: true,
      async handler() {
        console.log(this.url);
        let response = await fetch(this.url);
        this.html = await response.text();
      },
    },
  },
};
</script>