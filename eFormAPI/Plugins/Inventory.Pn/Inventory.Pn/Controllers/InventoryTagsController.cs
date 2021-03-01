﻿/*
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

namespace Inventory.Pn.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure.Models;
    using Infrastructure.Models.Tag;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using Services.InventoryTagsService;

    [Authorize]
    public class InventoryTagsController : Controller
    {
        private readonly IInventoryTagsService _inventoryTagsService;

        public InventoryTagsController(IInventoryTagsService inventoryTagsService)
        {
            _inventoryTagsService = inventoryTagsService;
        }


        /// <summary>
        /// Gets the inventory tags.
        /// </summary>
        /// <returns>Inventory tags.</returns>
        [HttpGet]
        [Route("api/inventory-pn/tags")]
        public async Task<OperationDataResult<List<InventoryTagModel>>> GetInventoryTags()
        {
            return await _inventoryTagsService.GetInventoryTags();
        }

        /// <summary>
        /// Gets the inventory tag by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Task&lt;OperationDataResult&lt;InventoryTagModel&gt;&gt;.</returns>
        [HttpGet]
        [Route("api/inventory-pn/tags/{id}")]
        public async Task<OperationDataResult<InventoryTagModel>> GetInventoryTagById(int id)
        {
            return await _inventoryTagsService.GetInventoryTagById(id);
        }

        /// <summary>
        /// Creates the inventory tag.
        /// </summary>
        /// <param name="requestModel">The request model.</param>
        /// <returns>Task&lt;OperationResult&gt;.</returns>
        [HttpPost]
        [Route("api/inventory-pn/tags")]
        public async Task<OperationResult> CreateInventoryTag([FromBody] InventoryTagModel requestModel)
        {
            return await _inventoryTagsService.CreateInventoryTag(requestModel);
        }

        /// <summary>
        /// Updates the inventory tag.
        /// </summary>
        /// <param name="requestModel">The request model.</param>
        /// <returns>Task&lt;OperationResult&gt;.</returns>
        [HttpPut]
        [Route("api/inventory-pn/tags")]
        public async Task<OperationResult> UpdateInventoryTag([FromBody] InventoryTagModel requestModel)
        {
            return await _inventoryTagsService.UpdateInventoryTag(requestModel);
        }

        /// <summary>
        /// Deletes the inventory tag.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Task&lt;OperationResult&gt;.</returns>
        [HttpDelete]
        [Route("api/inventory-pn/tags/{id}")]
        public async Task<OperationResult> DeleteInventoryTag(int id)
        {
            return await _inventoryTagsService.DeleteInventoryTag(id);
        }
    }
}