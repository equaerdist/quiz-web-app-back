using Amazon.S3;
using Amazon.S3.Model;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Exceptions;

namespace quiz_web_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IAmazonS3 _aws;
        private readonly QuizAppContext _ctx;
        private readonly AppConfig _cfg;
        private string _bucket = "quizwebapp";
        private string _prefix = "--quiz";

        public ImageController(IAmazonS3 aws, QuizAppContext ctx, AppConfig cfg)
        {
            _aws = aws;
            _ctx = ctx;
            _cfg = cfg;
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile imageFile, [FromForm] AccessType type)
        {
            var guid = Guid.NewGuid();
            var image = new Image() { Url = $"{guid + imageFile.FileName}", Id = guid, Mode = type };
            if(type == AccessType.Private)
            {
                if (HttpContext.User is null)
                    throw new BaseQuizAppException("Нельзя создать квиз с приватным доступом без авторизации");
                var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Login == HttpContext.User.Claims.First(c => c.Type == "Login").Value);
                if(user is null)
                    throw new BaseQuizAppException($"Ошибка");
                image.AccessUsers.Add(user);
            }
            var awsConifg = new AmazonS3Config() 
                    { 
                        ServiceURL = _cfg.FileStorage, 
                        AuthenticationRegion = _cfg.Region,
                        LogResponse = true, 
                        LogMetrics = true 
                    };
            awsConifg.RegionEndpoint = Amazon.RegionEndpoint.USEast1;

            var request = new PutObjectRequest()
            {
                BucketName = _bucket,
                InputStream = imageFile.OpenReadStream(),
                ContentType = imageFile.ContentType,
                Key = $"{_prefix}/{image.Url}"
            };

            var response = await _aws.PutObjectAsync(request);
            await _ctx.Images.AddAsync(image);
            await _ctx.SaveChangesAsync();
            return Ok(image.Url);
        }
        [ResponseCache(Duration = 86_400, Location = ResponseCacheLocation.Client)]
        [HttpGet("{path}")]
        public async Task<IActionResult> GetFile(string path)
        {
            var image = await _ctx.Images.FirstOrDefaultAsync(i => i.Url == path).ConfigureAwait(false);
            if (image is null)
                throw new BaseQuizAppException($"Файла с именем {path} не существует");
            if (image.Mode == Core.Models.AccessType.Private)
            {
                if (HttpContext.User is null)
                    return Forbid();
                var user = image.AccessUsers
                    .FirstOrDefault(u => u.Login == HttpContext.User.Claims.First(c => c.Type == "Login").Value);
                if (user is null)
                    return Forbid();
            }
            var request = new GetObjectRequest()
            {
                BucketName = _bucket,
                Key = $"{_prefix}/{image.Url}"
            };
            var response = await _aws.GetObjectAsync(request);
            return File(response.ResponseStream, response.Headers.ContentType);
        }
    }
}
