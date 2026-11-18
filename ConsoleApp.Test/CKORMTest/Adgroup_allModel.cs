using Framework.Common.Core.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Test.CKORMTest
{
    [TabelNameAttribute("Adgroup_all")]    
    public class AdgroupModel
    {
        [ColumnNameAttribute("adGroupId", ColumnTypeString.Int64)]
        [PK]
        public long AdGroupId { get; set; }
        [ColumnNameAttribute("campaignId", ColumnTypeString.Int64)]
        [PK]
        public long CampaignId { get; set; }
        [ColumnNameAttribute("profileId", ColumnTypeString.Int64)]
        [PK]
        public long ProfileId { get; set; }
        [ColumnNameAttribute("name", ColumnTypeString.String)]
        public string Name { get; set; }
        [ColumnNameAttribute("totalBudget", ColumnTypeString.Decimal_18_2)]
        public decimal? TotalBudget { get; set; }
        [ColumnNameAttribute("dailyBudget", ColumnTypeString.Decimal_18_2)]
        public decimal? DailyBudget { get; set; }
        [ColumnNameAttribute("status", ColumnTypeString.String)]
        public string Status { get; set; }
        [ColumnNameAttribute("state", ColumnTypeString.String)]
        public string State { get; set; }
        [ColumnNameAttribute("lastSyncTime", ColumnTypeString.DateTime64)]
        public DateTime? LastSyncTime { get; set; }
        [ColumnNameAttribute("lineItemId", ColumnTypeString.Int64)]
        public long? LineItemId { get; set; }
        [ColumnNameAttribute("startDate", ColumnTypeString.DateTime64)]
        public DateTime? StartDate { get; set; }
        [ColumnNameAttribute("endDate", ColumnTypeString.DateTime64)]
        public DateTime? EndDate { get; set; }
        [ColumnNameAttribute("budgetType", ColumnTypeString.String)]
        public string BudgetType { get; set; }
        [ColumnNameAttribute("rateType", ColumnTypeString.String)]
        public string RateType { get; set; }
        [ColumnNameAttribute("deliveryRateType", ColumnTypeString.String)]
        public string DeliveryRateType { get; set; }
        [ColumnNameAttribute("rate", ColumnTypeString.Decimal_18_2)]
        public decimal? Rate { get; set; }
        [ColumnNameAttribute("bookedImpressions", ColumnTypeString.Int64)]
        public long? BookedImpressions { get; set; }
        [ColumnNameAttribute("creationDate", ColumnTypeString.DateTime64)]
        public DateTime? CreationDate { get; set; }
        [ColumnNameAttribute("lineItemExtId", ColumnTypeString.String)]
        public string LineItemExtId { get; set; }
        [ColumnNameAttribute("frequencyCapDay", ColumnTypeString.Int64)]
        public long? FrequencyCapDay { get; set; }
        [ColumnNameAttribute("frequencyCapWeek", ColumnTypeString.Int64)]
        public long? FrequencyCapWeek { get; set; }
        [ColumnNameAttribute("frequencyCapMonth", ColumnTypeString.Int64)]
        public long? FrequencyCapMonth { get; set; }
        [ColumnNameAttribute("baseBid", ColumnTypeString.Decimal_18_2)]
        public decimal? BaseBid { get; set; }
        [ColumnNameAttribute("maxBid", ColumnTypeString.Decimal_18_2)]
        public decimal? MaxBid { get; set; }
        [ColumnNameAttribute("targeting", ColumnTypeString.String)]
        public string Targeting { get; set; }
        [ColumnNameAttribute("startDateTimeZone", ColumnTypeString.String)]
        public string StartDateTimeZone { get; set; }
        [ColumnNameAttribute("endDateTimeZone", ColumnTypeString.String)]
        public string EndDateTimeZone { get; set; }
        [ColumnNameAttribute("campaignName", ColumnTypeString.String)]
        public string CampaignName { get; set; }
        [ColumnNameAttribute("selectionType", ColumnTypeString.String)]
        public string SelectionType { get; set; }
        [ColumnNameAttribute("sign", ColumnTypeString.String)]
        public Boolean IsDelete { get; set; }
    }
}
