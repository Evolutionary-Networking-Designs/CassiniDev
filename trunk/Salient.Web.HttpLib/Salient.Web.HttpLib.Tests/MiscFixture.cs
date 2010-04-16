using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Salient.Web.HttpLib.Tests
{
    [TestFixture]
    public class MiscFixture
    {

        [Test]
        public void AppendQueryString()
        {
            var uri = new Uri("http://www.foo.bar/test.aspx");
            var actual = RequestFactory.AppendQueryString(uri, "x=1");
            Assert.AreEqual("http://www.foo.bar/test.aspx?x=1", actual.ToString());
        }

        [Test]
        public void AppendQueryString2()
        {
            var uri = new Uri("http://www.foo.bar/test.aspx");
            Uri actual;
            actual = RequestFactory.AppendQueryString(uri, string.Empty);
            Assert.AreEqual(uri.ToString(), actual.ToString());
            actual = RequestFactory.AppendQueryString(uri, null);
            Assert.AreEqual(uri.ToString(), actual.ToString());
        }

        [Test]
        public void AppendQueryString4()
        {
            var uri = new Uri("http://www.foo.bar");
            var actual = RequestFactory.AppendQueryString(uri, "x=1");
            Assert.AreEqual("http://www.foo.bar/?x=1", actual.ToString());
        }

        [Test]
        public void AppendQueryString5()
        {
            var uri = new Uri("http://www.foo.bar/test.aspx?b=1");
            var actual = RequestFactory.AppendQueryString(uri, "x=1");
            Assert.AreEqual("http://www.foo.bar/test.aspx?b=1&x=1", actual.ToString());


        }

        [Test]
        public void ToQueryString1()
        {
            var obj = new { a = 1, b = 2 };
            var actual = RequestFactory.ToQueryString(obj);
            Assert.AreEqual("a=1&b=2", actual);
        }

        [Test]
        public void ToQueryString3()
        {
            var obj = new { a = "abc&def", b = 2 };
            var actual = RequestFactory.ToQueryString(obj);
            Assert.AreEqual("a=abc%26def&b=2", actual);
        }

        [Test]
        public void ToQueryString2()
        {
            var obj = new NameValueCollection {{"a", "1"}, {"b", "2"}};
            var actual = RequestFactory.ToQueryString(obj);
            Assert.AreEqual("a=1&b=2", actual);
        }

        [Test]
        public void ToQueryString4()
        {
            var obj = new NameValueCollection { { "a", "abc&def" }, { "b", "2" } };
            var actual = RequestFactory.ToQueryString(obj);
            Assert.AreEqual("a=abc%26def&b=2", actual);
        }

        [Test]
        public void TryGetContentType()
        {
            string actual;
            var parsed = RequestFactory.TryGetContentType("abc.txt", out actual);
            Assert.IsTrue(parsed);
            Assert.AreEqual("text/plain", actual);
        }

        [Test]
        public void TryGetContentType2()
        {
            string actual;
            var parsed = RequestFactory.TryGetContentType("abc", out actual);
            Assert.IsFalse(parsed);
            Assert.AreEqual("", actual);
        }
    }

}
