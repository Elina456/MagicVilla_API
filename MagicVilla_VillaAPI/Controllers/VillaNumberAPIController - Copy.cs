
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        

        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _dbVilla = dbVilla;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            
            try
            {
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
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

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);

                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {

                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);


                }

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaNumberCreateDTO createDTO)
        {
            
            try
            {
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa  Number already exists!");
                    return BadRequest(ModelState);
                }
                if(await _dbVilla.GetAsync(u => u.Id == createDTO.VillaId) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa  Id is invalid");
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                    return BadRequest();
                }
                
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);
                
                await _dbVillaNumber.CreateAsync(villaNumber);
               
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.OK;
                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;

        }



        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    return BadRequest();
                }
                if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa  Id is invalid");
                    return BadRequest(ModelState);
                }
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);
                
               
                await _dbVillaNumber.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
           
        }

        //[HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO)
        //{
        //    if (patchDTO == null || id == 0)
        //    {
        //        return BadRequest();
        //    }
            
        //    var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id, tracked: false);
        //    VillaNumberUpdateDTO villaNumberDTO = _mapper.Map<VillaNumberUpdateDTO>(villaNumber);
            
        //    if (villaNumber == null)
        //    {
        //        return BadRequest();
        //    }
            
        //    patchDTO.ApplyTo(villaNumberDTO, ModelState);
        //    VillaNumber model = _mapper.Map<VillaNumber>(villaNumberDTO);
            
        //    await _dbVillaNumber.UpdateAsync(model);
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    return NoContent();
        //}


    }
}
