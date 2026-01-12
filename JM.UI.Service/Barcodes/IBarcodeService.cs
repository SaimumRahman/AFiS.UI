using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Barcodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Barcodes
{
    public interface IBarcodeService
    {
        Task<IEnumerable<BarcodeModelDTO>> GetBarcodes();
        Task<BarcodeModelDTO?> GetBarcodeById(int id);
        Task<ResponseResult> SaveUpdateBarcode(BarcodeModelDTO barcode);
        Task<ResponseResult> DeleteBarcode(int id);
    }
}
