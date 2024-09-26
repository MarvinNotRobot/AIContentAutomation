using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OpenAPI
{
    public static class OpenAIExceptionCode
    {
        public const int Prompt_Too_Long = 1001;
    }

    public class OpenAIPromptTooLongException : Exception
    {
        public OpenAIPromptTooLongException(string message) : base(message)
        {
        }

        public OpenAIPromptTooLongException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public int ErrorCode { get; set; }
    }

}
