using System;
using System.Collections.Generic;

namespace Entities.ViewModels.ObdConfig
{
    public class ObdConfigModel
    {
        public long ObdId { get; set; }
        public long Delivery { get; set; }
        public decimal NetWeight { get; set; }
        public DateTime LoadingDate { get; set; }
        public int Version { get; set; }
        public int? UserCreateID { get; set; }
        public int? UserUpdateID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateLast { get; set; }
        public string UserCreateName { get; set; }
        public string UserUpdateName { get; set; }
        public int TotalRow { get; set; }
    }

    public class ObdConfigSearchModel
    {
        public string Delivery { get; set; }
        public string LoadingDate { get; set; }
        public int? Version { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ObdConfigVersionModel
    {
        public int Version { get; set; }
        public int TotalObd { get; set; }
        public decimal TotalNetWeight { get; set; }
        public int TotalRow { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class ObdConfigImportRow
    {
        public int RowNumber { get; set; }
        public long Delivery { get; set; }
        public decimal NetWeight { get; set; }
    }

    public class ObdConfigImportResult
    {
        public List<ObdConfigImportRow> Rows { get; set; } = new List<ObdConfigImportRow>();
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid => Errors.Count == 0 && Rows.Count > 0;
    }

    public class ObdConfigLogModel
    {
        public const int ACTION_IMPORT = 1;
        public const int ACTION_INSERT = 2;
        public const int ACTION_UPDATE = 3;

        public long LogId { get; set; }
        public long? ObdId { get; set; }
        public int Action { get; set; }
        public DateTime? LoadingDate { get; set; }
        public int? Version { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string FileName { get; set; }
        public int? TotalRow { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class ObdConfigHistoryModel
    {
        public ObdConfigModel Detail { get; set; }
        public List<ObdConfigLogModel> Logs { get; set; } = new List<ObdConfigLogModel>();
    }

    public class ObdConfigSaveResult
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public long ObdId { get; set; }
        public DateTime? LoadingDate { get; set; }
        public int Version { get; set; }
        public int TotalRow { get; set; }
    }
}
