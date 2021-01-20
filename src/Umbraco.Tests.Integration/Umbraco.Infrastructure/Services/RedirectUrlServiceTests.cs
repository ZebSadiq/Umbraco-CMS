﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RedirectUrlServiceTests : UmbracoIntegrationTestWithContent
    {
        private IContent _firstSubPage;
        private IContent _secondSubPage;
        private IContent _thirdSubPage;
        private const string Url = "blah";
        private const string UrlAlt = "alternativeUrl";
        private const string CultureEnglish = "en";
        private const string CultureGerman = "de";
        private const string UnusedCulture = "es";

        private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

        public override void CreateTestData()
        {
            base.CreateTestData();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var repository = new RedirectUrlRepository((IScopeAccessor)ScopeProvider, AppCaches.Disabled, Mock.Of<ILogger<RedirectUrlRepository>>());
                IContent rootContent = ContentService.GetRootContent().First();
                var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _).ToList();
                _firstSubPage = subPages[0];
                _secondSubPage = subPages[1];
                _thirdSubPage = subPages[2];


                repository.Save(new RedirectUrl
                {
                    ContentKey = _firstSubPage.Key,
                    Url = Url,
                    Culture = CultureEnglish
                });
                Thread.Sleep(1000); //Added delay to ensure timestamp difference as sometimes they seem to have the same timestamp
                repository.Save(new RedirectUrl
                {
                    ContentKey = _secondSubPage.Key,
                    Url = Url,
                    Culture = CultureGerman
                });
                Thread.Sleep(1000);
                repository.Save(new RedirectUrl
                {
                    ContentKey = _thirdSubPage.Key,
                    Url = UrlAlt,
                    Culture = string.Empty
                });

                scope.Complete();
            }
        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl()
        {
            IRedirectUrl redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url);
            Assert.AreEqual(redirect.ContentId, _secondSubPage.Id);
        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
        {
            var redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url, CultureEnglish);
            Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture_When_No_CultureVariant_Exists()
        {
            var redirect = RedirectUrlService.GetMostRecentRedirectUrl(UrlAlt, UnusedCulture);
            Assert.AreEqual(redirect.ContentId, _thirdSubPage.Id);

        }
    }
}
