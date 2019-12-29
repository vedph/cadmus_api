using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagingApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Test controller.
    /// </summary>
#if !DEBUG
    [Authorize(Roles = "admin")]
#endif
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IMessageBuilderService _messageBuilderService;
        private readonly IMailerService _mailerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBuilderService">The message builder service.</param>
        /// <param name="mailerService">The mailer service.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public TestController(ILogger<TestController> logger,
            IMessageBuilderService messageBuilderService,
            IMailerService mailerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBuilderService = messageBuilderService ??
                throw new ArgumentNullException(nameof(messageBuilderService));
            _mailerService = mailerService ??
                throw new ArgumentNullException(nameof(mailerService));
        }

        /// <summary>
        /// Adds a diagnostic entry to the log.
        /// </summary>
        /// <returns>OK</returns>
        [HttpPost("api/test/log")]
        [ProducesResponseType(200)]
        public IActionResult AddLogEntry()
        {
            _logger.LogInformation("Diagnostic log entry posted at {Now} UTC " +
                                   "from IP {IP}",
                DateTime.UtcNow,
                HttpContext.Connection.RemoteIpAddress);
            return Ok();
        }

        /// <summary>
        /// Raises an exception to test for logging.
        /// </summary>
        /// <exception cref="Exception">error</exception>
        [HttpGet("api/test/exception")]
        [ProducesResponseType(500)]
        public IActionResult RaiseError()
        {
            Exception exception = new Exception("Fake exception raised for test purposes");
            _logger.LogError(exception, "Fake exception");
            throw exception;
        }

        /// <summary>
        /// Sends a test email message.
        /// </summary>
        [HttpGet("api/test/email")]
        [ProducesResponseType(200)]
        public async Task SendEmail()
        {
            var message = _messageBuilderService.BuildMessage("test-message", null);
            if (message != null)
            {
                await _mailerService.SendEmailAsync(
                    "dfusi@hotmail.com",
                    "Daniele Fusi",
                    message);
            }
        }
    }
}