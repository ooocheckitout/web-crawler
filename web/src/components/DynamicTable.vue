<template>
  <table class="table-auto w-full">
    <thead>
      <tr class="bg-stone-100">
        <th v-for="(key, keyIndex) in keys" :key="keyIndex" class="p-4 text-left">{{ key }}</th>
      </tr>
    </thead>
    <tbody>
      <tr v-if="objects.length == 0">
        No elements.
      </tr>
      <tr v-for="(object, objectIndex) in objects" :key="objectIndex" class="hover:bg-slate-100">
        <td
          v-for="(key, keyIndex) in keys"
          :key="keyIndex"
          class="p-4"
          @mouseover="mouseOverHandler(object)"
          @mouseout="mouseOutHandler(object)"
          @click="mouseClickHandler(object)"
        >
          <p>{{ getValue(key, object) }}</p>
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
      default: [],
    },
    columns: {
      type: Object,
      default: [],
    },
  },

  data() {
    return {
      keys: [],
    };
  },

  watch: {
    objects: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        if (this.columns.length != 0) {
          this.keys = this.columns;
        } else {
          this.keys = this.objects.flatMap(x => Object.keys(x)).uniqueBy(x => x);
        }
      },
    },
  },

  methods: {
    getValue(key, object) {
      return key.split(".").reduce((accumulator, key) => accumulator[key], object);
    },

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
