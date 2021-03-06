﻿namespace SSW.HealthCheck.Infrastructure
{
    public class TestResult
    {
        /// <summary>
        /// Gets or sets a value that indicate if the test passed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message that describe the result of the test.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the test running time.
        /// </summary>
        public long RunningTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show warning.
        /// </summary>
        public bool ShowWarning { get; set; }
    }
}
