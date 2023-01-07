export default {
    group(properties) {
        let numberOfElements = Math.max(...properties.map(x => x.values.length));

        let objects = []
        for (let index = 0; index < numberOfElements; index++) {
            let object = properties.reduce((accumulator, property) => {
                accumulator[property.name] = property.values[index];
                return accumulator;
            }, {})
            objects.push(object)
        }

        return objects;
    },

    partition(objects, property) {
        let numberOfPartitions = property.values.length;
        let numberOfElementsInPartition = objects.length / numberOfPartitions;

        let partitions = []
        for (let index = 0; index < numberOfPartitions; index++) {
            let from = index * numberOfElementsInPartition;
            let to = from + numberOfElementsInPartition;

            partitions.push({ key: property.name, value: property.values[index], objects: objects.slice(from, to) })
        }

        return partitions;
    }
}