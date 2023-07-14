using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
	// [Route("api/[controller]] -- Dynamic endpoint.
	[Route("api/VillaNumberAPI")]
	[ApiController]
	public class VillaNumberAPIController : ControllerBase
	{
		protected APIResponse _response;
		private readonly IVillaNumberRepository _dbVillaNumber;
		private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IMapper mapper, IVillaRepository dbVilla)
		{
            _dbVillaNumber = dbVillaNumber;
			_dbVilla = dbVilla;
			_mapper = mapper;
			this._response = new();
        }
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<APIResponse>> GetVillaNumbers()
		{
			try
			{
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties:"Villa");
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;


        }
		
		[HttpGet("{id:int}", Name = "GetVillaNumber")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
		{
			try
			{
                if (id == 0)
                {
                    return BadRequest(_response);
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id, includeProperties:"Villa");
                if (villaNumber == null)
                {
                    return NotFound();
                }

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
		}

		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost]
		public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
		{
			//if(!ModelState.IsValid)
			//{
			//	return BadRequest(ModelState);
			//}
			try
			{
				if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa Number already exist!");
					return BadRequest(ModelState);
				}

				if(await _dbVilla.GetAsync(u => u.Id == createDTO.VillaID) == null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid");
					return BadRequest(ModelState);
				}
				if (createDTO == null)
				{
					return BadRequest(createDTO);
				}

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);

				await _dbVillaNumber.CreateAsync(villaNumber);
				_response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
				_response.StatusCode = HttpStatusCode.Created;

				return CreatedAtRoute("GetVilla", new { id = villaNumber.VillaNo }, _response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}
            

		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
		public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
		{
			try
			{
                if (id == 0)
                {
                    return BadRequest();
                }
                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    return NotFound();
                }

                await _dbVillaNumber.RemoveAsync(villaNumber);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

			return _response;
		}

		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[HttpPut("{id:int}", Name = "UpdateVillaNumber")]
		public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
		{
			try
			{
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    return BadRequest();
                }
                if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaID) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid");
                    return BadRequest(ModelState);
                }
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);

                await _dbVillaNumber.UpdateAsync(model);
                await _dbVillaNumber.SaveAsync();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

			return _response;
		}
    }
}

