using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Barcodes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Barcodes
{
    public interface IBarcodeRepository
    {
        Task<IEnumerable<BarcodeModelDTO>> GetBarcodes();
        Task<BarcodeModelDTO?> GetBarcodeById(int id);
        Task<ResponseResult> SaveUpdateBarcode(BarcodeModelDTO barcode);
        Task DeleteBarcode(int id);
    }
}
