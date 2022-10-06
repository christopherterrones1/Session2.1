using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace AssignmentPetAPI
{
    [TestClass]
    public class HttpClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string UsersEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{UsersEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data

            Category category = new Category();
            category.Id = 1;
            category.Name = "Dog";

            Category tag = new Category();
            tag.Id = 20;
            tag.Name = "Siberian Husky";

            List<string> photoUrls = new List<string>();
            photoUrls.Add("www.google.com");

            List<Category> tags = new List<Category>();
            tags.Add(tag);

            // Create Json Object
            PetModel userData = new PetModel()
            {
                Id = 71020223,
                Category = category,
                Name = "Saber",
                PhotoUrls = photoUrls,
                Tags = tags,
                Status = "Available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(userData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            var httpResponse = await httpClient.PostAsync(GetURL(UsersEndpoint), postRequest);

            var listUserData = JsonConvert.DeserializeObject<PetModel>(httpResponse.Content.ReadAsStringAsync().Result);

            var statusCode = httpResponse.StatusCode;
            #endregion

            #region cleanup data

            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");

            #endregion

            #region get Username of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{userData.Id}"));

            // Deserialize Content
            listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var createdUserData = listUserData.Name;

            #endregion

            #region send put request to update data

            // Update value of userData
            userData = new PetModel()
            {
                Id = listUserData.Id,
                Category = listUserData.Category,
                Name = "Saber.put.updated",
                PhotoUrls = listUserData.PhotoUrls,
                Tags = listUserData.Tags,
                Status = listUserData.Status
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(userData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            httpResponse = await httpClient.PutAsync(GetURL($"{UsersEndpoint}/"), postRequest);

            // Get Status Code
            statusCode = httpResponse.StatusCode;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{userData.Id}"));

            // Deserialize Content
            listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            createdUserData = listUserData.Name;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");
            Assert.AreEqual(userData.Name, createdUserData, "Pet's name not matching");

            #endregion

        }
    }
}
