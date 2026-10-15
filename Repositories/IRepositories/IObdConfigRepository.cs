using Entities.ViewModels.ObdConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IObdConfigRepository
    {
        Task<DateTime> GetBusinessDate();
        ObdConfigImportResult ReadImportFile(Stream stream);
        Task<ObdConfigSaveResult> InsertFromExcel(List<ObdConfigImportRow> rows, int userId, string fileName);
        Task<ObdConfigSaveResult> Insert(long delivery, decimal netWeight, int userId);
        Task<ObdConfigSaveResult> UpdateDelivery(long obdId, long delivery, int userId);
        Task<List<ObdConfigVersionModel>> GetListVersion(DateTime loadingDate);
        Task<List<ObdConfigModel>> GetList(DateTime loadingDate, int version, string delivery, int pageIndex, int pageSize);
        ObdConfigHistoryModel GetHistory(long obdId);
    }
}
