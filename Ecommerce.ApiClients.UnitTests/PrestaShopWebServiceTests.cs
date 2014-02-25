using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.ApiClients;

namespace Ecommerce.ApiClients.UnitTests
{
    [TestClass]
    public class PrestaShopWebServiceTests
    {
        [TestMethod]
        public void MakeValidApiUrl_InputWithSlash_Test()
        {
            // Arrange
            string apiUrl = @"http://example.test.com/api/";
            string apiKey = String.Empty;
            var webService = new PrestaShopWebService(apiUrl, apiKey);
            var privateWebService = new PrivateObject(webService);

            // Act
            string actualResult = (string)privateWebService.Invoke("MakeValidApiUrl", apiUrl);

            // Assert
            Assert.AreEqual(apiUrl, actualResult);
        }

        [TestMethod]
        public void MakeValidApiUrl_InputWithoutSlash_Test()
        {
            // Arrange
            string apiUrl = @"http://example.test.com/api";
            string apiKey = String.Empty;
            var webService = new PrestaShopWebService(apiUrl, apiKey);
            var privateWebService = new PrivateObject(webService);

            // Act
            string actualResult = (string)privateWebService.Invoke("MakeValidApiUrl", apiUrl);

            // Assert
            Assert.AreEqual(apiUrl + "/", actualResult);
        }
    }
}
