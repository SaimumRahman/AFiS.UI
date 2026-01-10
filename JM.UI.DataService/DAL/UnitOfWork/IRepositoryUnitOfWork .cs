using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.UnitOfWork
{
    public interface IRepositoryUnitOfWork : IDisposable
    {
        IApprovalLevelRepository ApprovalLevelRepository { get; }
        IApprovalWorkflowRepository ApprovalWorkflowRepository { get; }
        IApprovalLevelApproverRepository ApprovalLevelApproverRepository { get; }
        IPendingApprovalRepository PendingApprovalRepository { get; }
        ICompanyRepository CompanyRepository { get; }
    }

}
