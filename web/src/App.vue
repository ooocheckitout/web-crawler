<template>
  <div>
    <Main />
  </div>
</template>

<script>
import Main from "./components/Main.vue";

export default {
  name: "App",
  components: {
    Main,
  },
};

Object.defineProperty(Array.prototype, "toMap", {
  value: function (keyFunc, valueFunc) {
    return this.reduce((map, val) => {
      map[keyFunc(val)] = valueFunc(val);
      return map;
    }, {});
  },
  writable: true,
  configurable: true,
});

Object.defineProperty(Array.prototype, "zip", {
  value: function () {
    if (this.some(x => !x))
      return;

    let arr = this;
    var length = Math.max(...arr.map((a) => a.length));
    return Array(length)
      .fill()
      .map((_, i) => arr.map((a) => a[i]));
  },
  writable: true,
  configurable: true,
});

Object.defineProperty(Array.prototype, "uniqueBy", {
  value: function (propFunc) {
    return this.filter((val, index, self) => {
      var found = self.find((x) => propFunc(x) === propFunc(val));
      var foundIndex = self.indexOf(found);
      return foundIndex == index;
    });
  },
  writable: true,
  configurable: true,
});

Object.defineProperty(Array.prototype, "compare", {
  value: function (arr) {
    if (!this || !arr)
      return;

    let lengthComparison = this.length === arr.length;
    let elementComparison = this.every(
      (inner, index) => JSON.stringify(inner) === JSON.stringify(arr[index])
    );

    return lengthComparison && elementComparison
  },
  writable: true,
  configurable: true,
});
</script>
