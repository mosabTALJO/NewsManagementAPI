using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialsFilter
    {
        int pageNo;
        int pageSize;
        bool? requiresConfirmation = null;
        DateTime? reportDate = null;
        NewsMaterialStatus[] statuses = [];
    }
}