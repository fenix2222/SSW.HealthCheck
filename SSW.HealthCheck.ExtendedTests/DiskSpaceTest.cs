namespace SSW.HealthCheck.ExtendedTests
{
    using System.Management;

    using SSW.HealthCheck.Infrastructure;

    public class DiskSpaceTest : ITest
    {
        public DiskSpaceTest(
            int order = 0,
            bool isDefault = true,
            string categoryName = "",
            int categoryOrder = 0)
        {
            this.Name = Labels.DiskSpaceTest;
            this.Description = Labels.DiskSpaceTestDescription;
            this.Order = order;
            this.IsDefault = isDefault;
            this.TestCategory = new TestCategory { Name = categoryName, Order = categoryOrder };
        }

        public void Test(ITestContext context)
        {
            var options = new ConnectionOptions();
            var scope = new ManagementScope(@"\\localhost\root\cimv2", options);
            scope.Connect();
            var query = new SelectQuery("Select * from Win32_LogicalDisk");

            var searcher = new ManagementObjectSearcher(scope, query);
            var queryCollection = searcher.Get();

            var fail = false;
            var warning = false;
            foreach (var managementBaseObject in queryCollection)
            {
                if (managementBaseObject["DeviceID"] != null && managementBaseObject["FreeSpace"] != null && managementBaseObject["Size"] != null)
                {
                    var driveLetter = managementBaseObject["DeviceID"].ToString();
                    var freeSpace = long.Parse(managementBaseObject["FreeSpace"].ToString());
                    var totalSpace = long.Parse(managementBaseObject["Size"].ToString());

                    var percentageFree = (freeSpace / (double)totalSpace) * 100;
                    var critical = percentageFree <= 2;
                    var low = percentageFree > 2 && percentageFree < 5;
                    context.WriteLine(
                        string.Format(
                            InfoMessages.DriveSpace,
                            driveLetter,
                            this.GetBytesFormatted(freeSpace),
                            this.GetBytesFormatted(totalSpace)),
                        critical ? EventType.Error : (low ? EventType.Warning : EventType.Success));
                    if (critical)
                    {
                        fail = true;
                    }
                    else if (low)
                    {
                        warning = true;
                    }
                }
            }

            if (fail)
            {
                Assert.Fails(Errors.VeryLowSpace);
            }
            else if (warning)
            {
                Assert.PassWithWarning(Errors.LowSpace);
            }
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public bool IsDefault { get; private set; }

        public int Order { get; set; }

        public TestCategory TestCategory { get; set; }

        public bool HasCategory { get; private set; }

        private string GetBytesFormatted(long bytes)
        {
            if (bytes <= 1024)
            {
                return string.Format("{0:N0} bytes", bytes);
            }

            var kiloBytes = (decimal)bytes / 1024;
            if (kiloBytes <= 1024)
            {
                return string.Format("{0:N2} KB", kiloBytes);
            }

            var megaBytes = kiloBytes / 1024;
            if (megaBytes <= 1024)
            {
                return string.Format("{0:N2} MB", megaBytes);
            }

            var gigaBytes = megaBytes / 1024;
            return string.Format("{0:N2} GB", gigaBytes);
        }
    }
}