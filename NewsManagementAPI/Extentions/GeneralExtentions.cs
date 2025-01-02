using Microsoft.AspNetCore.Identity;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using System.Security.Claims;

namespace NewsManagementAPI.Extentions
{
    public static class GeneralExtentions
    {
        public static string AllMessages(this Exception ex) => $"{ex.Message}___{ex.InnerException?.Message}";
        public static string GetPolicyNumber(this NewsUserRole p) => ((int)p).ToString();
    }
}