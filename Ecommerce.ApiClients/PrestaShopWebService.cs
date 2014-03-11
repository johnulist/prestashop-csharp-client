using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Ecommerce.Prestashop
{
    public class PrestaShopWebserviceException : Exception
    {
        public PrestaShopWebserviceException() { }
        public PrestaShopWebserviceException(string message) : base(message) { }
        public PrestaShopWebserviceException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RequestResponse
    {
        public RequestResponse(int code, string header, string data)
        {
            this.Code = code;
            this.Header = header;
            this.Data = data;
        }

        public int Code { get; set; }
        public string Header { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// Instantiate the PrestaShopWebService to start executing operations against the PrestaShop Web Service
    /// </summary>
    public class PrestaShopWebService
    {
        private readonly string apiUrl;
        private readonly string apiKey;
        private readonly bool debug;

        // Versions of the PrestaShop Web Service supported by this client library
        private readonly Version MIN_COMPATIBLE_VERSION;
        private readonly Version MAX_COMPATIBLE_VERSION;

        // For URL encoding
        private readonly Encoding ENCODING;

        public PrestaShopWebService(string apiUrl, string apiKey, bool debug = true)
        {
            this.MIN_COMPATIBLE_VERSION = new Version("1.4.0.17");
            this.MAX_COMPATIBLE_VERSION = new Version("1.6.0.4");   // 1.6.0.4 is PrestaShop 1.6 RC

            this.ENCODING = Encoding.UTF8;

            this.apiUrl = MakeValidApiUrl(apiUrl);
            this.apiKey = apiKey;
            this.debug = debug;
        }

        /// <summary>
        /// Add a slash to the url if it does not have it
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Url with slash at the end</returns>
        private string MakeValidApiUrl(string url)
        {
            if (url[url.Length - 1] != '/')
            {
                url += '/';
            }

            return url;
        }

        /// <summary>
        /// Take the status code and throw an exception if the server didn't return 200 or 201 code
        /// </summary>
        /// <param name="statusCode">Status code of an HTTP return</param>
        private int CheckStatusCode(HttpStatusCode statusCode)
        {
            string errorLabel = "This call to PrestaShop Web Services failed and returned an HTTP status of {0}. That means: {1}.";

            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    return 200;
                case HttpStatusCode.Created:
                    return 201;
                case HttpStatusCode.NoContent:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 204, "No content"));
                case HttpStatusCode.BadRequest:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 400, "Bad Request"));
                case HttpStatusCode.Unauthorized:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 401, "Unauthorized"));
                case HttpStatusCode.NotFound:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 404, "Not Found"));
                case HttpStatusCode.MethodNotAllowed:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 405, "Method Not Allowed"));
                case HttpStatusCode.InternalServerError:
                    throw new PrestaShopWebserviceException(String.Format(errorLabel, 500, "Internal Server Error"));
                default:
                    throw new PrestaShopWebserviceException(String.Format("This call to PrestaShop Web Services returned an unexpected HTTP status of: {0}", statusCode));
            }
        }

        /// <summary>
        /// Take the version and throw an exception if it does not conform to compatible version
        /// </summary>
        /// <param name="version">Version from HTTP header</param>
        private void CheckVersion(Version version)
        {
            if (version.CompareTo(MIN_COMPATIBLE_VERSION) < 0 || version.CompareTo(MAX_COMPATIBLE_VERSION) > 0)
            {
                throw new PrestaShopWebserviceException("This library is not compatible with this version of PrestaShop. Please upgrade/downgrade this library");
            }
        }

        /// <summary>
        /// Execute a request on the PrestaShop Webservice
        /// </summary>
        /// <param name="url">Full url to call</param>
        /// <param name="method">GET, POST, PUT, DELETE</param>
        /// <param name="document">For PUT (edit) and POST (add) only, the xml sent to PrestaShop</param>
        /// <returns>RequestResponse with statuscode, header and data</returns>
        private async Task<RequestResponse> Execute(string url, string method, XDocument document = null)
        {
            int statusCode = 0;
            string header = String.Empty;
            string data = String.Empty;

            //if (this.debug)
            //{
            //    Console.WriteLine("Request method: {1}", method);
            //}

            using (var handler = new HttpClientHandler { Credentials = new NetworkCredential(this.apiKey, "") })
            using (var client = new HttpClient(handler))
            {
                HttpResponseMessage response;
                HttpContent content;

                try 
                { 
                    switch (method.ToUpper())
                    {
                        case "GET":
                            response = await client.GetAsync(url);
                            break;
                        case "POST":
                            response = await client.PostAsync(url, new StringContent(document.ToString(), ENCODING, "text/xml"));
                            break;
                        case "PUT":
                            response = await client.PutAsync(url, new StringContent(document.ToString(), ENCODING, "text/xml"));
                            break;
                        case "DELETE":
                            response = await client.DeleteAsync(url);
                            break;
                        case "HEAD":
                            throw new PrestaShopWebserviceException("Http method 'HEAD' is not yet supported");
                        default:
                            throw new PrestaShopWebserviceException("Invalid Http Method provided. GET, POST, PUT, DELETE are valid");
                    }
                }
                catch (HttpRequestException)
                {
                    throw new PrestaShopWebserviceException("An error occured while sending the request,");
                }

                statusCode = CheckStatusCode(response.StatusCode);
                header = response.Headers.ToString() + "\n";

                if (response != null)
                {
                    content = response.Content;
                    data = await content.ReadAsStringAsync();
                }

                List<string> versionHeaders = response.Headers.GetValues("PSWS-Version").ToList();
                if (versionHeaders.Count == 1)
                {
                    Version version = Version.Parse(versionHeaders[0]);
                    CheckVersion(version);
                }

                response.Dispose();
            }

            //if (this.debug)
            //{
            //    Console.WriteLine("Response code: {0}\nResponse headers:\n{1}\nResponse body:\n{2}", statusCode, header, data);
            //}

            return new RequestResponse(statusCode, header, data);
        }

        /// <summary>
        /// Loads an XML into an Elem from a String
        /// Throws an exception if there is no XML or it won't validate
        /// </summary>
        /// <param name="xml">The XML string to parse</param>
        /// <returns>The parsed XML in an XElement ready to work with</returns>
        private XElement Parse(string xml)
        {
            XDocument xdoc;

            if (String.IsNullOrEmpty(xml))
            {
                throw new PrestaShopWebserviceException("HTTP XML response was empty");
            }

            try 
            { 
                xdoc = XDocument.Parse(xml);

                //if (this.debug)
                //{
                //    Console.WriteLine("Parsed XML:\n{0}", xdoc.ToString());
                //}
            }
            catch (Exception e) 
            {
                throw new PrestaShopWebserviceException("HTTP XML response is not parsable: " + e.Message);
            }

            return xdoc.Descendants("prestashop").Single();
        }

        /// <summary>
        /// Validates that the parameters are all either 'filter', 'display', 'sort', 'limit' or 'schema'
        /// Strictly schema isn't a permitted param for HEAD (only GET) but let's leave it
        /// Throws a PrestaShopWebServiceException if not
        /// </summary>
        /// <param name="options">Options to validate</param>
        /// <returns>The original parameters if everything is okay</returns>
        private Dictionary<string, string> Validate(Dictionary<string, string> options)
        {
            string[] validOptions = { "display", "sort", "limit", "schema" };

            foreach (var option in options.Keys)
            {
                if (!((IList<string>)validOptions).Contains(option) && option.Substring(0, 6) != "filter") 
                {
                    throw new PrestaShopWebserviceException(String.Format("Parameter {0} is not supported", option));
                }
            }

            return options;
        }

        /// <summary>
        /// Add (POST) a resource, self-assembly version
        /// </summary>
        /// <param name="resource">Type of resource to add</param>
        /// <param name="xml">Full XML of new resource</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> Add(string resource, XElement xml)
        {
            string url = this.apiUrl + resource;
            return await AddWithUrl(url, xml);
        }

        /// <summary>
        /// Add (POST) a resource, URL version
        /// </summary>
        /// <param name="url">Full URL for a POST request to the Web Service</param>
        /// <param name="xml">Full XML of new resource</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> AddWithUrl(string url, XElement xml)
        {
            //if (this.debug)
            //{
            //    Console.WriteLine(url);
            //}

            RequestResponse response = await Execute(url, "POST", xml.Document);
            return Parse(response.Data);
        }

        /// <summary>
        /// Retrieve (GET) a resource, self-assembly version with parameters
        /// </summary>
        /// <param name="resource">Type of resource to retrieve</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> Get(string resource)
        {
            return await Get(resource, null, null);
        }

        /// <summary>
        /// Retrieve (GET) a resource, self-assembly version without parameters
        /// </summary>
        /// <param name="resource">Type of resource to retrieve</param>
        /// <param name="id">Resource ID to retrieve</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> Get(string resource, int id)
        {
            return await Get(resource, id, null);
        }

        /// <summary>
        /// Retrieve (GET) a resource, self-assembly version with parameters
        /// </summary>
        /// <param name="resource">Type of resource to retrieve</param>
        /// <param name="options">Dictionary of options (one or more of 'filter', 'display', 'sort', 'limit')</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> Get(string resource, Dictionary<string, string> options)
        {
            XElement returnValue = await Get(resource, null, options);

            return returnValue;
        }

        /// <summary>
        /// Retrieve (GET) a resource, self-assembly version with parameters
        /// </summary>
        /// <param name="resource">Type of resource to retrieve</param>
        /// <param name="id">Resource ID to retrieve</param>
        /// <param name="options">Dictionary of options (one or more of 'filter', 'display', 'sort', 'limit')</param>
        /// <returns>XML response from Web Service</returns>
        public async Task<XElement> Get(string resource, int? id, Dictionary<string, string> options)
        {
            string url = this.apiUrl + resource;

            if (id != null)
            {
                url += "/" + id.ToString();
            }

            if (options != null)
            {
                url += String.Format("?{0}", Helpers.Canonicalize(Validate(options)));
            }

            XElement returnValue = await GetWithUrl(url);

            return returnValue;
        }

        /// <summary>
        /// Retrieve (GET) a resource, URL version
        /// </summary>
        /// <param name="url">A URL which explicitly sets the resource type and ID to retrieve</param>
        /// <returns>XML response from the Web Service</returns>
        public async Task<XElement> GetWithUrl(string url)
        {
            //if (this.debug)
            //{
            //    Console.WriteLine(url);
            //}

            RequestResponse response = await Execute(url, "GET");            
            return Parse(response.Data);
        }

        /// <summary>
        /// Head (HEAD) all resources of a type, self-assembly version
        /// </summary>
        /// <param name="resource">Type of resource to head</param>
        /// <returns>Header from Web Service's response</returns>
        public async Task<string> Head(string resource)
        {
            return await Head(resource, null, null);
        }

        /// <summary>
        /// Head (HEAD) an individual resource, self-assembly version
        /// </summary>
        /// <param name="resource">Type of resource to head</param>
        /// <param name="id">Resource ID to head (if not provided, head all resources of this type)</param>
        /// <returns>Header from Web Service's response</returns>
        public async Task<string> Head(string resource, int id)
        {
            return await Head(resource, id, null);
        }

        /// <summary>
        /// Head (HEAD) an individual resource or all resources of a type with possible parameters
        /// </summary>
        /// <param name="resource">Type of resource to head</param>
        /// <param name="id">Optional resource ID to head (if not provided, head all resources of this type)</param>
        /// <param name="options">Optional Dictionary of parameters (one or more of 'filter', 'display', 'sort', 'limit')</param>
        /// <returns>Header from Web Service's response</returns>
        public async Task<string> Head(string resource, int? id, Dictionary<string, string> options)
        {
            string url = this.apiUrl + resource;

            if (id != null)
            {
                url += "/" + id.ToString();
            }

            if (options != null)
            {
                url += String.Format("?{0}", Helpers.Canonicalize(Validate(options)));
            }

            return await HeadWithUrl(url);
        }

        /// <summary>
        /// Head (HEAD) an individual resource or all resources of a type, URL version
        /// </summary>
        /// <param name="url">Full URL for the HEAD request to the Web Service</param>
        /// <returns>Header from Web Service's response</returns>
        public async Task<string> HeadWithUrl(string url)
        {
            //if (this.debug)
            //{
            //    Console.WriteLine(url);
            //}

            RequestResponse response = await Execute(url, "HEAD");
            return response.Header;
        }

        /// <summary>
        /// Edit (PUT) a resource, self-assembly version
        /// </summary>
        /// <param name="resource">Type of resource to update</param>
        /// <param name="id">Resource ID to update</param>
        /// <param name="xml">Modified XML of the resource</param>
        public async Task<XElement> Edit(string resource, int id, XElement xml)
        {
            string url = this.apiUrl + resource + "/" + id.ToString();
            return await EditWithUrl(url, xml);
        }

        /// <summary>
        /// Edit (PUT) a resource, URL version
        /// </summary>
        /// <param name="url">A URL which explicitly sets the resource type and ID to edit</param>
        /// <param name="xml">Modified XML of the resource</param>
        public async Task<XElement> EditWithUrl(string url, XElement xml)
        {
            //if (this.debug)
            //{
            //    Console.WriteLine(url);
            //}

            RequestResponse response = await Execute(url, "PUT", xml.Document);
            return Parse(response.Data);
        }

        /// <summary>
        /// Delete (DELETE) a resource, self-assembly version supporting one ID
        /// This version takes a resource type and a single ID to delete
        /// </summary>
        /// <param name="resource">The type of resource to delete (e.g. "orders")</param>
        /// <param name="id">An ID of this resource type, to delete</param>
        public async Task Delete(string resource, int id)
        {
            string url = this.apiUrl + resource + String.Format("/?id={0}", id);
            await DeleteWithUrl(url);
        }

        /// <summary>
        /// Delete (DELETE) a resource, self-assembly version supporting multiple IDs
        /// This version takes a resource type and an array of IDs to delete
        /// </summary>
        /// <param name="resource">The type of resource to delete (e.g. "orders")</param>
        /// <param name="ids">An array of IDs of this resource type, to delete</param>
        public async Task Delete(string resource, int[] ids)
        {
            string url = this.apiUrl + resource + String.Format("/?id=[{0}]", String.Join(",", ids));
            await DeleteWithUrl(url);
        }

        /// <summary>
        /// Delete (DELETE) a resource, URL version
        /// </summary>
        /// <param name="url">A URL which explicitly sets resource type and resource ID</param>
        public async Task DeleteWithUrl(string url)
        {
            //if (this.debug)
            //{
            //    Console.WriteLine(url);
            //}

            await Execute(url, "DELETE");
        }
    }
}
