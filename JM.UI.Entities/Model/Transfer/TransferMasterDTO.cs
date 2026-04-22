using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Transfer;

public class TransferMasterDTO
{
    public int TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public int TransTypeID { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.Today;
    public int? StoreId { get; set; }
    public int? ToStoreId { get; set; }
    public int DeliveryTypeId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Comments { get; set; }
    public string? StoreName { get; set; }
    public string? ToStoreName { get; set; }
    public int? RequisitionID { get; set; }
    public int IsCompleted { get; set; }
    public int? ReceiveStatusId { get; set; }
    public string? RecievedBy { get; set; }
    public DateTime? RecievedDate { get; set; }
    public int CompanyID { get; set; }

    // ── Audit ──
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedRemarks { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdateRemark { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeleteRemarks { get; set; }

    public bool IsDispatched { get; set; }
    public int? DispatchedBy { get; set; }
    public DateTime? DispatchedDate { get; set; }
    public bool IsReceived { get; set; }
    public int? ReceivedBy { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsCancelled { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledDate { get; set; }

    public List<TransferDetailDTO> Details { get; set; } = new();
}