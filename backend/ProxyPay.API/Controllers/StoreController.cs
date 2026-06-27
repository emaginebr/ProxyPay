using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System;
using System.Threading.Tasks;

namespace ProxyPay.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IStoreService _storeService;

        public StoreController(IUserClient userClient, IStoreService storeService)
        {
            _userClient = userClient;
            _storeService = storeService;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] StoreInsertInfo store)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            try
            {
                var result = await _storeService.InsertAsync(store, userSession.UserId);
                return Created($"/store/{result.StoreId}", new { result.StoreId, result.ClientId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] StoreUpdateInfo store)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            try
            {
                await _storeService.UpdateAsync(store, userSession.UserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{storeId}")]
        public async Task<ActionResult> Delete(long storeId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            try
            {
                await _storeService.DeleteAsync(storeId, userSession.UserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
