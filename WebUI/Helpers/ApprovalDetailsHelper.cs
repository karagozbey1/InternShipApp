using Core.Entity;
using WebUI.Models.ViewModels.Student;

namespace WebUI.Helpers
{
    public static class ApprovalDetailsHelper
    {
        public static ApprovalDetailsData GetApprovalDetailsData(ApprovalFormViewModel model)
        {
            return new ApprovalDetailsData
            {
                StatusText = GetStatusText(model),
                CheckLegacyRevision = CheckLegacyRevision(model),
                StartDateText = GetFormattedDate(model.StartDate, "dd MMMM yyyy"),
                EndDateText = GetFormattedDate(model.EndDate, "dd MMMM yyyy"),
                StatusColor = GetStatusColor(model.Status),
                IsRevisionCase = IsRevisionCase(model),
                RevisionText = GetRevisionText(model)
            };
        }

        private static string GetStatusText(ApprovalFormViewModel model)
        {
            var statusText = model.Status switch
            {
                InternshipApprovalStatus.Pending => "Beklemede",
                InternshipApprovalStatus.Approved => "Onaylandý",
                InternshipApprovalStatus.Rejected => "Reddedildi",
                InternshipApprovalStatus.Revision => "Revizyonda",
                _ => model.Status.ToString()
            };

            // Legacy revision check
            if (CheckLegacyRevision(model))
            {
                statusText = "Revizyonda";
            }

            return statusText;
        }

        private static bool CheckLegacyRevision(ApprovalFormViewModel model)
        {
            return !string.IsNullOrEmpty(model.RejectionReason) && 
                   model.RejectionReason.StartsWith("REVISION:");
        }

        private static string GetFormattedDate(DateTime date, string format)
        {
            return date != DateTime.MinValue ? date.ToString(format) : "-";
        }

        private static string GetStatusColor(InternshipApprovalStatus status)
        {
            return status switch
            {
                InternshipApprovalStatus.Pending => "warning",
                InternshipApprovalStatus.Approved => "success",
                InternshipApprovalStatus.Rejected => "danger",
                InternshipApprovalStatus.Revision => "warning",
                _ => "secondary"
            };
        }

        private static bool IsRevisionCase(ApprovalFormViewModel model)
        {
            return model.Status == InternshipApprovalStatus.Revision || 
                   CheckLegacyRevision(model);
        }

        private static string GetRevisionText(ApprovalFormViewModel model)
        {
            if (model.Status == InternshipApprovalStatus.Revision && 
                !string.IsNullOrEmpty(model.RevisionNote))
            {
                return model.RevisionNote;
            }
            else if (CheckLegacyRevision(model))
            {
                return model.RejectionReason.Substring("REVISION:".Length).Trim();
            }
            
            return string.Empty;
        }
    }

    public class ApprovalDetailsData
    {
        public string StatusText { get; set; } = string.Empty;
        public bool CheckLegacyRevision { get; set; }
        public string StartDateText { get; set; } = string.Empty;
        public string EndDateText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public bool IsRevisionCase { get; set; }
        public string RevisionText { get; set; } = string.Empty;
    }
}