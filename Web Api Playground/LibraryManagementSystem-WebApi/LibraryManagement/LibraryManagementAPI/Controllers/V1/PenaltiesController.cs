﻿using AutoMapper;
using EmployeeRecordBook.Api.Infrastructure.Specs;
using LibraryManagement.Api.ViewModels;
using LibraryManagement.Core.Contracts.Repositories;
using LibraryManagement.Core.Contracts.Services;
using LibraryManagement.Core.Dtos;
using LibraryManagement.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/bookspenalty")]
    public class PenaltiesController : ApiController
    {
        private readonly IPenaltyService _penaltyService;
        private readonly IMapper _mapper;
        private readonly IPenaltyRepository _penaltyRepository;
        private readonly IIssueRepository _issueRepository;
        private readonly ILogger<PenaltiesController> _logger;

        public PenaltiesController(IPenaltyService penaltyService, IMapper mapper, IPenaltyRepository penaltyRepository, IIssueRepository issueRepository, ILogger<PenaltiesController> logger)
        {
            _penaltyService = penaltyService;
            _mapper = mapper;
            _penaltyRepository = penaltyRepository;
            _issueRepository = issueRepository;
            _logger = logger;
        }

        [HttpPost("pay")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> PayPenalty([FromBody] PayPenaltyVm penaltyVm/*short bookIssuedId, int penaltyAmount*/)
        {
            var existingPenalty = await _penaltyRepository.GetPenaltyByIdAsync(penaltyVm.BookIssuedId);
            var bookIssuedDetails = await _issueRepository.GetBookIssuedByIdAsync(penaltyVm.BookIssuedId);
            _logger.LogInformation($"Paying Penalty with book issued id: {penaltyVm.BookIssuedId}");
            Penalty? isPenalty = _penaltyService.IsPenalty(penaltyVm.BookIssuedId, existingPenalty, bookIssuedDetails);
            if (isPenalty == null)
            {
                return BadRequest("Penalty not found!");
            }
            var isPenaltyExist = await _penaltyRepository.IsPenalty(isPenalty);
            var penaltyPaidStatusDetails = isPenaltyExist != null ? _penaltyService.PayPenalty(penaltyVm.PenaltyAmount, isPenaltyExist) : null;
            if (penaltyPaidStatusDetails != null && penaltyPaidStatusDetails.PenaltyPaidStatus == true)
            {
                var penaltyPaid = _penaltyRepository.PayPenaltyAsync(penaltyPaidStatusDetails);
                _logger.LogInformation($"Paying Penalty with book issued id: {penaltyVm.BookIssuedId}");
                return Ok();
            }
            return NotFound("Transaction Failed");
        }

        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [Authorize(Roles = "Librarian,Director,Principle,Accountant")]
        public async Task<ActionResult> GetPenalties()
        {
            _logger.LogInformation("Getting Penalties}");
            var penalties = await _penaltyRepository.GetPenaltiesAsync();
            if (penalties != null)
            {
                var penaltiesDto = _mapper.Map<IEnumerable<Penalty>, IEnumerable<PenaltyDto>>(penalties);
                return Ok(penaltiesDto);
            }
            return NotFound("Penalties not Found!");
        }

        [HttpGet("{issueId}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [Authorize(Roles = "Librarian,Director,Principle,Accountant")]
        public async Task<ActionResult> GetPenaltiesById(short issueId)
        {
            _logger.LogInformation($"Getting penalty with issue book issue id: {issueId}");
            var penalty = await _penaltyRepository.GetPenaltyByIdAsync(issueId);
            if (penalty != null)
            {
                var penaltyDto = _mapper.Map<Penalty, PenaltyDto>(penalty);
                return Ok(penaltyDto);
            }
            return NotFound("Penalty not Found!");
        }

        [HttpDelete("{issueId}")]
        [ApiConventionMethod(typeof(CustomApiConventions), nameof(CustomApiConventions.Delete))]
        [Authorize(Roles = "Librarian,Director")]
        public async Task<ActionResult> DeletePenalty(short issueId)
        {
            _logger.LogInformation($"Deleting penalty with book issue id: {issueId} ");
            var penaltyToBeDelete = await _penaltyRepository.GetPenaltyByIdAsync(issueId);
            if (penaltyToBeDelete != null)
            {
                var deletedPenalty = await _penaltyRepository.DeletePenaltyAsync(penaltyToBeDelete);
                return NoContent();
            }
            return BadRequest("Penalty not found!");
        }
    }
}