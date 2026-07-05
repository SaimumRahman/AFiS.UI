using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.PurchaseItems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items;

public class ItemService : IItemService
{
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public ItemService(IRepositoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ItemDTO>> GetItems()
    {
        return await _unitOfWork.ItemRepository.GetItems();
    }
    public async Task<IEnumerable<ItemDTO>> GetItemByPurchaseId(int purchaseId)
    {
        return await _unitOfWork.ItemRepository.GetItemByPurchaseId(purchaseId);
    }
    public async Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId)
    {
        return await _unitOfWork.ItemRepository.GetItemsByStoreId(storeId);
    }

    public async Task<ItemDTO?> GetItemById(int id)
    {
        return await _unitOfWork.ItemRepository.GetItemById(id);
    }
   
    public async Task<int> CreateItem(PreviewItemRow createItemRequest)
    {
        CreateItemRequestDTO createItemRequestDTO = new()
        {
            Name = createItemRequest.ItemName,
            GroupId = createItemRequest.GroupId ?? 0,
            SubGroupId = createItemRequest.SubGroupId ?? 0,
            ShadeNo = createItemRequest.ShadeNo,
            ColorId = createItemRequest.ColorId,
            SizeId = createItemRequest.SizeId,
            MaterialType = createItemRequest.MaterialType,
            Origin = createItemRequest.OriginName,
            ProductPricePercentage = null, // Set this value as needed
            MesurementUnitId = createItemRequest.MesurementUnitId ?? 0,
            CountStockByColor = createItemRequest.CountStockByColor,
            CountStockBySize = createItemRequest.CountStockBySize,
            SalePrice = createItemRequest.SalePrice,
            WholeSalePrice = 0, // Set this value as needed
            PurchasePrice = createItemRequest.PurchasePrice,
            Barcode = createItemRequest.Barcode,
            ProductType = createItemRequest.ProductType,
            Catalogue = createItemRequest.CatalogueName,
            BrandId = createItemRequest.BrandId ?? 0,
            OriginId = createItemRequest.OriginId ?? 0,
            DesignId = createItemRequest.DesignId ?? 0,
            RawMaterial = false, // Set this value as needed
            IsSaleable = createItemRequest.IsSaleable,
            IsConsume = createItemRequest.IsConsume,
            CatalogueId = createItemRequest.CatalogueId,
            FeatureIds = createItemRequest.FeatureIds,
            Features = createItemRequest.FeatureIds != null && createItemRequest.FeatureIds.Any()
    ? string.Join(",", createItemRequest.FeatureIds)
    : string.Empty
        };
        var res = await _unitOfWork.ItemRepository.CreateItem(createItemRequestDTO);
        return Convert.ToInt32(res.Id);
    }

    public async Task<ResponseResult> DeleteItem(int id)
    {
        try
        {
            await _unitOfWork.ItemRepository.DeleteItem(id);
            return new ResponseResult { IsSuccessStatus = true, Message = "Item deleted successfully." };
        }
        catch (Exception ex)
        {
            return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId)
        {
            try
            {
                return await _unitOfWork.ItemRepository.LoadItemsBySubGroup(subGroupId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TransactionTypeDTO>> GetTransactionTypes()
        {
            return await _unitOfWork.ItemRepository.GetTransactionTypes();
        }
    }
