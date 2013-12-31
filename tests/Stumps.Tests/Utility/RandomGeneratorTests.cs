﻿namespace Stumps.Utility {

    using NUnit.Framework;

    [TestFixture]
    public class RandomGeneratorTests {

        [Test]
        public void RandomGenerator_GeneratesValuesOfTheAppropriateSize() {

            var value = RandomGenerator.GenerateIdentifier();
            Assert.AreEqual(RandomGenerator.KeySize, value.Length);
            
        }

        [Test]
        public void RandomGenerator_GeneratesUniqueValues() {

            var value1 = RandomGenerator.GenerateIdentifier();
            var value2 = RandomGenerator.GenerateIdentifier();

            Assert.AreNotEqual(value1, value2);

        }


    }

}