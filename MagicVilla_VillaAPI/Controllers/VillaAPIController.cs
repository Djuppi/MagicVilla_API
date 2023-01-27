﻿using System;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
	// [Route("api/[controller]] -- Dynamic endpoint.
	[Route("api/VillaAPI")]
	[ApiController]
	public class VillaAPIController : ControllerBase
    {
		[HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
		{
			return Ok(VillaStore.villaList);
		}

        [HttpGet("{id:int}", Name ="GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesResponseType(200, Type =typeof(VillaDTO))]
		public ActionResult<VillaDTO> GetVilla(int id)
        {
			if(id == 0)
			{
				return BadRequest();
			}

			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			if(villa==null)
			{
				return NotFound();
			}

            return Ok(villa);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost]
		public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
		{
			//if(!ModelState.IsValid)
			//{
			//	return BadRequest(ModelState);
			//}
			if (VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
			{
				ModelState.AddModelError("CustomError", "Villa already exist!");
				return BadRequest(ModelState);
			}
			if(villaDTO == null)
			{
				return BadRequest(villaDTO);
			}
			if(villaDTO.Id > 0)
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
			villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
			VillaStore.villaList.Add(villaDTO);

			return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
		}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id:int}", Name ="DeleteVilla")]
		public IActionResult DeleteVilla(int id)
		{
			if(id == 0)
			{
				return BadRequest();
			}
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

			VillaStore.villaList.Remove(villa);

			return NoContent();

        }

		[HttpPut("{id:int}", Name = "UpdateVilla")]
        public IActionResult UpdateVilla(int id, [FromBody]VillaDTO villaDTO)
		{
			if(villaDTO == null || id != villaDTO.Id)
			{
				return BadRequest();
			}
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			villa.Name = villaDTO.Name;
			villa.Sqft = villaDTO.Sqft;
			villa.Occupancy = villaDTO.Occupancy;

			return NoContent();
        }
    }
}
