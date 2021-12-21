using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Geonorge.GmlKart.Application.Helpers
{
    public class FileHelper
    {
        private static readonly Regex _gmlRegex =
            new(@"^<\?xml.*?<gml:FeatureCollection.*?xmlns:gml=""http:\/\/www\.opengis\.net\/gml\/3\.2""", RegexOptions.Compiled | RegexOptions.Singleline);

        public static bool IsGmlFile(IFormFile file)
        {
            var fileContents = ReadLines(file.OpenReadStream(), 25);

            return _gmlRegex.IsMatch(fileContents);
        }

        private static string ReadLines(Stream stream, int numberOfLines)
        {
            if (numberOfLines < 1)
                throw new ArgumentException("numberOfLines må være større enn 0");

            var counter = 0;
            var stringBuilder = new StringBuilder(numberOfLines * 250);
            using var streamReader = new StreamReader(stream);

            while (counter++ < numberOfLines && !streamReader.EndOfStream)
                stringBuilder.Append(streamReader.ReadLine());

            return stringBuilder.ToString();
        }
    }
}
