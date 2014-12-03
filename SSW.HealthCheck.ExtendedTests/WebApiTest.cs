namespace SSW.HealthCheck.ExtendedTests
{
    using System;

    using SSW.Common.Extensions;
    using SSW.HealthCheck.Infrastructure;
    using SSW.WebApiHelper;

    public class WebApiTest : ITest
    {
        private string url;

        private string action;

        private object arguments;

        private int timeout = 30000;

        private WebApiService service;

        public WebApiTest(string url,
            string action,
            int timeout,
            string name,
            string description,
            int order = 0,
            bool isDefault = true,
            string categoryName = "",
            int categoryOrder = 0,
            object arguments = null)
        {
            this.timeout = timeout;
            this.url = url;
            this.action = action;
            this.Name = name;
            this.Description = description;
            this.Order = order;
            this.IsDefault = isDefault;
            this.TestCategory = new TestCategory { Name = categoryName, Order = categoryOrder };
            this.arguments = arguments;
        }

        public void Test(ITestContext context)
        {
            if (this.url.IsNullOrEmpty() || this.url == "/" || this.url == "~" || this.url == "~/")
            {
                this.url = this.FullyQualifiedApplicationPath(context);
            }

            if (this.action.IsNullOrEmpty())
            {
                throw new ApplicationException(Errors.UrlAndActionCannotBeEmpty);
            }

            this.service = new WebApiService { Timeout = this.timeout };

            try
            {
                var result = this.arguments == null
                                 ? this.service.Get<object>(this.url, this.action)
                                 : this.service.Get<object>(this.url, this.action, this.arguments);
            }
            catch (WebApiException ex)
            {
                Assert.Fails(Errors.FailedToAccessService, ex.Message);
            }
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public bool IsDefault { get; private set; }

        public int Order { get; set; }

        public TestCategory TestCategory { get; set; }

        public bool HasCategory { get; private set; }

        public string FullyQualifiedApplicationPath(ITestContext testContext)
        {
 
            //Return variable declaration
            var appPath = string.Empty;
 
            //Getting the current context of HTTP request
            var context = testContext.HttpContext;
 
            //Checking the current context content
            if (context != null)
            {
                //Formatting the fully qualified website url/name
                appPath = string.Format("{0}://{1}{2}{3}",
                                        context.Request.Url.Scheme,
                                        context.Request.Url.Host,
                                        context.Request.Url.Port == 80
                                            ? string.Empty
                                            : ":" + context.Request.Url.Port,
                                        context.Request.ApplicationPath);
            }
 
            if (!appPath.EndsWith("/"))
                appPath += "/";
 
            return appPath;
        }
    }
}