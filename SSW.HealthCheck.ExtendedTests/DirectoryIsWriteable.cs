using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using SSW.HealthCheck.Infrastructure;
using SSW.HealthCheck.Infrastructure.Tests;

namespace SSW.HealthCheck.ExtendedTests
{
    public class DirectoryIsWriteable : GenericTest
    {

        public DirectoryIsWriteable(string folder, int order) 
            : base("Directory Is Writable", 
            string.Format("check that {0} can be written to by this application", folder), 
            true, 
            context => CheckDirectory(context, folder), 
            order )
        {
            
        }

        private static void CheckDirectory(ITestContext testContext, string folder)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        folder,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
            }
            catch (UnauthorizedAccessException) 
            {
               Assert.Fails(string.Format("failed to write to {0}", folder));
            }
        }

    }
}