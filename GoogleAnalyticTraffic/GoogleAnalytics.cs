using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
namespace Suyati.GoogleTraffic
{
    public class GoogleAnalytics
    {
        /// <summary>
        /// store google auth data
        /// </summary>
        private string _GoogleAuthToken;
        /// <summary>
        /// Access token of authenticated user
        ///Please Refer This For More Info  https://developers.google.com/analytics/devguides/config/mgmt/v3/mgmtAuthorization?hl=en
        /// </summary>
        /// <param name="GoogleTempSession"></param>
        public GoogleAnalytics(string GoogleAuthToken)
        {
            _GoogleAuthToken = GoogleAuthToken;

        }
        /// <summary>
        /// Fetch Analytical Data From Google Analytics
        /// </summary>
        /// //start
        /// Google analytical filtering options
        /// <param name="StartDate" Required="True"></param>
        /// <param name="EndDate" Required="True"></param>
        /// <param name="Filters" Required="False">Comma Seperated  Filters For The Google Analytics ex:- ga:campaigns,ga:</param>
        /// <param name="Goals" Required="False">Applying Metrics For Google Analytics  ex:-ga:users,ga:pageviews </param>
        /// <param name="ProfileId" Required="True">Profile of google Analytic Account  ex:301777</param>
        /// <param name="Options"></param>
        /// //end
        /// <returns>GoogleAnalyticResults</returns>
        public List<KeyValuePair<string, int>> GetGoogleData(DateTime StartDate, DateTime EndDate, string Filters, string Goals, string ProfileId)
        {
            try
            {
                //Building Uri For Webrequest to www.googleapis.com with the user filters
                string uriBuilder =
                              string.Format(
                              @"https://www.googleapis.com/analytics/v3/data/ga?ids=ga:{1}&dimensions=ga:source" + (string.IsNullOrEmpty(Goals) ? "&metrics=ga:goalCompletionsAll" : "ga:goal{4}").ToString() + "&start-date={2}&end-date={3}&"+(string.IsNullOrEmpty(Filters)?"":"filters={4}&")+"access_token={0}"
                                  , this._GoogleAuthToken
                                  , ProfileId
                                  , StartDate.ToString("yyyy-MM-dd")
                                  , StartDate.ToString("yyyy-MM-dd")
                                  , Filters
                                 );
                //creating webrequest to https://www.googleapis.com/analytics to fetch the data
                WebRequest AnalyticWebRequest = WebRequest.Create(uriBuilder);
                //Manipulating the Response From The Google Analytic To a  User Readable Format
                using (var response = (HttpWebResponse)AnalyticWebRequest.GetResponse())
                {
                    using (Stream data = response.GetResponseStream())
                        if (data != null)
                        {
                            using (var reader = new StreamReader(data))
                            {
                                dynamic dynamicData = JObject.Parse(reader.ReadToEnd());
                                //casting response to  keyvalue pair  
                                GoogleAnalyticsData googleAnalyticsData = dynamicData.ToObject<GoogleAnalyticsData>();
                                if (googleAnalyticsData == null || googleAnalyticsData.Rows == null || googleAnalyticsData.Rows.Length <= 0)
                                {
                                    return null;
                                }
                                Dictionary<string, int> analyticsData = Enumerable.Range(0, googleAnalyticsData.Rows.GetLength(0))
                                        .ToDictionary(
                                            i => googleAnalyticsData.Rows[i, 0],
                                            i => Convert.ToInt32(googleAnalyticsData.Rows[i, 1]));

                                return analyticsData.ToList();

                            }
                        }
                }
                return null;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        /// <summary>
        /// Internal Class for Converting to usable data from analytical Data
        /// </summary>
        internal class GoogleAnalyticsData
        {
            /// <summary>
            ///     Gets or sets the rows.
            /// </summary>
            public string[,] Rows { get; set; }
        }
    }
}



