using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Globalization;

namespace GeoHashDaemon
{
    public class GeoHash
    {
        private static HttpClient httpClient = new HttpClient();

        public static double[] GetGeoHash(DateTime date, int latitude, int longitude)
        {
            /*
            var gdate = GDate.ForLongitude(date, longitude);
            var djia = GetDowJonesAsync(gdate).ConfigureAwait(false).GetAwaiter().GetResult();
            var fractions = CalculateFractions(djia, gdate);
            fractions[0] = (fractions[0] + Math.Abs(latitude)) * Math.Sign(latitude);
            fractions[1] = (fractions[1] + Math.Abs(longitude)) * Math.Sign(longitude);

            return fractions;
            */
            var fractions = GetFractions(date, latitude, longitude);
            var coords = Fractions2Coord(fractions, latitude, longitude);
            return coords;
        }

        public static double[] Fractions2Coord(double[] fractions, int latitude, int longitude)
        {
            double[] coords = new double[2];
            coords[0] = (fractions[0] + Math.Abs(latitude)) * Math.Sign(latitude);
            coords[1] = (fractions[1] + Math.Abs(longitude)) * Math.Sign(longitude);
            return coords;
        }

        public static double[] GetFractions(DateTime date, int latitude, int longitude)
        {
            var gdate = GDate.ForLongitude(date, longitude);
            var djia = GetDowJonesAsync(gdate).ConfigureAwait(false).GetAwaiter().GetResult();
            var fractions = CalculateFractions(djia, gdate);

            return fractions;
        }

        public static double[] GetGlobalHash(DateTime date)
        {
            var gdate = GDate.ForGlobalhash(date);
            var djia = GetDowJonesAsync(gdate).ConfigureAwait(false).GetAwaiter().GetResult();
            var fractions = CalculateFractions(djia, gdate);
            fractions[0] = fractions[0] * 180.0 - 90.0;
            fractions[1] = fractions[1] * 360.0 - 180.0;
                       
            return fractions;
        }

        public static async Task<string> GetDowJonesAsync(GDate gdate)
        {
            // http://geo.crox.net/djia/%Y/%m/%d
            // According to the W30 rule, use actual date or date before, depending on date and longitude
            var result = await httpClient.GetAsync($"http://geo.crox.net/djia/{gdate.DowJonesString()}");
            if (!result.IsSuccessStatusCode)
                return "";

            var data = await result.Content.ReadAsStringAsync();
            return data;
        }

        public static int Graticule(double coord)
        {
            return Convert.ToInt32(Math.Truncate(coord));
        }

        public static int Centicule(double[] fractions)
        {
            var i1 = Convert.ToInt32(Math.Truncate(fractions[0] * 10.0));
            var i2 = Convert.ToInt32(Math.Truncate(fractions[1] * 10.0));
            return i1 * 10 + i2;
        }

        public static double[] CalculateFractions(string djia, GDate gdate)
        {
            string hashstring = gdate.ToString() + "-" + djia; // In the hash string we always use actual date

            string hash = GetMd5Hash(hashstring);

            var fraction1 = HexFraction(hash.Substring(0, 16));
            var fraction2 = HexFraction(hash.Substring(16, 16));
            double[] result = new double[] { fraction1, fraction2 };

            return result;
        }

        public static string GetMd5Hash(string input)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=netcore-3.1

            using (MD5 md5Hash = MD5.Create())
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static double HexFraction(string hexString)
        {
            double curvalue = 0;
            double base16 = 16;
            hexString = hexString.ToUpper();

            byte[] hex = Encoding.ASCII.GetBytes(hexString);
            for (int i = 0; i < hex.Length; i++)
            {
                int value = hex[i] - '0';
                if (value > 9)
                    value = value - 7; // '9' - 'A'

                double dvalue = (double)value;
                var weight = Math.Pow(base16, i + 1);
                curvalue += dvalue / weight;
            }

            return curvalue;
        }

        /// <summary>
        /// Calculate distance between two points using the Haversine formula
        /// i.e the shortast distance over the earth's curvature.
        /// https://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        /// <returns>Distance in meters</returns>
        public static double CalcDistance(double[] from, double[] to)
        {
            var lat1 = from[0];
            var lon1 = from[1];
            var lat2 = to[0];
            var lon2 = to[1];

            var R = 6371e3; // metres
            var φ1 = lat1.toRadians();
            var φ2 = lat2.toRadians();
            var Δφ = (lat2 - lat1).toRadians();
            var Δλ = (lon2 - lon1).toRadians();

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = R * c;

            return d;
        }
    }

    public static class Extensions
    {
        public static double toRadians(this double degrees)
        {
            double b = (degrees * (Math.PI)) / 180;
            return b;
        }

        /// <summary>
        /// Display coordinates in a way that Google Maps likes.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static string toGoogle(this double degrees)
        {
            string s = degrees.ToString("F5", CultureInfo.GetCultureInfo("en-US"));
            return s;
        }

    }
}