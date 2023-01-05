<template>
  <div class="flex flex-row">
    <div class="w-1/2">
      <pre>{{ datas }}</pre>
    </div>
    <div class="w-1/2">
      <pre>{{ transformed }}</pre>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      datas: [
        {
          property: "Property-1",
          values: [
            "MERCY",
            "MOIRA",
            "ZENYATTA",
            "JUNKRAT",
            "SYMMETRA",
            "SOJOURN",
            "ORISA",
            "DOOMFIST",
            "TORBJÖRN",
            "PHARAH",
            "ANA",
            "LÚCIO",
            "D.VA",
            "ECHO",
            "SIGMA",
            "ROADHOG",
            "HANZO",
            "SOLDIER: 76",
            "REAPER",
            "WIDOWMAKER",
            "GENJI",
            "ZARYA",
            "ASHE",
            "BASTION",
            "REINHARDT",
            "TRACER",
            "WRECKING BALL",
            "BAPTISTE",
            "CASSIDY",
            "BRIGITTE",
          ],
        },
        {
          property: "Property-2",
          values: [
            "04:18:41",
            "03:48:39",
            "02:06:07",
            "54:20",
            "46:35",
            "46:27",
            "44:09",
            "30:48",
            "28:01",
            "23:56",
            "23:47",
            "22:32",
            "15:36",
            "12:38",
            "12:09",
            "11:42",
            "11:42",
            "09:33",
            "08:12",
            "05:17",
            "05:10",
            "04:13",
            "02:42",
            "02:05",
            "01:51",
            "01:41",
            "01:25",
            "01:06",
            "01:02",
            "00:19",
          ],
        },
        {
          property: "Property-3",
          values: ["ezkatka"],
        },
      ],
      transformed: [],
    };
  },

  created() {
    console.log("datas", this.datas);

    let propertiesOfSameLength = this.datas
      .reduce((acc, data) => {
        let lengthKey = data.values.length;

        if (!acc[lengthKey]) acc[lengthKey] = [];

        acc[lengthKey].push(data);
        return acc;
      }, {});

    console.log("propertiesOfSameLength", propertiesOfSameLength);

    // var results = this.datas.map(x => {
    //   return {
    //     name: x.property,
    //     values: x.values,
    //     length: x.values.length,
    //     combineWith:
    //   };
    // });

    let properties = this.datas.map(x => x.property);

    let objects = Object.keys(propertiesOfSameLength).map(lengthKey => {
      let arrays = propertiesOfSameLength[lengthKey];
      let zippedValues = arrays.map(x => x.values).zip();

      return zippedValues.map(value => {
        return properties.reduce((acc, property, index) => {
          acc[property] = value[index];
          return acc;
        }, {});
      });
    });

    console.log("objects", objects);

    this.transformed = objects;

    // Object.keys(propertiesOfSameLength).map(lengthKey => {
    //   let arrays = propertiesOfSameLength[lengthKey];

    //   let objects = arrays.map(x => x.values).zip();

    //   for (const object of objects) {
    //     for (const property of properties) {
    //         object[property] =
    //     }
    //   }

    //   //   objects.map(object =>  )

    //   console.log();
    // });
  },
};
</script>
