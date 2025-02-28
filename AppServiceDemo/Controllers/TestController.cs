﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace AppServiceDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        [Route("dumpplayerclaims")]
        public ActionResult<IEnumerable<string>> Get()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            return Ok(new[] 
            { 
                identity.FindFirst(ClaimTypes.NameIdentifier).Value,  
                identity.FindFirst(ClaimTypes.Name).Value
            });
        }
    }
}
