<template>
  <table class="w-full table-auto">
    <thead>
      <tr v-if="objects.length == 0">
        No elements.
      </tr>
      <tr v-else class="bg-stone-100 hover:bg-slate-200 cursor-copy" @click="copyHandler">
        <th v-for="(key, keyIndex) in keys" :key="keyIndex" class="p-4 text-left">{{ key }}</th>
        <th>✂️</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="(object, objectIndex) in objects" :key="objectIndex" class="hover:bg-slate-100">
        <td
          v-for="(key, keyIndex) in keys"
          :key="keyIndex"
          class="p-4"
          @mouseover="mouseOverHandler(object)"
          @mouseout="mouseOutHandler(object)"
          @click="mouseClickHandler(object)"
        >
          <p>{{ getValue(object, key) }}</p>
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
        let columns = this.objects.flatMap(x => Object.keys(x)).uniqueBy(x => x);

        if (this.columns.length != 0) {
          var index = this.columns.indexOf("*");
          if (index != -1) {
            this.columns.splice(index, 1);
            columns = columns.concat(this.columns);
          } else {
            columns = this.columns;
          }
        }

        this.keys = columns;
      },
    },
  },

  methods: {
    getValue(object, key) {
      return key.split(".").reduce((accumulator, key) => {
        if (!accumulator || !accumulator.hasOwnProperty(key)) return null;
        return accumulator[key];
      }, object);
    },

    async copyHandler() {
      var objects = this.objects.map(object => {
        return this.keys.reduce((acc, key) => {
          acc[key] = this.getValue(object, key);
          return acc;
        }, {});
      });

      await navigator.clipboard.writeText(JSON.stringify(objects, null, 2));
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
