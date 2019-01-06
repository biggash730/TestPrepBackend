using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using RestSharp;
using TaxPayCoAPI.DataAccess.Repositories;
using TaxPayCoAPI.Models;

namespace TaxPayCoAPI.AxHelpers
{
    public class GviveHelpers
    {
        //private const string Username = "someusername";
        //private const string ApiKey = "QnXWfHirEJ4SykMUj0zVzZB7Cw2WsSicdVLiHnA2k7g";
        private const string Username = "L_Sqaure";
        private const string ApiKey = "vK9FIt+vAgqrsgTPbwZ8BnRCuhrT+7LpxAq4mw8NK7A=";
        private const string BaseUrl = "https://gvivegh.com:1355/gvivewar";

        public static string HmacSha256Digest(string strng)
        {
            var shaKeyBytes = Convert.FromBase64String(ApiKey);
            using (var shaAlgorithm = new System.Security.Cryptography.HMACSHA256(shaKeyBytes))
            {
                var signatureBytes = Encoding.UTF8.GetBytes(strng);
                var signatureHashBytes = shaAlgorithm.ComputeHash(signatureBytes);
                var signatureHashHex = string.Concat(Array.ConvertAll(signatureHashBytes, b => b.ToString("X2"))).ToLower();

                return signatureHashHex;
            }

        }

        public static string GenerateEncodedUrl(string finalurl, string method)
        {
            //encode url and convert to lowercase
            var urlEncode = HttpUtility.UrlEncode(finalurl);
            if (urlEncode == null) return "";
            var encodedUrl = urlEncode.ToLower();
            //Concatenate Method Name and URL(reqconcat ) 
            var reqconcat = method.ToUpper() + encodedUrl;
            //Generated Digest(digest
            var digest = HmacSha256Digest(reqconcat);
            //Prefix digest with username separated by a colon 
            digest = Username + ":" + digest;
            //base64 encoding
            var bytes = Encoding.UTF8.GetBytes(digest);
            var res = Convert.ToBase64String(bytes);
            return res;
        }

        public static void VerifyVotersId(long id)
        {
            const string resource = "voter";
            using (var db = new AppDbContext())
            {
                var rec = db.UserIdentifications.First(x => x.Id == id);
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(resource, Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("vid", rec.Number);
                var url = client.BuildUri(request).AbsoluteUri;
                var authKey = GenerateEncodedUrl(url, "GET");
                request.AddHeader(HttpRequestHeader.Authorization.ToString(),
                    $"hmac {authKey}");
                var res = client.Execute(request);
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    //mark id as verified
                    rec.IsVerified = true;
                    rec.ModifiedAt = DateTime.Now;
                    
                    //save the results
                    var newLookUp = new GviveLookup
                    {
                        IdType = IdType.Voters,
                        IdNumber = rec.Number,
                        Data = res.Content
                    };
                    db.GviveLookups.Add(newLookUp);
                }
                db.SaveChanges();
            }
        }

        public static void VerifyDriversLicenseId(long id)
        {
            const string resource = "driver";
            using (var db = new AppDbContext())
            {
                var rec = db.UserIdentifications.First(x => x.Id == id);
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(resource, Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("coc", rec.Number);
                request.AddParameter("fname", rec.User.Name);
                var url = client.BuildUri(request).AbsoluteUri;
                var authKey = GenerateEncodedUrl(url, "GET");
                request.AddHeader(HttpRequestHeader.Authorization.ToString(),
                    $"hmac {authKey}");
                var res = client.Execute(request);
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    //mark id as verified
                    rec.IsVerified = true;
                    rec.ModifiedAt = DateTime.Now;

                    //save the results
                    var newLookUp = new GviveLookup
                    {
                        IdType = IdType.DriversLicense,
                        IdNumber = rec.Number,
                        Data = res.Content
                    };
                    db.GviveLookups.Add(newLookUp);
                }
                db.SaveChanges();
            }
        }

        public static void VerifyPassportId(long id)
        {
            const string resource = "passport";
            using (var db = new AppDbContext())
            {
                var rec = db.UserIdentifications.First(x => x.Id == id);
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(resource, Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("pid", rec.Number);
                var url = client.BuildUri(request).AbsoluteUri;
                var authKey = GenerateEncodedUrl(url, "GET");

                request.AddHeader(HttpRequestHeader.Authorization.ToString(),
                    $"hmac {authKey}");
                var res = client.Execute(request);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    //mark id as verified
                    rec.IsVerified = true;
                    rec.ModifiedAt = DateTime.Now;

                    //save the results
                    var newLookUp = new GviveLookup
                    {
                        IdType = IdType.Passport,
                        IdNumber = rec.Number,
                        Data = res.Content
                    };
                    db.GviveLookups.Add(newLookUp);
                }
                db.SaveChanges();
            }
        }

        
    }
}