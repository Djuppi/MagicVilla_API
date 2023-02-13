using System;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
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
        private readonly ILogging _logger;
		private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAPIController(ILogging logger, ApplicationDbContext db, IMapper mapper)
		{
			_logger = logger;
			_db = db;
			_mapper = mapper;
        }
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
		{

			_logger.Log("Getting all villas", "");
			IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
			return Ok(_mapper.Map<List<VillaDTO>>(villaList));
		}

		[HttpGet("{id:int}", Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesResponseType(200, Type =typeof(VillaDTO))]
		public async Task<ActionResult<VillaDTO>> GetVilla(int id)
		{
			if (id == 0)
			{
                _logger.Log("Get Villa Error with id: " + id, "error");
                return BadRequest();
			}

			var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}

			return Ok(_mapper.Map<VillaDTO>(villa));
		}

		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost]
		public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createdDTO)
		{
			//if(!ModelState.IsValid)
			//{
			//	return BadRequest(ModelState);
			//}
			if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createdDTO.Name.ToLower()) != null)
			{
				ModelState.AddModelError("CustomError", "Villa already exist!");
				return BadRequest(ModelState);
			}
			if (createdDTO == null)
			{
				return BadRequest(createdDTO);
			}

			Villa model = _mapper.Map<Villa>(createdDTO);

			await _db.Villas.AddAsync(model);
			await _db.SaveChangesAsync();

			return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
		}

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpDelete("{id:int}", Name = "DeleteVilla")]
		public async Task<IActionResult> DeleteVilla(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}
			var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}

			_db.Villas.Remove(villa);
			await _db.SaveChangesAsync();

			return NoContent();

		}

		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[HttpPut("{id:int}", Name = "UpdateVilla")]
		public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
		{
			if (updateDTO == null || id != updateDTO.Id)
			{
				return BadRequest();
			}
			Villa model = _mapper.Map<Villa>(updateDTO);

			_db.Villas.Update(model);
			await _db.SaveChangesAsync();


            return NoContent();
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
			var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
			{
				return BadRequest();
			}
			patchDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

			_db.Villas.Update(model);
			await _db.SaveChangesAsync();

            if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			return NoContent();
        }
    }
}

