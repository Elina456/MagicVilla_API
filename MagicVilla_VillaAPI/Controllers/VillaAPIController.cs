
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        //private readonly ILogger<VillaAPIController> _logger;

        //private readonly ILogging _logger;
        // private readonly ApplicationDbContext _db;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        //public VillaAPIController(ApplicationDbContext db,IMapper mapper)
        //{
        //    _db = db;
        //    _mapper = mapper;
        //}

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
            //_logger.Log("Getting all villas","");
            // _logger.LogInformation("Geetting all villas");

            // return Ok(VillaStore.villaList);
            //return Ok( await _db.Villas.ToListAsync());  
            //IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
            // return Ok(_mapper.Map<List<VillaDTO>>(villaList));
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200,Type = typeof(VillaDTO))]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    //_logger.Log(" Get villa error with Id " + id,"error");
                    // return BadRequest();
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);

                }
                //var Villa = VillaStore.villaList.FirstOrDefault(u => u.Id ==id);
                //var Villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                   // return NotFound();
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);


                }
                //return Ok(Villa);
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
            // return Ok(_mapper.Map<VillaDTO>(Villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            //  if (!ModelState.IsValid)
            // {
            //   return BadRequest(ModelState);
            // }
            // if(VillaStore.villaList.FirstOrDefault(u =>u.Name.ToLower()== villaDTO.Name.ToLower()) != null
            //if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            try
            {
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exists!");
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                    return BadRequest();
                }
                //if(villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}
                Villa villa = _mapper.Map<Villa>(createDTO);
                //Villa model = new()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //   // Id = villaDTO.Id,
                //    ImageUrl = createDTO.ImageUrl,
                //    Name = createDTO.Name,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft
                //};
                //villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id +1;
                //await _db.Villas.AddAsync(model);
                //await _db.SaveChangesAsync();
                
                await _dbVilla.CreateAsync(villa);
                // VillaStore.villaList.Add(villaDTO);

                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                // var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                //var villa =  await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                //VillaStore.villaList.Remove(villa);
                // _db.Villas.Remove(villa);
                //await _db.SaveChangesAsync();
                await _dbVilla.RemoveAsync(villa);
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



        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }
                // var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                //villa.Name = villaDTO.Name;
                //villa.Occupancy = villaDTO.Occupancy;
                //villa.Sqft = villaDTO.Sqft;
                Villa model = _mapper.Map<Villa>(updateDTO);
                //Villa model = new()
                //{
                //    Amenity = updateDTO.Amenity,
                //    Details = updateDTO.Details,
                //    Id = updateDTO.Id,
                //    ImageUrl = updateDTO.ImageUrl,
                //    Name = updateDTO.Name,
                //    Occupancy = updateDTO.Occupancy,
                //    Rate = updateDTO.Rate,
                //    Sqft = updateDTO.Sqft
                //};
                //_db.Villas.Update(model);
                // await _db.SaveChangesAsync();

                await _dbVilla.UpdateAsync(model);
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
            //return NoContent();
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            // var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            //var villa =  await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    Id = villa.Id,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft
            //};
            if (villa == null)
            {
                return BadRequest();
            }
            //patchDTO.ApplyTo(villa,ModelState);
            patchDTO.ApplyTo(villaDTO, ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);
            //Villa model = new()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    Id = villaDTO.Id,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft
            //};
            //_db.Villas.Update(model);
            //await _db.SaveChangesAsync();
            await _dbVilla.UpdateAsync(model);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }


    }
}
