namespace Infrastructure.Utility
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } // Indicates success or error
        public T Data { get; set; } // The response data
        public ApiError Error { get; set; } // Error details (if any)
        public ApiMetadata Metadata { get; set; } // Metadata (if any)
        public string Message { get; set; } // Informational or error message
    }

    public class ApiError
    {
        public string Code { get; set; } // Error code
        public string Message { get; set; } // Error message
    }

    public class ApiMetadata
    {
        // Add metadata properties as needed
    }

    public static class ApiResponseHelper
    {
        public static ApiResponse<T> CreateSuccessResponse<T>(T data, string message = null, ApiMetadata metadata = null)
        {
            return new ApiResponse<T>
            {
                Status = "success",
                Data = data,
                Error = null,
                Metadata = metadata,
                Message = message
            };
        }

        public static ApiResponse<string> CreateErrorResponse(string code, string message)
        {
            return new ApiResponse<string>
            {
                Status = "error",
                Data = null, // Data should be null for error responses
                Error = new ApiError
                {
                    Code = code,
                    Message = message
                },
                Metadata = null,
                //Message = message // Including the error message
            };
        }
    }
}
