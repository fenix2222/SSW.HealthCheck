[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SSW.HealthCheck.MVC4.Examples.App_Start.HealthCheckConfig), "PreStart")]
namespace SSW.HealthCheck.MVC4.Examples.App_Start 
{
	using SSW.HealthCheck.Infrastructure;
    using SSW.HealthCheck.Infrastructure.Tests;
    using SSW.HealthCheck.Mvc4;

    public static class HealthCheckConfig 
    {
        public static void PreStart() 
        {
            // Add your start logic here
            RegisterTests();
        }
        public static void RegisterTests()
        {
            RegisterTests(HealthCheckService.Default);
        }
        public static void RegisterTests(HealthCheckService svc)
        {
            svc.Add(new NotDebugTest(0));
            svc.Add(new DbConnectionTest(1));
            svc.Setup<SSW.HealthCheck.MVC4.Examples.Hubs.HealthCheckHub>();
        }
    }
}