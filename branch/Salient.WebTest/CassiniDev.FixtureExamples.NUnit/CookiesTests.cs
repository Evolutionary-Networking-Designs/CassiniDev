// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Net;
using CassiniDev.Testing;
using NUnit.Framework;

namespace CassiniDev.FixtureExamples.NUnit
{
    /// <summary>
    /// See AllTests.cs for commentary
    /// </summary>
    [TestFixture, Category("Discrete")]
    public class CookiesTests : TestAppFixture
    {
        [Test]
        public void GetWebFormWithHttpRequestHelperWithCookies()
        {
            CookieContainer cookies = new CookieContainer();
            Uri requestUri = NormalizeUri("TestWebFormCookies1.aspx");
            HttpRequestHelper.Get(requestUri, null, cookies);
            CookieCollection mycookies = cookies.GetCookies(requestUri);
            Cookie cookie = mycookies["Cooookie"];
            Assert.IsNotNull(cookie);
            Assert.AreEqual("TestWebFormCookies1", cookie.Value);
            requestUri = NormalizeUri("TestWebFormCookies2.aspx");
            HttpRequestHelper.Get(requestUri, null, cookies);
            mycookies = cookies.GetCookies(requestUri);
            cookie = mycookies["Cooookie"];
            Assert.IsNotNull(cookie);
            Assert.AreEqual("TestWebFormCookies2", cookie.Value);
        }
    }
}