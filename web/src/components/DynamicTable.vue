<template>
  <table class="w-full table-auto">
    <caption>{{ caption }}</caption>
    <thead>
      <tr v-if="objects.length == 0">
        No elements.
      </tr>
      <tr v-else class="bg-stone-100 hover:bg-slate-200 cursor-copy" @click="copyHandler">
        <th v-for="(key, keyIndex) in displayColumns" :key="keyIndex" class="p-4 text-left">{{ key }}</th>
        <th>✂️</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="(object, objectIndex) in displayObjects" :key="objectIndex" class="hover:bg-slate-100">
        <td
          v-for="(key, keyIndex) in displayColumns"
          :key="keyIndex"
          class="p-4"
          @mouseover="mouseOverHandler(object)"
          @mouseout="mouseOutHandler(object)"
          @click="mouseClickHandler(object)"
        >
          {{ getValue(object, key) }}
        </td>
        <td></td>
      </tr>
      <tr v-if="objects.length > limit" >Showing only {{ limit }} results...</tr>
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
    limit: {
      type: Number,
      default: 100,
    },
    caption: {
      type: String,
      default: null,
    },
  },

  data() {
    return {
      displayColumns: [],
      displayObjects: [],
    };
  },

  watch: {
    objects: {
      immediate: true,
      deep: true,
      handler(current, previous) {
        let displayColumns = this.objects.flatMap(x => Object.keys(x)).uniqueBy(x => x);

        var asterixIndex = this.columns.indexOf("*");
        if (this.columns.length > 0 && asterixIndex == -1) {
          this.displayColumns = this.columns;
        }
        else {
          this.columns.splice(asterixIndex, 1);
          this.displayColumns = displayColumns.concat(this.columns);
        }
        
        this.displayObjects = this.objects.slice(0, this.limit);
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
