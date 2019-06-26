using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamDotNet.Api.Gateway
{
    /// <summary>
    /// Error Details Container
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        /// <value>
        /// The stack trace.
        /// </value>
        public string StackTrace { get; set; }


        /// <summary>
        ///   <para>
        ///  Converts to Json </para>
        ///   <para>string.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
