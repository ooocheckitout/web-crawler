<template>
  <table class="table-auto w-full">
    <thead>
      <tr class="bg-stone-100">
        <th v-for="key in keys" :key="key" class="p-4 truncate text-left">{{ key }}</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="(object, index) in objects" :key="index">
        <td
          v-for="key in keys"
          :key="key"
          class="p-4 truncate"
          @mouseover="mouseOverHandler(object)"
          @mouseout="mouseOutHandler(object)"
          @click="mouseClickHandler(object)"
        >
          {{ object[key] }}
        </td>
      </tr>
    </tbody>
  </table>
</template>

<script>
export default {
  props: {
    objects: {
      type: Object,
      default: [{ test: 1, check: 2 }],
    },
  },

  watch: {
    data: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        if (!current) current = [];

        console.log("dynamic table", this.objects);
        this.keys = this.objects.flatMap(x => Object.keys(x)).uniqueBy(x => x);
      },
    },
  },
  methods: {
    mouseClickHandler(object) {
      this.$emit("itemClick", object);
    },
    mouseOverHandler(object) {
      this.$emit("itemMouseOver", object);
    },
    mouseOutHandler(object) {
      this.$emit("itemMouseOut", object);
    },
  },
};
</script>
