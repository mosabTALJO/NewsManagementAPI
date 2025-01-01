using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
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