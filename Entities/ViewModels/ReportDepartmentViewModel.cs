using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace Entities.ViewModels.Report
{
    public class ReportDepartmentViewModel
    {
        public long Type { get;set;}
        public long DepartmentId { get;set;}
        public long DepartmentType { get;set;}
        public long SupplierId { get;set;}
        public string SalerId { get;set;}
        public string OrderStatus { get;set;}
        public string ServiceType { get;set;}
        public long ClientId { get;set;}
        public double Vat { get; set; } = 0;
        public int PaymentStatus { get; set; } = -1;
        public int PermisionType { get; set; } = -1;
        public string HINHTHUCTT { get; set; }
        public string StartDateFromStr { get;set;}
        public DateTime? StartDateFrom
        {
            get
            {
                return DateUtil.StringToDate(StartDateFromStr);
            }
        }
        public string StartDateToStr { get;set;}
        public DateTime? StartDateTo
        {
            get
            {
                return DateUtil.StringToDate(StartDateToStr);
            }
        }
        public string EndDateFromStr { get;set;}
        public DateTime? EndDateFrom
        {
            get
            {
                return DateUtil.StringToDate(EndDateFromStr);
            }
        }
        public string EndDateToStr { get;set;}
        public DateTime? EndDateTo
        {
            get
            {
                return DateUtil.StringToDate(EndDateToStr);
            }
        }

        public string CreateDateFrom { get; set; }
       
        public string CreateDateTo { get; set; }
      
        public string FromDate { get;set;}
        public string ToDate { get;set;}
        public string SalerPermission { get;set;}
        public int? PageIndex { get; set; } = 0;
        public int? PageSize { get; set; } = -1;
    }
    public class SearchReportDepartmentViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public long ParentDepartmentId { get; set; }
        public long DepartmentId { get; set; }
        public long TotalOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public double Comission { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double AmountVat { get; set; }
        public double PriceVat { get; set; }
        public double ProfitVat { get; set; }
    }
    public class SearchReportDepartmentSupplier
    {
      
        public string FullName { get; set; }
       
        public long SupplierId { get; set; }
        public long TotalOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public double Comission { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double AmountVat { get; set; }
        public double PriceVat { get; set; }
        public double ProfitVat { get; set; }
    }
    public class SearchReportDepartmentClient
    {

        public string ClientName { get; set; }

        public long ClientId { get; set; }
        public long TotalOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public double Comission { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double AmountVat { get; set; }
        public double PriceVat { get; set; }
        public double ProfitVat { get; set; }
    }
    public class ListReportDepartmentViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }

        public long ParentDepartmentTotalOrder { get; set; }
        public double ParentDepartmentAmount { get; set; }
        public double ParentDepartmentPrice { get; set; }
        public double ParentDepartmentComission { get; set; }
        public double ParentDepartmentProfit { get; set; }
        public double ParentDepartmentPercent { get; set; }
        public double ParentDepartmentAmountVat { get; set; }
        public double ParentDepartmentPriceVat { get; set; }
        public double ParentDepartmentProfitVat { get; set; }
        public List<SearchReportDepartmentViewModel> listReportDepartment { get; set; }
    }

    public class DetailRevenueByDepartmentViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ClientName { get; set; }
        public long ParentDepartmentId { get; set; }
        public long DepartmentId { get; set; }
        public long TotalOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public double Comission { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double AmountVat { get; set; }
        public double PriceVat { get; set; }
        public double ProfitVat { get; set; }

        public double FlyBookingAmount { get; set; }
        public double FlyBookingPrice { get; set; }
        public double FlyBookingProfit { get; set; }
        public double HotelBookingAmount { get; set; }
        public double HotelBookingPrice { get; set; }
        public double HotelBookingProfit { get; set; }
        public double TourAmount { get; set; }
        public double TourPrice { get; set; }
        public double TourProfit { get; set; }
        public double OtherBookingAmount { get; set; }
        public double OtherBookingPrice { get; set; }
        public double OtherBookingProfit { get; set; }
        public double VinWonderAmount { get; set; }
        public double VinWonderPrice { get; set; }
        public double VinWonderProfit { get; set; }

    }
    public class ListDetailRevenueByDepartmentViewModel
    {
        public string ParentDepartmentName { get; set; }
        public string DepartmentName { get; set; }
        public long ParentDepartmentTotalOrder { get; set; }
        public double ParentDepartmentAmount { get; set; }
        public double ParentDepartmentPrice { get; set; }
        public double ParentDepartmentComission { get; set; }
        public double ParentDepartmentProfit { get; set; }
        public double ParentDepartmentPercent { get; set; }
        public double ParentDepartmentAmountVat { get; set; }
        public double ParentDepartmentPriceVat { get; set; }
        public double ParentDepartmentProfitVat { get; set; }
        public double DepartmentFlyBookingAmount { get; set; }
        public double DepartmentFlyBookingPrice { get; set; }
        public double DepartmentFlyBookingProfit { get; set; }
        public double DepartmentHotelBookingAmount { get; set; }
        public double DepartmentHotelBookingPrice { get; set; }
        public double DepartmentHotelBookingProfit { get; set; }
        public double DepartmentTourAmount { get; set; }
        public double DepartmentTourPrice { get; set; }
        public double DepartmentTourProfit { get; set; }
        public double DepartmentOtherBookingAmount { get; set; }
        public double DepartmentOtherBookingPrice { get; set; }
        public double DepartmentOtherBookingProfit { get; set; }
        public double DepartmentVinWonderAmount { get; set; }
        public double DepartmentVinWonderPrice { get; set; }
        public double DepartmentVinWonderProfit { get; set; }
        public List<DetailRevenueByDepartmentViewModel> listReportDepartment { get; set; }
    }

    public class RevenueBySupplierViewModel
    {

        public long SupplierId { get; set; }
        public long Branch { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public int? PageIndex { get; set; } = 0;
        public int? PageSize { get; set; } = -1;
    }
    public class SearchRevenueBySupplierViewModel
    {

        public long SupplierId { get; set; }
        public string FullName { get; set; }
        public string DebtAccount { get; set; }
        public double AmountOpeningBalanceCredit { get; set; }

        public double AmountOpeningBalanceDebit { get; set; } 
        public double AmountDebit { get; set; }
        public double AmountCredit { get; set; }
        public double AmountClosingBalanceDebit { get; set; }
        public double AmountClosingBalanceCredit { get; set; } 
    }

    public class SearchDetailRevenueBySupplierViewModel
    {

        public long SupplierId { get; set; }
        public string FullName { get; set; }
        public string DebtAccount { get; set; }
        public string CreatedDate { get; set; }
        public string PaymentCode { get; set; }
        public string BillNo { get; set; }
        public string Description { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public double AmountOpeningBalance { get; set; }
        public double AmountCredit { get; set; }
        public double AmountDebit { get; set; }
        public double AmountRemain { get; set; }

    }
    public class ReportDepartmentBysaleViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public double Amount { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public double TotalSignNew { get; set; }
        public double TotalOrder { get; set; }
        public string DepartmentName { get; set; }

    }
    public class DetailDepartmentBysaleViewModel
    {
        public long Id { get; set; }
        public string ClientName { get; set; }
        public double Amount { get; set; }
        public double Profit { get; set; }
        public double Percent { get; set; }
        public long TotalOrder { get; set; }
     
    }
}
