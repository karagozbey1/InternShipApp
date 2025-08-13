using Core.Entity;
using System.Collections.Generic;
namespace WebUI.Models
{
    public class DocumentsViewModel
    {
        internal List<InternshipApplication>? ApplicationsWithDocuments;

        public List<InternshipDocument> ?Documents { get; set; }
    }
}
