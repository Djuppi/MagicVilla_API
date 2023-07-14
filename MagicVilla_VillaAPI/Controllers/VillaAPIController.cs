using System;
using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
	// [Route("api/[controller]] -- Dynamic endpoint.
	[Route("api/VillaAPI")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		protected APIResponse _response;
		private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
		{
			_dbVilla = dbVilla;
			_mapper = mapper;
			this._response = new();
        }
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<APIResponse>> GetVillas()
		{
			try
			{
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
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
		
		[HttpGet("{id:int}", Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesResponseType(200, Type =typeof(VillaDTO))]
		public async Task<ActionResult<APIResponse>> GetVilla(int id)
		{
			try
			{
                if (id == 0)
                {
                    return BadRequest();
                }

                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }

                _response.Result = _mapper.Map<VillaDTO>(villa);
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
		public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createdDTO)
		{
			//if(!ModelState.IsValid)
			//{
			//	return BadRequest(ModelState);
			//}
			try
			{
				if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createdDTO.Name.ToLower()) != null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa already exist!");
					return BadRequest(ModelState);
				}
				if (createdDTO == null)
				{
					return BadRequest(createdDTO);
				}

				Villa villa = _mapper.Map<Villa>(createdDTO);

				await _dbVilla.CreateAsync(villa);
				_response.Result = _mapper.Map<VillaDTO>(villa);
				_response.StatusCode = HttpStatusCode.Created;

				return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}
            

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpDelete("{id:int}", Name = "DeleteVilla")]
		public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
		{
			try
			{
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }

                await _dbVilla.RemoveAsync(villa);
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

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[HttpPut("{id:int}", Name = "UpdateVilla")]
		public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
		{
			try
			{
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }
                Villa model = _mapper.Map<Villa>(updateDTO);

                await _dbVilla.UpdateAsync(model);
                await _dbVilla.SaveAsync();

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

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]

        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
		{
			if (patchDTO == null || id == 0)
			{
                return BadRequest();
            }
			var villa = await _dbVilla.GetAsync(u => u.Id == id,tracked:false);

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
			{
				return BadRequest();
			}
			patchDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

			await _dbVilla.UpdateAsync(model);
			await _dbVilla.SaveAsync();

            if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			return NoContent();
        }
    }
}

