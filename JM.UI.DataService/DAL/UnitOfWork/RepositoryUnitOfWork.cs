using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Shift;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.UnitOfWork
{
    public class RepositoryUnitOfWork : IRepositoryUnitOfWork
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILoggerFactory _loggerFactory;

    
        public IApprovalLevelRepository ApprovalLevelRepository { get; }
        public IApprovalWorkflowRepository ApprovalWorkflowRepository { get; }
        public IApprovalLevelApproverRepository ApprovalLevelApproverRepository { get; }
        public IPendingApprovalRepository PendingApprovalRepository { get; }
        public IDesignationRepository DesignationRepository { get; }
        public ICompanyRepository CompanyRepository { get; }
        public IBanksRepository BanksRepository { get; }
        public IShiftRepository ShiftRepository { get; }

        public RepositoryUnitOfWork(
            IHttpClientFactory factory,
            ITokenProvider tokenProvider,
            ILoggerFactory loggerFactory)
        {
            _factory = factory;
            _tokenProvider = tokenProvider;
            _loggerFactory = loggerFactory;

       
            ApprovalLevelRepository = new ApprovalLevelRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalLevelRepository>());
            ApprovalWorkflowRepository = new ApprovalWorkflowRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalWorkflowRepository>());
            ApprovalLevelApproverRepository = new ApprovalLevelApproverRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalLevelApproverRepository>());
            PendingApprovalRepository = new PendingApprovalRepository(factory, tokenProvider, loggerFactory.CreateLogger<PendingApprovalRepository>());
            CompanyRepository = new CompanyRepository(factory, tokenProvider, loggerFactory.CreateLogger<CompanyRepository>());
            DesignationRepository = new DesignationRepository(factory, tokenProvider, loggerFactory.CreateLogger<DesignationRepository>());
            BanksRepository = new BanksRepository(factory, tokenProvider, loggerFactory.CreateLogger<BanksRepository>());
            ShiftRepository = new ShiftRepository(factory, tokenProvider, loggerFactory.CreateLogger<ShiftRepository>());

        }

        public void Dispose()
        {
            // No unmanaged resources — nothing to dispose.
        }
    }
}
