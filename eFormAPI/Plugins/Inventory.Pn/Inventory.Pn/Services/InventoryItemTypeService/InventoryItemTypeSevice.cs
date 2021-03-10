/*
The MIT License (MIT)
Copyright (c) 2007 - 2021 Microting A/S
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Inventory.Pn.Services.InventoryItemTypeService
{
    using Infrastructure.Models.ItemType;
    using InventoryLocalizationService;
    using Microsoft.EntityFrameworkCore;
    using Microting.eForm.Infrastructure.Constants;
    using Microting.eFormApi.BasePn.Abstractions;
    using Microting.eFormApi.BasePn.Infrastructure.Extensions;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using Microting.eFormInventoryBase.Infrastructure.Data;
    using Microting.eFormInventoryBase.Infrastructure.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Common;
    using Microting.eFormInventoryBase.Infrastructure.Const;

    public class InventoryItemTypeService : IInventoryItemTypeService
    {
        private readonly InventoryPnDbContext _dbContext;
        private readonly IInventoryLocalizationService _inventoryLocalizationService;
        private readonly IUserService _userService;
        //private readonly IEFormCoreService _coreService;

        public InventoryItemTypeService(InventoryPnDbContext dbContext,
            IInventoryLocalizationService inventoryLocalizationService,
            IUserService userService/*,
            IEFormCoreService coreService*/)
        {
            _userService = userService;
            _inventoryLocalizationService = inventoryLocalizationService;
            //_coreService = coreService;
            _dbContext = dbContext;
        }
        public async Task<OperationResult> CreateItemType(ItemTypeCreateModel itemTypeCreateModel)
        {
            try
            {
                var tags = new List<InventoryTag>();
                if (itemTypeCreateModel.TagIds.Count > 0)
                {
                    tags = _dbContext.InventoryTags
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Where(x => itemTypeCreateModel.TagIds.Contains(x.Id))
                        .ToList();

                    if (tags.Count != itemTypeCreateModel.TagIds.Count)
                    {
                        return new OperationResult(false,
                            _inventoryLocalizationService.GetString("TagsNotFound"));
                    }
                }

                var itemType = new ItemType
                {
                    Name = itemTypeCreateModel.Name,
                    Description = itemTypeCreateModel.Description,
                    NetWeight = itemTypeCreateModel.NetWeight,
                    GrossWeight = itemTypeCreateModel.GrossWeight,
                    UnitVolume = itemTypeCreateModel.UnitVolume,
                    CostingMethod = itemTypeCreateModel.CostingMethod,
                    ProfitPercent = itemTypeCreateModel.ProfitPercent,
                    Region = itemTypeCreateModel.Region,
                    CreatedByUserId = _userService.UserId,
                    GtinEanUpc = itemTypeCreateModel.GtinEanUpc,
                    LastPhysicalInventoryDate = itemTypeCreateModel.LastPhysicalInventoryDate,
                    UpdatedByUserId = _userService.UserId,
                    UnitPrice = itemTypeCreateModel.UnitPrice,
                    No = itemTypeCreateModel.No,
                    Usage = itemTypeCreateModel.Usage,
                    RiskDescription = itemTypeCreateModel.RiskDescription,
                    SalesUnitOfMeasure = itemTypeCreateModel.SalesUnitOfMeasure,
                    EformId = itemTypeCreateModel.EformId,
                    Comment = itemTypeCreateModel.Comment,
                    BaseUnitOfMeasure = itemTypeCreateModel.BaseUnitOfMeasure,
                    ItemGroupId = itemTypeCreateModel.ItemGroupId,
                    StandardCost = itemTypeCreateModel.StandardCost,
                    UnitCost = itemTypeCreateModel.UnitCost,
                };
                await itemType.Create(_dbContext);

                foreach (var itemTypeTag in tags.Select(inventoryTag => new ItemTypeTag
                {
                    CreatedByUserId = _userService.UserId,
                    UpdatedByUserId = _userService.UserId,
                    InventoryTagId = inventoryTag.Id,
                    ItemTypeId = itemType.Id,
                }))
                {
                    await itemTypeTag.Create(_dbContext);
                }

                return new OperationResult(true);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationResult(false,
                    _inventoryLocalizationService.GetString("ErrorWhileCreatingItemType"));
            }
        }

        public async Task<OperationResult> DeleteItemTypeById(int itemTypeId)
        {
            try
            {
                var itemTypesFromDb = await _dbContext.ItemTypes
                    .Where(x => x.Id == itemTypeId)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .FirstOrDefaultAsync();

                if (itemTypesFromDb == null)
                {
                    return new OperationResult(false, _inventoryLocalizationService.GetString("InventoryItemTypeNotFount"));
                }

                var tags = await _dbContext.ItemTypeTags
                    .Where(x => x.ItemTypeId == itemTypeId)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .ToListAsync();

                foreach (var tag in tags)
                {
                    tag.UpdatedByUserId = _userService.UserId;
                    await tag.Delete(_dbContext);
                }

                var typeUploadedDatas = await _dbContext.ItemTypeUploadedDatas
                    .Where(x => x.ItemTypeId == itemTypeId)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .ToListAsync();

                foreach (var uploadedData in typeUploadedDatas)
                {
                    uploadedData.UpdatedByUserId = _userService.UserId;
                    await uploadedData.Delete(_dbContext);
                }

                var items = await _dbContext.Items
                    .Where(x => x.ItemTypeId == itemTypeId)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .ToListAsync();

                foreach (var item in items)
                {
                    item.UpdatedByUserId = _userService.UserId;
                    await item.Delete(_dbContext);
                }

                itemTypesFromDb.UpdatedByUserId = _userService.UserId;
                await itemTypesFromDb.Delete(_dbContext);

                return new OperationResult(true);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationResult(false,
                    _inventoryLocalizationService.GetString("ErrorWhileDeleteInventoryItemType"));
            }
        }

        public async Task<OperationDataResult<List<CommonDictionaryModel>>> GetItemTypesDictionary()
        {
            try
            {
                var inventoryItemTypes = await _dbContext.ItemTypes
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .Select(x => new CommonDictionaryModel
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Id = x.Id,
                    })
                    .ToListAsync();

                return new OperationDataResult<List<CommonDictionaryModel>>(true, inventoryItemTypes);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<List<CommonDictionaryModel>>(false,
                    _inventoryLocalizationService.GetString("ErrorObtainingLists"));
            }
        }

        public async Task<OperationDataResult<ItemTypeModel>> GetItemTypeById(int itemTypeId)
        {
            try
            {
                var inventoryItemTypeQuery = _dbContext.ItemTypes
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Where(x => x.Id == itemTypeId)
                .AsQueryable();

                var inventoryItemTypeFromDb = await AddSelectToItemTypeQueryForFullObject(inventoryItemTypeQuery, _dbContext).FirstAsync();

                if (inventoryItemTypeFromDb == null)
                {
                    return new OperationDataResult<ItemTypeModel>(false,
                        _inventoryLocalizationService.GetString("InventoryItemTypeNotFount"));
                }

                return new OperationDataResult<ItemTypeModel>(true, inventoryItemTypeFromDb);

            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<ItemTypeModel>(false,
                    _inventoryLocalizationService.GetString("ErrorWhileGetInventoryItemType"));
            }
        }

        public async Task<OperationDataResult<Paged<ItemTypeSimpleModel>>> GetItemTypes(
            ItemTypeRequest itemTypeRequest)
        {
            try
            {
                var inventoryItemTypeQuery = _dbContext.ItemTypes
                    .AsQueryable();

                // sort
                if (!string.IsNullOrEmpty(itemTypeRequest.Sort))
                {
                    if (itemTypeRequest.IsSortDsc)
                    {
                        inventoryItemTypeQuery = inventoryItemTypeQuery
                            .CustomOrderByDescending(itemTypeRequest.Sort);
                    }
                    else
                    {
                        inventoryItemTypeQuery = inventoryItemTypeQuery
                            .CustomOrderBy(itemTypeRequest.Sort);
                    }
                }
                else
                {
                    inventoryItemTypeQuery = _dbContext.ItemTypes
                        .OrderBy(x => x.Id);
                }

                // filter by name
                if (!string.IsNullOrEmpty(itemTypeRequest.NameFilter))
                {
                    inventoryItemTypeQuery = inventoryItemTypeQuery
                        .Where(x => x.Name.Contains(itemTypeRequest.NameFilter));
                }

                // filter by tags
                if (itemTypeRequest.TagIds.Any())
                {
                    foreach (var tagId in itemTypeRequest.TagIds)
                    {
                        inventoryItemTypeQuery = inventoryItemTypeQuery
                            .Where(x => x.InventoryTags
                                .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                                .Any(y => y.Id == tagId));
                    }
                }

                inventoryItemTypeQuery = inventoryItemTypeQuery
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed);

                // calculate total before pagination
                var total = inventoryItemTypeQuery.Count();

                // pagination
                inventoryItemTypeQuery
                    = inventoryItemTypeQuery
                        .Skip(itemTypeRequest.Offset)
                        .Take(itemTypeRequest.PageSize);

                // add select and take objects from db
                var inventoryItemTypeFromDb = await AddSelectToItemTypeQuery(inventoryItemTypeQuery, _dbContext, _userService).ToListAsync();
                var returnValue = new Paged<ItemTypeSimpleModel>
                {
                    Entities = inventoryItemTypeFromDb,
                    Total = total,
                };

                return new OperationDataResult<Paged<ItemTypeSimpleModel>>(true, returnValue);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<Paged<ItemTypeSimpleModel>>(false,
                    _inventoryLocalizationService.GetString("ErrorObtainingLists"));
            }
        }

        public async Task<OperationResult> UpdateItemType(ItemTypeUpdateModel model)
        {
            try
            {
                var itemTypesFromDb = await _dbContext.ItemTypes
                    .Where(x => x.Id == model.Id)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .FirstOrDefaultAsync();

                if (itemTypesFromDb == null)
                {
                    return new OperationResult(false, _inventoryLocalizationService.GetString("InventoryItemTypeNotFount"));
                }

                itemTypesFromDb.LastPhysicalInventoryDate = model.LastPhysicalInventoryDate;
                itemTypesFromDb.SalesUnitOfMeasure = model.SalesUnitOfMeasure;
                itemTypesFromDb.BaseUnitOfMeasure = model.BaseUnitOfMeasure;
                itemTypesFromDb.RiskDescription = model.RiskDescription;
                itemTypesFromDb.UpdatedByUserId = _userService.UserId;
                itemTypesFromDb.ProfitPercent = model.ProfitPercent;
                itemTypesFromDb.CostingMethod = model.CostingMethod;
                itemTypesFromDb.StandardCost = model.StandardCost;
                itemTypesFromDb.ItemGroupId = model.ItemGroup?.Id;
                itemTypesFromDb.Description = model.Description;
                itemTypesFromDb.GrossWeight = model.GrossWeight;
                itemTypesFromDb.GtinEanUpc = model.GtinEanUpc;
                itemTypesFromDb.UnitVolume = model.UnitVolume;
                itemTypesFromDb.UnitPrice = model.UnitPrice;
                itemTypesFromDb.NetWeight = model.NetWeight;
                itemTypesFromDb.UnitCost = model.UnitCost;
                itemTypesFromDb.EformId = model.EformId;
                itemTypesFromDb.Comment = model.Comment;
                itemTypesFromDb.Region = model.Region;
                itemTypesFromDb.Usage = model.Usage;
                itemTypesFromDb.Name = model.Name;
                itemTypesFromDb.No = model.No;

                await itemTypesFromDb.Update(_dbContext);

                return new OperationResult(true);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationResult(false,
                    _inventoryLocalizationService.GetString("ErrorWhileUpdateInventoryItemType"));
            }
        }

        private static IQueryable<ItemTypeSimpleModel> AddSelectToItemTypeQuery(IQueryable<ItemType> itemTypeQuery, InventoryPnDbContext dbContext, IUserService userService)
        {
            return itemTypeQuery
                .Select(x => new ItemTypeSimpleModel
                {

                    RiskDescription = x.RiskDescription,
                    Description = x.Description,
                    EformId = x.EformId,
                    Usage = x.Usage,
                    Name = x.Name,
                    Id = x.Id,
                    CreatedDate = x.CreatedAt,
                    CreatedBy = GetUserNameByUserId(x.CreatedByUserId, userService),
                    DangerLabelImageName = dbContext.ItemTypeUploadedDatas
                        .Where(y => y.ItemTypeId == x.Id)
                        .Where(y => y.Type == TypeUploadedData.Danger)
                        .FirstOrDefault(y => y.WorkflowState != Constants.WorkflowStates.Removed).FileName,
                    PictogramImageName = dbContext.ItemTypeUploadedDatas
                        .Where(y => y.ItemTypeId == x.Id)
                        .Where(y => y.Type == TypeUploadedData.Pictogram)
                        .FirstOrDefault(y => y.WorkflowState != Constants.WorkflowStates.Removed).FileName,
                    ParentTypeName = dbContext.ItemTypeDependencys
                        .Where(y => y.ItemTypeId == x.Id)
                        .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                        .Select(y => y.DependItemType.Name)
                        .FirstOrDefault(),
                    Tags = dbContext.ItemTypeTags
                        .Where(y => y.ItemTypeId == x.Id)
                        .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                        .Select(y => new CommonTagModel
                        {
                            Name = y.InventoryTag.Name,
                            Id = y.InventoryTag.Id
                        })
                        .ToList(),
                });
        }

        private static IQueryable<ItemTypeModel> AddSelectToItemTypeQueryForFullObject(IQueryable<ItemType> itemTypeQuery, InventoryPnDbContext dbContext)
        {
            return itemTypeQuery
                 .Select(x => new ItemTypeModel
                 {
                     LastPhysicalInventoryDate = x.LastPhysicalInventoryDate,
                     SalesUnitOfMeasure = x.SalesUnitOfMeasure,
                     BaseUnitOfMeasure = x.BaseUnitOfMeasure,
                     RiskDescription = x.RiskDescription,
                     ProfitPercent = x.ProfitPercent,
                     CostingMethod = x.CostingMethod,
                     StandardCost = x.StandardCost,
                     Description = x.Description,
                     GrossWeight = x.GrossWeight,
                     GtinEanUpc = x.GtinEanUpc,
                     UnitVolume = x.UnitVolume,
                     NetWeight = x.NetWeight,
                     UnitPrice = x.UnitPrice,
                     UnitCost = x.UnitCost,
                     Comment = x.Comment,
                     EformId = x.EformId,
                     Region = x.Region,
                     Usage = x.Usage,
                     Name = x.Name,
                     Id = x.Id,
                     No = x.No,
                     Tags = dbContext.ItemTypeTags
                         .Where(y => y.ItemTypeId == x.Id)
                         .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                         .Select(y => new CommonTagModel
                         {
                             Name = y.InventoryTag.Name,
                             Id = y.InventoryTag.Id
                         })
                         .ToList(),
                     ItemGroupDependency = GetGroupDependencyByItemTypeId(x.Id, dbContext),
                     ItemTypeDependency = dbContext.ItemTypeDependencys
                         .Where(y => y.ItemTypeId == x.Id)
                         .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                         .Select(y => new CommonDictionaryModel
                         {
                             Description = y.DependItemType.Description,
                             Name = y.DependItemType.Name,
                             Id = y.DependItemType.Id,
                         })
                         .ToList(),
                 });
        }

        private static string GetUserNameByUserId(int userId, IUserService _userService)
        {
            var user = _userService.GetByIdAsync(userId).Result;

            return $"{user.FirstName} {user.LastName}";
        }

        private static ItemTypeDependencyItemGroup GetGroupDependencyByItemTypeId(int itemTypeId, InventoryPnDbContext dbContext)
        {
            var itemGroupId = dbContext.ItemGroupDependencys
                .Where(y => y.ItemTypeId == itemTypeId)
                .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(y => y.ItemGroupId).FirstOrDefault();

            return dbContext.ItemGroups
                .Where(z => z.Id == itemGroupId)
                .Select(z => new ItemTypeDependencyItemGroup
                {
                    Name = z.Name,
                    Id = z.Id,
                }).FirstOrDefault();
        }
    }
}
