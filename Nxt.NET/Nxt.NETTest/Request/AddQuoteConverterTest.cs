using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nxt.NET.Request;

namespace Nxt.NETTest.Request
{
    public class AddQuoteConverterTest
    {
        private StringBuilder _stringBuilder;
        protected readonly long Id = 123456L;
        private JsonTextWriter _writer;

        protected void SetupTest()
        {
            _stringBuilder = new StringBuilder();
            _writer = new JsonTextWriter(new StringWriter(_stringBuilder));
        }

        protected void AssertTest(string propertyName, IRequest request)
        {
            new JsonSerializer().Serialize(_writer, request);
            StringAssert.Contains(_stringBuilder.ToString(), string.Format("\"{0}\":\"123456\"", propertyName));
        }
    }
}