using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ecommerce.ApiClients.UnitTests
{
    [TestClass]
    public class HelpersTests
    {
        [TestMethod]
        public void Canonicalize_WithPlainValues_Test()
        {
            // Arrange
            string expectedResult = "filter=foo&display=bazinga";
            var options = new Dictionary<string, string>();
            options.Add("filter", "foo");
            options.Add("display", "bazinga");

            // Act
            string actualResult = Helpers.Canonicalize(options);

            // Assert
            Assert.AreEqual<string>(expectedResult, actualResult);
        }

        [TestMethod]
        public void Canonicalize_WithSpacesInValues_Test()
        {
            // Arrange
            string expectedResult = "filter=foo&display=baz+inga";
            var options = new Dictionary<string, string>();
            options.Add("filter", "foo");
            options.Add("display", "baz inga");

            // Act
            string actualResult = Helpers.Canonicalize(options);

            // Assert
            Assert.AreEqual<string>(expectedResult, actualResult);
        }
    }
}
