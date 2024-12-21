using Microsoft.AspNetCore.Mvc;

namespace NewsManagementAPI.Helpers
{
    public class ApiResponse<T>
    {
        public List<string> Errors { get; set; } = [];
        public bool IsSuccess => Errors.Count > 0;
        public T? Data { get; set; }
    }
    public static class ApiResponse
    {
        public static ApiResponse<T> CreateSuccess<T>(T data) => new() { Data = data };
        public static ApiResponse<object> CreateFailure(string error) => new() { Errors = [error] };
        public static ApiResponse<object> CreateFailure(IEnumerable<string> errors) => new() { Errors = errors.ToList() };
    }
    public enum ResponseCode { Ok, NotFound, Unauthorized, BadRequest }
}
